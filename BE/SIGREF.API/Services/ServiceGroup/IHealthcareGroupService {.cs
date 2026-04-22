using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.Common.Dtos;
using FhirList = Hl7.Fhir.Model.List;
namespace SIGREF.API.Services.ServiceGroup;

/// <summary>
/// Contrato para el servicio de gestión de grupos de atención.
/// </summary>
public interface IHealthcareGroupService
{
    /// <summary>
    /// Retorna los grupos de atención que coincidan con los filtros indicados, paginados.
    /// </summary>
    Task<PagedResultDto<ServiceGroupDto>> GetFilteredAsync(ServiceGroupFilterDto filter);

    /// <summary>
    /// Retorna un único grupo de atención por su ID FHIR, o nulo si no existe.
    /// </summary>
    Task<ServiceGroupDto?> GetByIdAsync(string id);

    /// <summary>
    /// Crea un nuevo grupo de atención en el servidor FHIR.
    /// </summary>
    Task<FhirList> CreateAsync(FhirList careGroup);

    /// <summary>
    /// Actualiza un grupo de atención existente en el servidor FHIR.
    /// </summary>
    Task<FhirList> UpdateAsync(FhirList list);

    /// <summary>
    /// Elimina un grupo de atención del servidor FHIR por su ID.
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Recupera el recurso crudo desde FHIR para usarlo en operaciones de actualización.
    /// </summary>
    Task<FhirList?> GetFhirListByIdAsync(string id);
}
