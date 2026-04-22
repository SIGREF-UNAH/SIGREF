#nullable enable
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;
using FhirList = Hl7.Fhir.Model.List;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.ServiceGroup;
/// <summary>
/// Servicio para la gestión de grupos de atención (HealthcareGroup),
/// modelados como recursos FHIR <c>List</c> que agrupan <c>HealthcareService</c>
/// y <c>Location</c>. Hereda trazabilidad y auditoría de <see cref="BaseFhirService"/>.
/// </summary>
public class HealthcareGroupService(
    FhirClient fhirClient,
    SIGREFContext dbContext,
    IUserContextService userContext,
    IFhirNamespaceService ns)
    : BaseFhirService(userContext, ns) , IHealthcareGroupService
{
    // -------------------------------------------------------------------------
    // READ — paginado
    // -------------------------------------------------------------------------
 
    /// <summary>
    /// Retorna los grupos de atención que coincidan con los filtros indicados,
    /// paginados y envueltos en <see cref="PagedResultDto{T}"/>.
    /// </summary>
    public async Task<PagedResultDto<ServiceGroupDto>> GetFilteredAsync(ServiceGroupFilterDto filter)
    {
        var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);
 
        var searchParams = BuildSearchParams(filter, pageSize, offset);
        var bundle = await fhirClient.SearchAsync<FhirList>(searchParams);
 
        // --- Extraer recursos del bundle ---
        var lists     = bundle.Entry?.Select(e => e.Resource).OfType<FhirList>().ToList()                         ?? [];
        var services  = bundle.Entry?.Select(e => e.Resource).OfType<HealthcareService>().ToList()                ?? [];
        var locations = bundle.Entry?.Select(e => e.Resource).OfType<Hl7.Fhir.Model.Location>().ToList()         ?? [];
 
        // --- Mapear y enriquecer cada grupo ---
        var items = new List<ServiceGroupDto>(lists.Count);
        foreach (var list in lists)
        {
            items.Add(await MapAndEnrichAsync(list, services, locations));
        }
 
        var pagination = FhirPaginationHelper.BuildPagination(
            bundle.Total ?? 0,
            pageNumber,
            pageSize,
            hasNextLink: bundle.NextLink != null);
 
        return new PagedResultDto<ServiceGroupDto>
        {
            Items      = items,
            Pagination = pagination
        };
    }
 
    // -------------------------------------------------------------------------
    // READ — por ID
    // -------------------------------------------------------------------------
 
    /// <summary>
    /// Retorna un único grupo de atención por su ID FHIR,
    /// o <c>null</c> si no existe.
    /// </summary>
    public async Task<ServiceGroupDto?> GetByIdAsync(string id)
    {
        var searchParams = new SearchParams()
            .Add("_id",      id)
            .Add("_include", "List:item")
            .Add("_elements:Location",         "id,name")
            .Add("_elements:HealthcareService", "id,name,active");
 
        var bundle = await fhirClient.SearchAsync<FhirList>(searchParams);
 
        var list = bundle.Entry?.Select(e => e.Resource).OfType<FhirList>().FirstOrDefault();
        if (list is null) return null;
 
        var services  = bundle.Entry!.Select(e => e.Resource).OfType<HealthcareService>().ToList();
        var locations = bundle.Entry!.Select(e => e.Resource).OfType<Hl7.Fhir.Model.Location>().ToList();
 
        return await MapAndEnrichAsync(list, services, locations);
    }
 
    // -------------------------------------------------------------------------
    // CREATE
    // -------------------------------------------------------------------------
 
    /// <summary>
    /// Crea un nuevo grupo de atención en el servidor FHIR.
    /// Aplica los metadatos de trazabilidad definidos en <see cref="BaseFhirService"/>.
    /// </summary>
    /// <param name="careGroup">El recurso <see cref="FhirList"/> que representa el grupo de atención a crear.</param>
    /// <returns>El recurso <see cref="FhirList"/> creado y devuelto por el servidor FHIR.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="careGroup"/> es nulo.</exception>
    /// <exception cref="FhirOperationException">Se lanza si ocurre un error en la comunicación con el servidor FHIR.</exception>
    public async Task<FhirList> CreateAsync(FhirList careGroup)
    {
        // 1. Validación temprana (Fail-fast)
        ArgumentNullException.ThrowIfNull(careGroup);

        // 2. Aplicación de lógica de negocio
        ApplyMeta(careGroup, isCreate: true);

        // 3. Llamada asíncrona optimizada
        return await fhirClient.CreateAsync(careGroup);
    }
 
    // -------------------------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------------------------
 
    /// <summary>
    /// Actualiza un grupo de atención existente en el servidor FHIR.
    /// Incrementa automáticamente el <c>VersionId</c> y refresca <c>LastUpdated</c>.
    /// </summary>
    public async Task<FhirList> UpdateAsync(FhirList list)
    {
        ApplyMeta(list, isCreate:false);
        return await fhirClient.UpdateAsync(list);
    }
 
    // -------------------------------------------------------------------------
    // DELETE
    // -------------------------------------------------------------------------
 
    /// <summary>Elimina un grupo de atención del servidor FHIR por su ID.</summary>
    public async System.Threading.Tasks.Task DeleteAsync(string id)
        => await fhirClient.DeleteAsync($"List/{id}");
 
    // -------------------------------------------------------------------------
    // HELPER — obtener recurso raw para operaciones de escritura
    // -------------------------------------------------------------------------
 
    /// <summary>
    /// Recupera el recurso <c>FhirList</c> crudo desde FHIR para usarlo en
    /// operaciones de actualización. Retorna <c>null</c> si no existe.
    /// </summary>
    public async Task<FhirList?> GetFhirListByIdAsync(string id)
    {
        try
        {
            return await fhirClient.ReadAsync<FhirList>($"List/{id}");
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
 
    // =========================================================================
    // MÉTODOS PRIVADOS
    // =========================================================================
 
    /// <summary>
    /// Construye los <see cref="SearchParams"/> para la consulta paginada y filtrada.
    /// </summary>
    private static SearchParams BuildSearchParams(ServiceGroupFilterDto filter, int pageSize, int offset)
    {
        var sp = new SearchParams();
 
        if (!string.IsNullOrWhiteSpace(filter.Title))
            sp.Add("title", filter.Title);
 
        if (!string.IsNullOrWhiteSpace(filter.Status))
            sp.Add("status", filter.Status);
 
        if (!string.IsNullOrWhiteSpace(filter.Location))
            sp.Add("item", $"Location/{filter.Location}");
 
        sp.Add("_include",                    "List:item");
        sp.Add("_elements:Location",           "id,name");
        sp.Add("_elements:HealthcareService",  "id,name,active");
        sp.Count = pageSize;
        sp.Add("_offset", offset.ToString());
        sp.Add("_total",  "accurate");
 
        return sp;
    }
 
    /// <summary>
    /// Mapea un recurso <c>FhirList</c> a <see cref="ServiceGroupDto"/> y lo
    /// enriquece con servicios activos (con precios desde PostgreSQL) y ubicaciones.
    /// </summary>
    private async Task<ServiceGroupDto> MapAndEnrichAsync(
        FhirList list,
        List<HealthcareService> services,
        List<Hl7.Fhir.Model.Location> locations)
    {
        var dto = list.ToDto();
 
        if (list.Entry is null) return dto;
 
        // --- Servicios activos ---
        var serviceIds = list.Entry
            .Where(e => e.Item?.Reference?.StartsWith("HealthcareService/") == true)
            .Select(e => e.Item.Reference.Split('/').Last())
            .ToList();
 
        var activeServices = services
            .Where(s => serviceIds.Contains(s.Id) && s.Active == true)
            .Select(s => s.ToSimplifiedDto())
            .ToList();
 
        await EnrichServicesWithPricesAsync(activeServices);
 
        dto.HealthcareService = activeServices;
 
        // --- Ubicaciones ---
        var locationIds = list.Entry
            .Where(e => e.Item?.Reference?.StartsWith("Location/") == true)
            .Select(e => e.Item.Reference.Split('/').Last())
            .ToList();
 
        dto.Locations = [.. locations
            .Where(l => locationIds.Contains(l.Id))
            .Select(l => l.ToSimplifiedDto())];
 
        // --- Precio total ---
        dto.TotalPrice = activeServices.Sum(s => s.Price ?? 0);
 
        return dto;
    }
 
    /// <summary>
    /// Consulta PostgreSQL para obtener los precios de los servicios y
    /// los asigna a sus respectivos DTOs.
    /// </summary>
    private async System.Threading.Tasks.Task EnrichServicesWithPricesAsync(
        List<ServiceGroupHealthcareDto> serviceDtos)
    {
        if (serviceDtos is null || serviceDtos.Count == 0) return;
 
        var fhirIds = serviceDtos
            .Select(s => s.Id)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
 
        if (fhirIds.Count == 0) return;
 
        var priceMap = await dbContext.HealthServices
            .Where(hs => fhirIds.Contains(hs.HealthServiceFhirId))
            .Select(hs => new { hs.HealthServiceFhirId, hs.Price })
            .ToDictionaryAsync(x => x.HealthServiceFhirId, x => x.Price);
 
        foreach (var dto in serviceDtos)
        {
            if (!string.IsNullOrEmpty(dto.Id) && priceMap.TryGetValue(dto.Id, out var price))
                dto.Price = price;
        }
    }
    
}