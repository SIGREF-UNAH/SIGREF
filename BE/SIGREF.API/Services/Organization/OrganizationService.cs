using System.Runtime.Serialization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Services.Organization;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.API.Services.Organizations;

public class OrganizationService : BaseFhirService, IOrganizationService
{
    private readonly FhirClient _fhirClient;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(FhirClient fhirClient, ILogger<OrganizationService> logger,  IUserContextService userContext,       
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
            var organization = dto.ToFhirResource();
            ApplyMeta(organization,isCreate:true);
            var result = await _fhirClient.CreateAsync(organization);
            return result.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando organizaci�n");
            throw;
        }
    }

    public async Task<OrganizationDto> GetOrganizationByIdAsync(string id)
    {
        try
        {
            // USAR EL TIPO COMPLETAMENTE CALIFICADO
            var organization = await _fhirClient.ReadAsync<Hl7.Fhir.Model.Organization>($"Organization/{id}");
            return organization?.ToDto();
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Organizaci�n con ID {Id} no encontrada", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo organizaci�n con ID {Id}", id);
            throw;
        }
    }

    public async Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationDto dto)
    {
        try
        {
            // USAR EL TIPO COMPLETAMENTE CALIFICADO
            var existingOrganization = await _fhirClient.ReadAsync<Hl7.Fhir.Model.Organization>($"Organization/{id}");
            if (existingOrganization == null)
            {
                return null;
            }

            var updatedOrganization = dto.UpdateFhirResource(existingOrganization);
            ApplyMeta(updatedOrganization,isCreate:false);
            var result = await _fhirClient.UpdateAsync(updatedOrganization);
            return result.ToDto();
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Organizacion con ID {Id} no encontrada para actualizar", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando organizacion con ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteOrganizationAsync(string id)
    {
        try
        {
            await _fhirClient.DeleteAsync($"Organization/{id}");
            return true;
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Organizaci�n con ID {Id} no encontrada para eliminar", id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando organizaci�n con ID {Id}", id);
            throw;
        }
    }

    // Filtros
    public async Task<PagedResultDto<OrganizationDto>> GetFilteredOrganizationsAsync(OrganizationFilterDto filter)
    {
        // Normalizar paginación usando el helper
        var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);

        var searchParams = new SearchParams();

        // Filtros
        if (!string.IsNullOrWhiteSpace(filter.Name))
            searchParams.Add("name:contains", filter.Name);

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
            searchParams.Add("partof", filter.PartOf);

        // Parámetros de paginación FHIR
        searchParams.Count = pageSize;
        searchParams.Add("_offset", offset.ToString());
        searchParams.Add("_total", "accurate");

        // Ejecutar búsqueda
        var bundle = await _fhirClient.SearchAsync<Hl7.Fhir.Model.Organization>(searchParams);

        // Obtener PagedResult del helper
        var pagedResult = FhirPaginationHelper.ToPagedResult<Hl7.Fhir.Model.Organization>(bundle, pageNumber, pageSize);

        // Convertir Items a DTO
        var resultDto = new PagedResultDto<OrganizationDto>
        {
            Items = pagedResult.Items.Select(o => o.ToDto()).ToList(),
            Pagination = pagedResult.Pagination
        };

        return resultDto;
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