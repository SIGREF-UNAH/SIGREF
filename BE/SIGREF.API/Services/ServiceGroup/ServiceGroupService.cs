#nullable enable
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Exceptions;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Middleware;
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
    : BaseFhirService(userContext, ns), IHealthcareGroupService
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
        // Validación de seguridad (Fail Fast)
        if (filter.PageSize > 500)
        {
            throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
            {
                { "Field", nameof(filter.PageSize) },
                { "MaxAllowed", 500 },
                { "AttemptedValue", filter.PageSize }
            });
        }
        
        var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);
        var searchParams = BuildSearchParams(filter, pageSize, offset);

        Bundle? bundle;

        // Operación externa (FHIR) controlada
        try
        {
            bundle = await fhirClient.SearchAsync<FhirList>(searchParams);
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, resourceId: "List/ServiceGroup", action: "SEARCH_FILTERED_GROUPS");
        }

        // Una sola pasada O(n) sobre bundle.Entry en vez de tres iteraciones separadas
        var lists     = new List<FhirList>();
        var services  = new List<HealthcareService>();
        var locations = new List<Hl7.Fhir.Model.Location>();

        foreach (var entry in bundle?.Entry ?? [])
        {
            switch (entry.Resource)
            {
                case FhirList fl:                   lists.Add(fl);      break;
                case HealthcareService hs:          services.Add(hs);   break;
                case Hl7.Fhir.Model.Location loc:  locations.Add(loc); break;
            }
        }
        
        var serviceIdsInBundle = services
            .Where(s => !string.IsNullOrEmpty(s.Id))
            .Select(s => s.Id!)
            .Distinct()
            .ToList();

        var priceMap = await BuildPriceMapAsync(serviceIdsInBundle);

        // Mapeo y enriquecimiento en memoria (capacidad prealoca para evitar redimensionamientos)
        var items = new List<ServiceGroupDto>(lists.Count);
        foreach (var list in lists)
        {
            items.Add(MapAndEnrich(list, services, locations, priceMap));
        }

        // Construcción de respuesta
        var pagination = FhirPaginationHelper.BuildPagination(
            bundle?.Total ?? 0,
            pageNumber,
            pageSize,
            hasNextLink: bundle?.NextLink != null);

        return new PagedResultDto<ServiceGroupDto>
        {
            Items = items,
            Pagination = pagination
        };
    }

    // -------------------------------------------------------------------------
    // READ — por ID
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retorna un único grupo de atención por su ID FHIR,
    /// o lanza <see cref="NotFoundException"/> si no existe.
    /// </summary>
    public async Task<ServiceGroupDto> GetByIdAsync(string id)
    {
        var searchParams = new SearchParams()
            .Add("_id", id)
            .Add("_include", "List:item")
            .Add("_elements:Location", "id,name")
            .Add("_elements:HealthcareService", "id,name,active");

        Bundle bundle;

        // Operación externa (FHIR) controlada
        try
        {
            bundle = await fhirClient.SearchAsync<FhirList>(searchParams);
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, resourceId: id, action: "GET_GROUP_BY_ID");
        }

        // Una sola pasada O(n) sobre bundle.Entry
        FhirList? list = null;
        var services  = new List<HealthcareService>();
        var locations = new List<Hl7.Fhir.Model.Location>();

        foreach (var entry in bundle.Entry ?? [])
        {
            switch (entry.Resource)
            {
                case FhirList fl:                   list ??= fl;        break;
                case HealthcareService hs:          services.Add(hs);   break;
                case Hl7.Fhir.Model.Location loc:  locations.Add(loc); break;
            }
        }

        // Fail Fast: pedimos por un ID explícito y no está
        if (list is null)
        {
            throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
            {
                { "ResourceId", id },
                { "ResourceType", "ServiceGroup (FhirList)" }
            });
        }

        // lógica de priceMap 
        var serviceIdsInBundle = services
            .Where(s => !string.IsNullOrEmpty(s.Id))
            .Select(s => s.Id!)
            .Distinct()
            .ToList();

        var priceMap = await BuildPriceMapAsync(serviceIdsInBundle);

        return MapAndEnrich(list, services, locations, priceMap);
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
        // Validación temprana (Fail-fast)
        ArgumentNullException.ThrowIfNull(careGroup);
        
        ApplyMeta(careGroup, isCreate: true);

        //  Operación externa (FHIR) controlada
        try
        {
            return await fhirClient.CreateAsync(careGroup);
        }
        catch (FhirOperationException ex)
        {
            // En un Create, el Id suele ser nulo antes de enviarse.
            // Usamos un fallback descriptivo para que el log del Mapper tenga contexto.
            var identifier = careGroup.Id ?? "New ServiceGroup (FhirList)";
            throw FhirExceptionMapper.Map(ex, resourceId: identifier, action: "CREATE_GROUP");
        }
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
        // Validaciones tempranas (Fail-fast)
        ArgumentNullException.ThrowIfNull(list);

        if (string.IsNullOrWhiteSpace(list.Id))
        {
            throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
            {
                { "Field", nameof(list.Id) },
                { "Message", "El ID del recurso es obligatorio para realizar una actualización." },
                { "ResourceType", "ServiceGroup (FhirList)" }
            });
        }
        
        ApplyMeta(list, isCreate: false);

        // Operación externa (FHIR) controlada
        try
        {
            return await fhirClient.UpdateAsync(list);
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, resourceId: list.Id, action: "UPDATE_GROUP");
        }
    }

    // -------------------------------------------------------------------------
    // DELETE
    // -------------------------------------------------------------------------

    /// <summary>Elimina un grupo de atención del servidor FHIR por su ID.</summary>
    public async Task DeleteAsync(string id)
    {
        // Validación temprana (Fail-fast)
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
            {
                { "Field", nameof(id) },
                { "Message", "El ID del recurso es obligatorio para realizar la eliminación." },
                { "ResourceType", "ServiceGroup (FhirList)" }
            });
        }

        // Operación externa (FHIR) controlada
        try
        {
            await fhirClient.DeleteAsync($"List/{id}");
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, resourceId: id, action: "DELETE_GROUP");
        }
    }

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

        sp.Add("_include", "List:item");
        sp.Add("_elements:Location", "id,name");
        sp.Add("_elements:HealthcareService", "id,name,active");
        sp.Count = pageSize;
        sp.Add("_offset", offset.ToString());
        sp.Add("_total", "accurate");

        return sp;
    }

    /// <summary>
    /// Consulta única a PostgreSQL para obtener precios por FHIR IDs.
    /// Centraliza la lógica evitando duplicación entre GetFilteredAsync y GetByIdAsync.
    /// Retorna diccionario vacío si no hay IDs que consultar.
    /// </summary>
    private async System.Threading.Tasks.Task<Dictionary<string, decimal>> BuildPriceMapAsync(
        List<string> fhirIds)
    {
        if (fhirIds.Count == 0) return [];

        return await dbContext.HealthServices
            .Where(hs => fhirIds.Contains(hs.HealthServiceFhirId))
            .Select(hs => new { hs.HealthServiceFhirId, hs.Price })
            .ToDictionaryAsync(x => x.HealthServiceFhirId, x => x.Price);
    }

    /// <summary>
    /// Extrae el ID del segmento final de una referencia FHIR sin alocación de array.
    /// Usa Span para evitar Split('/') en bucles calientes.
    /// </summary>
    private static string ExtractIdFromReference(string reference)
    {
        var idx = reference.LastIndexOf('/');
        return idx >= 0 ? reference[(idx + 1)..] : reference;
    }

    /// <summary>
    /// Mapea un recurso <c>FhirList</c> a <see cref="ServiceGroupDto"/> y lo
    /// enriquece con servicios activos (con precios desde PostgreSQL) y ubicaciones.
    /// </summary>
    /// TODO : EXPLICACION DEL CONTEXTO DE POR QUE SE USA HALTCARESERVICES
    /// TODO : Metodo Creado bajo presion no explicado completamente paso a paso 
    private ServiceGroupDto MapAndEnrich(
        FhirList list,
        List<HealthcareService> services,
        List<Hl7.Fhir.Model.Location> locations,
        Dictionary<string, decimal> priceMap)
    {
        var dto = list.ToDto();

        if (list.Entry is null) return dto;
        
        // --- Servicios activos ---
        var serviceIds = list.Entry
            .Where(e => !string.IsNullOrEmpty(e.Item?.Reference)
                        && e.Item.Reference.StartsWith("HealthcareService/", StringComparison.Ordinal))
            .Select(e => ExtractIdFromReference(e.Item.Reference))
            .ToHashSet();

        var activeServices = services
            .Where(s => serviceIds.Contains(s.Id) && s.Active == true)
            .Select(s =>
            {
                var sDto = s.ToSimplifiedDto();
                if (sDto.Id != null && priceMap.TryGetValue(sDto.Id, out var price))
                    sDto.Price = price;
                return sDto;
            })
            .ToList();

        dto.HealthcareService = activeServices;

        // --- Ubicaciones ---
        var locationIds = list.Entry
            .Where(e => !string.IsNullOrEmpty(e.Item?.Reference)
                        && e.Item.Reference.StartsWith("Location/", StringComparison.Ordinal))
            .Select(e => ExtractIdFromReference(e.Item.Reference))
            .ToHashSet();

        dto.Locations =
        [
            .. locations
                .Where(l => locationIds.Contains(l.Id))
                .Select(l => l.ToSimplifiedDto())
        ];

        // --- Precio total ---
        dto.TotalPrice = activeServices.Sum(s => s.Price ?? 0);

        return dto;
    }
}