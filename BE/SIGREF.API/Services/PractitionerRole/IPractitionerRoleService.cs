#nullable enable
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Services.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.PractitionerRole;
public interface IPractitionerRoleService
{
    // Crear un nuevo PractitionerRole
    Task<ServiceResult<PractitionerRoleDto>> CreateAsync(CreatePractitionerRoleDto dto);

    // Obtener un PractitionerRole por Id
    Task<PractitionerRoleDto?> GetByIdAsync(string id);

    // Obtener un PractitionerRole por PractitionerId
    Task<IEnumerable<PractitionerRoleDto>> GetByPractitionerIdAsync(string practitionerId);

    // Actualizar
    Task<ServiceResult<PractitionerRoleDto?>> UpdateAsync(string id, UpdatePractitionerRoleDto dto);

    // Eliminar
    Task<bool> DeleteAsync(string id);

    // Filtrar
    Task<PagedResultDto<PractitionerRoleDto>> GetFilteredAsync(PractitionerRoleFilterDto filters);
}

