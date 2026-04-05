using System.Security.Claims;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;

namespace SIGREF.Infrastructure.Keycloak.Interfaces;

/// <summary>
/// Contrato de la capa de aplicación para la gestión de usuarios en Keycloak.
/// </summary>
/// <remarks>
/// <para>
/// Esta interfaz actúa como fachada sobre <see cref="IKeycloakClient"/>, añadiendo
/// lógica de negocio: validación de permisos por rol, integración con FHIR,
/// y normalización de respuestas mediante <see cref="ResponseDto{T}"/>.
/// </para>
/// <para>
/// Ningún método de esta interfaz expone <see cref="System.Text.Json.JsonElement"/> al caller;
/// todas las respuestas están mapeadas a DTOs tipados.
/// </para>
/// </remarks>
public interface IKeycloakAdminService
{
    // =========================================================
    // CREAR USUARIO
    // =========================================================
 
    /// <summary>
    /// Crea un usuario en Keycloak vinculado a un Practitioner FHIR.
    /// </summary>
    /// <remarks>
    /// Valida en orden:
    /// <list type="number">
    ///   <item><description>Que el creador tenga un rol válido y pueda asignar los roles solicitados.</description></item>
    ///   <item><description>Que el Practitioner exista y esté activo en FHIR.</description></item>
    ///   <item><description>Que el Practitioner no esté ya vinculado a otro usuario en Keycloak.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="creator">Claims del usuario que realiza la operación (para validar permisos).</param>
    /// <param name="username">Nombre de usuario único en el realm.</param>
    /// <param name="practitionerId">ID del Practitioner FHIR a vincular.</param>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="password">Contraseña inicial (no temporal).</param>
    /// <param name="roles">Arreglo de roles de realm a asignar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con el <see cref="KeycloakUserDto"/> creado,
    /// o con el código de error correspondiente si alguna validación falla.
    /// </returns>
    Task<ResponseDto<KeycloakUserDto>> CreateUserAsync(
        ClaimsPrincipal creator,
        string username,
        string practitionerId,
        string email,
        string password,
        string[] roles,
        CancellationToken ct = default);
 
    // =========================================================
    // OBTENER USUARIO
    // =========================================================
 
    /// <summary>
    /// Obtiene un usuario por su ID de Keycloak.
    /// </summary>
    /// <param name="keycloakUserId">UUID del usuario en Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con el <see cref="KeycloakUserDto"/> si existe,
    /// o con <c>Data = null</c> si no fue encontrado (HTTP 404 se trata como resultado vacío).
    /// </returns>
    Task<ResponseDto<KeycloakUserDto?>> GetUserByIdAsync(
        string keycloakUserId,
        CancellationToken ct = default);
 
    /// <summary>
    /// Obtiene en una sola petición varios usuarios a partir de una lista de IDs.
    /// </summary>
    /// <remarks>
    /// Usa la sintaxis <c>id:uuid1 uuid2 …</c> de Keycloak 26.3+.
    /// Los IDs no encontrados son ignorados silenciosamente.
    /// </remarks>
    /// <param name="userIds">Colección de UUIDs de Keycloak a consultar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con la lista de <see cref="KeycloakUserDto"/> encontrados.
    /// </returns>
    Task<ResponseDto<List<KeycloakUserDto>>> GetUsersByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default);
 
    /// <summary>
    /// Busca el usuario vinculado a un Practitioner FHIR por su <c>practitionerId</c>.
    /// </summary>
    /// <param name="practitionerId">ID del Practitioner FHIR.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con el usuario encontrado, o <c>Data = null</c> si no existe.
    /// </returns>
    Task<ResponseDto<KeycloakUserDto?>> GetUserByPractitionerIdAsync(
        string practitionerId,
        CancellationToken ct = default);
 
    /// <summary>
    /// Verifica si un Practitioner FHIR ya está vinculado a algún usuario de Keycloak.
    /// </summary>
    /// <param name="practitionerId">ID del Practitioner FHIR.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con <c>true</c> si ya tiene usuario vinculado,
    /// <c>false</c> en caso contrario.
    /// </returns>
    Task<ResponseDto<bool>> PractitionerHasUserAsync(
        string practitionerId,
        CancellationToken ct = default);
 
    /// <summary>
    /// Verifica si un username ya está en uso en el realm, y retorna sugerencias similares.
    /// </summary>
    /// <param name="username">Username a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con un <see cref="KeycloakUsernameDto"/> que indica
    /// si el username está tomado y lista los usernames similares encontrados.
    /// </returns>
    Task<ResponseDto<KeycloakUsernameDto>> ExistUserNameAsync(
        string username,
        CancellationToken ct = default);
 
    // =========================================================
    // LISTAR USUARIOS
    // =========================================================
 
    /// <summary>
    /// Obtiene una página de usuarios con filtros opcionales.
    /// </summary>
    /// <remarks>
    /// El tamaño de página está limitado a un máximo de 30 elementos para proteger
    /// el rendimiento del servidor de identidad. Keycloak no devuelve el total real
    /// de registros en listas filtradas, por lo que <c>TotalItems</c> y <c>TotalPages</c>
    /// siempre serán <c>null</c>.
    /// </remarks>
    /// <param name="filter">Parámetros de paginación y filtrado.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con un <see cref="PagedResultDto{T}"/> de <see cref="KeycloakUserDto"/>.
    /// </returns>
    Task<ResponseDto<PagedResultDto<KeycloakUserDto>>> GetUsersListAsync(KeycloakFilter filter);
 
    // =========================================================
    // TOGGLE DE ESTADO
    // =========================================================
 
    /// <summary>
    /// Alterna el estado activo/inactivo de un usuario.
    /// </summary>
    /// <remarks>
    /// Solo un usuario con rol <c>ti</c> o <c>admin</c> puede ejecutar esta operación.
    /// Un usuario no puede desactivarse a sí mismo.
    /// </remarks>
    /// <param name="requestor">Claims del usuario que solicita el cambio de estado.</param>
    /// <param name="targetUserId">UUID del usuario cuyo estado se va a alternar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con <c>true</c> si el usuario quedó habilitado,
    /// <c>false</c> si fue deshabilitado.
    /// </returns>
    Task<ResponseDto<bool>> ToggleUserStatusAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        CancellationToken ct = default);
 
    // =========================================================
    // EDICIÓN DE USUARIO
    // =========================================================
 
    /// <summary>
    /// Actualiza los datos de un usuario existente (sin contraseña ni username).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Solo los campos con valor no nulo en <paramref name="updateDto"/> son modificados;
    /// los demás conservan su valor actual en Keycloak.
    /// </para>
    /// <para>
    /// Si <paramref name="updateDto"/> incluye <c>NewRoleName</c>, el rol actual del usuario
    /// es reemplazado. Solo un usuario con rol <c>ti</c> o <c>admin</c> puede cambiar roles,
    /// y las mismas restricciones de jerarquía de <see cref="CreateUserAsync"/> aplican.
    /// </para>
    /// </remarks>
    /// <param name="requestor">Claims del usuario que realiza la edición.</param>
    /// <param name="targetUserId">UUID del usuario a editar.</param>
    /// <param name="updateDto">DTO con los campos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con el <see cref="KeycloakUserDto"/> actualizado.
    /// </returns>
    Task<ResponseDto<KeycloakUserDto>> UpdateUserAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        KeycloakUpdateUserDto updateDto,
        CancellationToken ct = default);
}