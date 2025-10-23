using System.Runtime.Serialization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions;
using SIGREF.API.Services.Organization;

namespace SIGREF.API.Services.Organizations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly FhirClient _fhirClient;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(FhirClient fhirClient, ILogger<OrganizationService> logger)
        {
            _fhirClient = fhirClient;
            _logger = logger;
        }

        public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto)
        {
            try
            {
                var organization = dto.ToFhirResource();
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
                var result = await _fhirClient.UpdateAsync(updatedOrganization);
                return result.ToDto();
            }
            catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Organizaci�n con ID {Id} no encontrada para actualizar", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando organizaci�n con ID {Id}", id);
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
        public async Task<PagedResult<OrganizationDto>> GetFilteredOrganizationsAsync(OrganizationFilterDto filter)
        {
            // Validar y normalizar parámetros de paginación
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
            var offset = (pageNumber - 1) * pageSize;

            var searchParams = new SearchParams();

            // Filtros
            if (!string.IsNullOrWhiteSpace(filter.Name))
                searchParams.Add("name", filter.Name);

            if (filter.Active.HasValue)
                searchParams.Add("active", filter.Active.Value.ToString().ToLowerInvariant());

            if (filter.Types != null && filter.Types.Any())
            {
                foreach (var type in filter.Types)
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

            var organizations = bundle.Entry
                .Where(e => e.Resource is Hl7.Fhir.Model.Organization)
                .Select(e => ((Hl7.Fhir.Model.Organization)e.Resource).ToDto())
                .ToList();

            // Calcular totales
            var totalItems = bundle.Total ?? organizations.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagination = new PaginationDto
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasPrevious = pageNumber > 1,
                HasNext = bundle.NextLink != null || pageNumber < totalPages
            };

            return new PagedResult<OrganizationDto>
            {
                Items = organizations,
                Pagination = pagination
            };
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
}