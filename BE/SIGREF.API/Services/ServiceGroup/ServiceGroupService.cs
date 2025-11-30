#nullable enable
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using FhirList = Hl7.Fhir.Model.List;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.ServiceGroup;

public class ServiceGroupService(FhirClient fhirService)
{
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

                dto.HealthcareService = activeServices;

                var locationIds = list.Entry
                    .Where(e => e.Item?.Reference?.StartsWith("Location/") == true)
                    .Select(e => e.Item.Reference.Split('/').Last())
                    .ToList();

                // Mapear a DTO simplificado
                dto.Locations = [.. locations
                    .Where(l => locationIds.Contains(l.Id))
                    .Select(l => l.ToSimplifiedDto())];  // DTO simplificado

                // Calcular precio total (por ahora será 0, se actualizará cuando se integre Postgres)
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

    public async Task<FhirList> CreateServiceGroupAsync(FhirList list)
    {
        list.Meta = new Meta
        {
            LastUpdated = DateTimeOffset.Now,
            VersionId = "1"
        };
        return await fhirService.CreateAsync(list);
    }

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

    public async Task DeleteServiceGroupAsync(string id)
    {
        await fhirService.DeleteAsync($"List/{id}");
    }

    // Helper to get raw FhirList for update
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
}