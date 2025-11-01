#nullable enable
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Services.Common;

namespace SIGREF.API.Services.PractitionerRole;
public interface IPractitionerRoleService
{
    // Crear un nuevo PractitionerRole
    Task<ServiceResult<PractitionerRoleDto>> CreateAsync(CreatePractitionerRoleDto dto);

    // Obtener un PractitionerRole por Id
    Task<PractitionerRoleDto?> GetByIdAsync(string id);

    // Actualizar
    Task<ServiceResult<PractitionerRoleDto?>> UpdateAsync(string id, UpdatePractitionerRoleDto dto);

    // Eliminar
    Task<bool> DeleteAsync(string id);

    // Filtrar
    Task<PagedResult<PractitionerRoleDto>> GetFilteredAsync(PractitionerRoleFilterDto filters);
}

