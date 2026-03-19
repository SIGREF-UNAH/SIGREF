#nullable enable
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Persistence;
using FhirList = Hl7.Fhir.Model.List;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.ServiceGroup;

/// <summary>
/// Servicio para gestionar la lógica de negocio relacionada con los Grupos de Servicios (Service Groups),
/// interactuando con el servidor FHIR (para List, HealthcareService, Location) y PostgreSQL (para precios).
/// </summary>
public class ServiceGroupService(FhirClient fhirService, SIGREFContext dbContext)
{
    /// <summary>
    /// Obtiene una lista paginada y filtrada de Grupos de Servicios desde FHIR.
    /// </summary>
    /// <param name="filter">Objeto que contiene los criterios de filtrado y paginación.</param>
    /// <returns>Una tupla que contiene la lista de DTOs de Grupos de Servicios y la información de paginación.</returns>
    public async Task<(List<ServiceGroupDto>, PaginationDto)> GetFilteredServiceGroupsAsync(ServiceGroupFilterDto filter)
    {
        var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);
        var searchParams = new SearchParams();

        if (!string.IsNullOrWhiteSpace(filter.Title))
            searchParams.Add("title", filter.Title);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            searchParams.Add("status", filter.Status);

        // Filtrar por ubicación (ahora está en entries, no en extensions)
        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            searchParams.Add("item", $"Location/{filter.Location}");
        }

        // Incluir todos los recursos referenciados (HealthcareService y Location) en una sola consulta
        searchParams.Add("_include", "List:item");

        // Optimización: Solo traer las propiedades que necesitamos de cada recurso
        searchParams.Add("_elements:Location", "id,name");
        searchParams.Add("_elements:HealthcareService", "id,name,active");

        searchParams.Count = pageSize;
        searchParams.Add("_offset", offset.ToString());
        searchParams.Add("_total", "accurate");

        var bundle = await fhirService.SearchAsync<FhirList>(searchParams);

        // Extraer recursos del bundle
        var lists = bundle.Entry?.Select(e => e.Resource).OfType<FhirList>().ToList() ?? [];
        var services = bundle.Entry?.Select(e => e.Resource).OfType<Hl7.Fhir.Model.HealthcareService>().ToList() ?? [];
        var locations = bundle.Entry?.Select(e => e.Resource).OfType<Hl7.Fhir.Model.Location>().ToList() ?? [];

        // Mapear y enriquecer cada ServiceGroup
        var dtos = new List<ServiceGroupDto>();
        foreach (var list in lists)
        {
            var dto = list.ToDto(); // Mapeo base

            // Enriquecer con objetos simplificados
            if (list.Entry != null)
            {
                var serviceIds = list.Entry
                    .Where(e => e.Item?.Reference?.StartsWith("HealthcareService/") == true)
                    .Select(e => e.Item.Reference.Split('/').Last())
                    .ToList();

                // Filtrar solo services activos y mapear a DTO simplificado
                var activeServices = services
                    .Where(s => serviceIds.Contains(s.Id) && s.Active == true)  // Solo activos
                    .Select(s => s.ToSimplifiedDto())  // DTO simplificado
                    .ToList();

                // Enriquecer con precios desde PostgreSQL
                await EnrichServicesWithPricesAsync(activeServices);

                dto.HealthcareService = activeServices;

                var locationIds = list.Entry
                    .Where(e => e.Item?.Reference?.StartsWith("Location/") == true)
                    .Select(e => e.Item.Reference.Split('/').Last())
                    .ToList();

                // Mapear a DTO simplificado
                dto.Locations = [.. locations
                    .Where(l => locationIds.Contains(l.Id))
                    .Select(l => l.ToSimplifiedDto())];  // DTO simplificado

                // Calcular precio total
                dto.TotalPrice = activeServices.Sum(s => s.Price ?? 0);
            }

            dtos.Add(dto);
        }

        var pagination = FhirPaginationHelper.BuildPagination(
            bundle.Total ?? 0,
            pageNumber,
            pageSize,
            hasNextLink: bundle.NextLink != null
        );

        return (dtos, pagination);
    }

    /// <summary>
    /// Obtiene un Grupo de Servicios específico por su ID de FHIR List.
    /// </summary>
    /// <param name="id">El ID del recurso List en FHIR.</param>
    /// <returns>El DTO del Grupo de Servicios o null si no se encuentra.</returns>
    public async Task<ServiceGroupDto?> GetServiceGroupByIdAsync(string id)
    {
        // 1. Query única: Busca la lista por ID e incluye TODOS los items (HealthcareService y Location) en la misma respuesta
        var searchParams = new SearchParams()
            .Add("_id", id)
            .Add("_include", "List:item")
            .Add("_elements:Location", "id,name")
            .Add("_elements:HealthcareService", "id,name,active");

        var bundle = await fhirService.SearchAsync<FhirList>(searchParams);

        // 2. Extraer la List (el recurso principal)
        var list = bundle.Entry.Select(e => e.Resource).OfType<FhirList>().FirstOrDefault();
        if (list == null) return null;

        var dto = list.ToDto();

        // 3. Extraer HealthcareService y Location del Bundle (ya vienen incluidos, sin consultas extra)
        var services = bundle.Entry.Select(e => e.Resource).OfType<Hl7.Fhir.Model.HealthcareService>().ToList();
        var locations = bundle.Entry.Select(e => e.Resource).OfType<Hl7.Fhir.Model.Location>().ToList();

        // 4. Mapear servicios de salud desde los entries de la lista
        if (list.Entry != null)
        {
            var serviceIds = list.Entry
                .Where(entry => entry.Item?.Reference?.StartsWith("HealthcareService/") == true)
                .Select(entry => entry.Item.Reference.Split('/').Last())
                .ToList();

            // Filtrar solo services activos y mapear a DTO simplificado
            var activeServiceDtos = services
                .Where(svc => serviceIds.Contains(svc.Id) && svc.Active == true)  // Solo activos
                .Select(svc => svc.ToSimplifiedDto())  // DTO simplificado
                .ToList();

            // Enriquecer con precios desde PostgreSQL
            await EnrichServicesWithPricesAsync(activeServiceDtos);

            dto.HealthcareService = activeServiceDtos;

            // 5. Mapear ubicaciones desde los entries de la lista
            var locationIds = list.Entry
                .Where(entry => entry.Item?.Reference?.StartsWith("Location/") == true)
                .Select(entry => entry.Item.Reference.Split('/').Last())
                .ToList();

            dto.Locations = locations
                .Where(loc => locationIds.Contains(loc.Id))
                .Select(loc => loc.ToSimplifiedDto())  // DTO simplificado
                .ToList();

            // Calcular precio total
            dto.TotalPrice = activeServiceDtos.Sum(s => s.Price ?? 0);
        }

        return dto;
    }

    /// <summary>
    /// Crea un nuevo Grupo de Servicios en el servidor FHIR.
    /// </summary>
    /// <param name="list">El recurso FHIR List a crear.</param>
    /// <returns>El recurso FHIR List creado, incluyendo metadatos actualizados.</returns>
    public async Task<FhirList> CreateServiceGroupAsync(FhirList list)
    {
        list.Meta = new Meta
        {
            LastUpdated = DateTimeOffset.Now,
            VersionId = "1"
        };
        return await fhirService.CreateAsync(list);
    }

    /// <summary>
    /// Actualiza un Grupo de Servicios existente en el servidor FHIR, incrementando la versión.
    /// </summary>
    /// <param name="list">El recurso FHIR List con los datos actualizados.</param>
    /// <returns>El recurso FHIR List actualizado.</returns>
    public async Task<FhirList> UpdateServiceGroupAsync(FhirList list)
    {
        if (list.Meta == null) list.Meta = new Meta();
        list.Meta.LastUpdated = DateTimeOffset.Now;

        if (int.TryParse(list.Meta.VersionId, out var currentVersion))
            list.Meta.VersionId = (currentVersion + 1).ToString();
        else
            list.Meta.VersionId = "1";

        return await fhirService.UpdateAsync(list);
    }

    /// <summary>
    /// Elimina un Grupo de Servicios por su ID en FHIR.
    /// </summary>
    /// <param name="id">El ID del recurso List a eliminar.</param>
    public async Task DeleteServiceGroupAsync(string id)
    {
        await fhirService.DeleteAsync($"List/{id}");
    }

    /// <summary>
    /// Obtiene el recurso FHIR List crudo por ID. Útil para obtener la versión actual antes de una actualización.
    /// </summary>
    /// <param name="id">El ID del recurso List.</param>
    /// <returns>El recurso FHIR List o null si no se encuentra.</returns>
    public async Task<FhirList?> GetFhirListByIdAsync(string id)
    {
        try
        {
            return await fhirService.ReadAsync<FhirList>($"List/{id}");
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Obtiene los precios de los servicios desde la base de datos PostgreSQL y los asigna a los DTOs
    /// </summary>
    /// <param name="serviceDtos">Lista de DTOs de servicios de salud a enriquecer.</param>
    private async Task EnrichServicesWithPricesAsync(List<ServiceGroupHealthcareDto> serviceDtos)
    {
        if (serviceDtos == null || serviceDtos.Count == 0) return;

        // Obtener los IDs de FHIR de los servicios
        var fhirServiceIds = serviceDtos.Select(s => s.Id).Where(id => !string.IsNullOrEmpty(id)).ToList();
        
        if (fhirServiceIds.Count == 0) return;

        // Consultar la base de datos para obtener los precios
        var servicePrices = await dbContext.HealthServices
            .Where(hs => fhirServiceIds.Contains(hs.HealthServiceFhirId))
            .Select(hs => new { HealthServiceIdFHIR = hs.HealthServiceFhirId, hs.Price })
            .ToDictionaryAsync(x => x.HealthServiceIdFHIR, x => x.Price);

        // Asignar los precios a los DTOs
        foreach (var serviceDto in serviceDtos)
        {
            if (!string.IsNullOrEmpty(serviceDto.Id) && servicePrices.TryGetValue(serviceDto.Id, out var price))
            {
                serviceDto.Price = price;
            }
        }
    }
}