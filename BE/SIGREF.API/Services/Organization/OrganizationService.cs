using System.Runtime.Serialization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Exceptions;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Middleware;
using SIGREF.API.Services.Organization;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Organizations;

public class OrganizationService : BaseFhirService, IOrganizationService
{
    private readonly FhirClient _fhirClient;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(FhirClient fhirClient, ILogger<OrganizationService> logger,
        IUserContextService userContext,
        IFhirNamespaceService ns)
        : base(userContext, ns)
    {
        _fhirClient = fhirClient;
        _logger = logger;
    }

    public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto)
    {
        try
        {
            // Transformación a Entidad FHIR y Metadatos
            var organization = dto.ToFhirResource();
            ApplyMeta(organization, isCreate: true);
            var result = await _fhirClient.CreateAsync(organization);

            return result.ToDto();
        }
        catch (FhirOperationException ex)
        {
            // Mapeo centralizado de errores (Conflictos de nombre, validaciones de esquema, etc.)
            throw FhirExceptionMapper.Map(ex, "NEW_ORGANIZATION", "CREATE_ORGANIZATION");
        }
    }

    public async Task<OrganizationDto> GetOrganizationByIdAsync(string id)
    {
        try
        {
            // Intentar obtener el recurso con el tipo calificado
            // Usamos el operador ?? throw para aplicar Fail Fast si el recurso es null
            var organization = await _fhirClient.ReadAsync<Hl7.Fhir.Model.Organization>($"Organization/{id}")
                               ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                               {
                                   { "ResourceId", id },
                                   { "ResourceType", "Organization" }
                               });

            // Mapeo a DTO
            return organization.ToDto();
        }
        catch (FhirOperationException ex)
        {
            // Centralización total con el Mapper
            throw FhirExceptionMapper.Map(ex, id, "GET_ORGANIZATION_BY_ID");
        }
    }

    public async Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationDto dto)
    {
        try
        {
            //  Leer recurso existente (Fail Fast)
            var existing = await _fhirClient.ReadAsync<Hl7.Fhir.Model.Organization>($"Organization/{id}")
                           ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                           {
                               { "ResourceId", id },
                               { "ResourceType", "Organization" }
                           });

            // Aplicar actualizaciones y metadatos
            var updated = dto.UpdateFhirResource(existing);
            ApplyMeta(updated, isCreate: false);

            // Enviar actualización al servidor FHIR
            var result = await _fhirClient.UpdateAsync(updated);
            return result.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "UPDATE_ORGANIZATION");
        }
    }

    public async Task DeleteOrganizationAsync(string id)
    {
        try
        {
            // Verificación previa (Fail Fast)
            _ = await _fhirClient.ReadAsync<Hl7.Fhir.Model.Organization>($"Organization/{id}")
                ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                {
                    { "ResourceId", id },
                    { "ResourceType", "Organization" }
                });

            // Ejecutar el borrado físico
            await _fhirClient.DeleteAsync($"Organization/{id}");
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "DELETE_ORGANIZATION");
        }
    }

    // Filtros
    public async Task<PagedResultDto<OrganizationDto>> GetFilteredOrganizationsAsync(OrganizationFilterDto filter)
    {
        try
        {
            // 1. Validación de seguridad (Fail Fast)
            if (filter.PageSize > 500)
            {
                throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
                {
                    { "Field", "PageSize" },
                    { "MaxAllowed", 500 }
                });
            }

            // 2. Normalizar paginación y preparar búsqueda
            var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);
            var searchParams = new SearchParams();

            // 3. Construcción de Filtros
            if (!string.IsNullOrWhiteSpace(filter.Name))
                searchParams.Add("name:contains", filter.Name.Trim());

            if (filter.Active.HasValue)
                searchParams.Add("active", filter.Active.Value.ToString().ToLowerInvariant());

            if (filter.Type != null && filter.Type.Any())
            {
                foreach (var type in filter.Type)
                {
                    var typeValue = GetEnumMemberValue(type);
                    searchParams.Add("type", typeValue);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.PartOf))
                searchParams.Add("partof", filter.PartOf.Trim());

            // Parámetros técnicos de paginación FHIR
            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            // 4. Ejecución de búsqueda en el servidor FHIR
            var bundle = await _fhirClient.SearchAsync<Hl7.Fhir.Model.Organization>(searchParams);

            // 5. Transformación mediante Helpers y mapeo a DTO
            var pagedResult =
                FhirPaginationHelper.ToPagedResult<Hl7.Fhir.Model.Organization>(bundle, pageNumber, pageSize);

            return new PagedResultDto<OrganizationDto>
            {
                Items = pagedResult.Items
                    .Select(o => o.ToDto())
                    .Where(dto => dto != null)!
                    .ToList(),
                Pagination = pagedResult.Pagination
            };
        }
        catch (FhirOperationException ex)
        {
            // Centralización de errores de parámetros o saturación del servicio
            throw FhirExceptionMapper.Map(ex, "SEARCH_FILTERED", "ORGANIZATION_LIST");
        }
    }

    // Auxiliar para obtener el valor de EnumMember
    private static string GetEnumMemberValue(Enum enumValue)
    {
        var type = enumValue.GetType();
        var info = type.GetField(enumValue.ToString());
        var attr = info?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
            .Cast<EnumMemberAttribute>()
            .FirstOrDefault();

        return attr?.Value ?? enumValue.ToString().ToLowerInvariant();
    }
}