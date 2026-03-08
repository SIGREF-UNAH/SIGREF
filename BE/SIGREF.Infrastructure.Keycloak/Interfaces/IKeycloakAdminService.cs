using System.Security.Claims;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;

namespace SIGREF.Infrastructure.Keycloak.Interfaces;

public interface IKeycloakAdminService
{
    /// <summary>
    /// Crea un usuario en Keycloak vinculado a un Practitioner FHIR.
    /// Valida:
    /// - permisos del creador (rol)
    /// - existencia del Practitioner en FHIR
    /// - que ese Practitioner no esté ya vinculado a otro usuario
    /// </summary>
    Task<ResponseDto<KeycloakUserDto>> CreateUserAsync(
        ClaimsPrincipal creator,
        string username,
        string practitionerId,
        string email,
        string password,
        string[] roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario por ID de Keycloak.
    /// </summary>
    Task<ResponseDto<KeycloakUserDto?>> GetUserByIdAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un usuario por practitionerId (atributo en Keycloak).
    /// </summary>
    Task<ResponseDto<KeycloakUserDto?>> GetUserByPractitionerIdAsync(
        string practitionerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un practitioner ya está vinculado a algún usuario.
    /// </summary>
    Task<ResponseDto<bool>> PractitionerHasUserAsync(
        string practitionerId,
        CancellationToken cancellationToken = default);
    Task<ResponseDto<KeycloakUsernameDto>> ExistUserNameAsync(
        string username,
        CancellationToken cancellationToken = default);
    
    Task<ResponseDto<PagedResultDto<KeycloakUserDto>>> GetUsersListAsync(KeycloakFilter filter);

    
}