using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.API.Controllers.Auth;

/// <summary>
/// Controlador para la gestión de usuarios en Keycloak.
/// </summary>
/// <remarks>
/// Expone operaciones de creación, consulta, edición y gestión del ciclo de vida
/// de usuarios del realm. Todas las rutas requieren autenticación Bearer y al menos
/// un rol autorizado.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Usuarios - Gestión en Keycloak")]
public class UsersController : ControllerBase
{
    private readonly IKeycloakAdminService _kcAdmin;

    public UsersController(IKeycloakAdminService kcAdmin)
    {
        _kcAdmin = kcAdmin;
    }

    // ============================================================
    // CREAR USUARIO
    // ============================================================

    /// <summary>
    /// Crea un nuevo usuario en Keycloak vinculado a un Practitioner FHIR.
    /// </summary>
    /// <remarks>
    /// Valida permisos del creador, existencia del Practitioner en FHIR y que
    /// dicho Practitioner no esté ya vinculado a otro usuario.
    /// </remarks>
    /// <param name="dto">Datos del usuario a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpPost]
    [EndpointName("CreateUser")]
    [EndpointSummary("Crear un nuevo usuario en Keycloak")]
    [EndpointDescription("Crea un nuevo usuario en Keycloak vinculado a un Practitioner FHIR. Valida permisos del creador, existencia del Practitioner en FHIR y que dicho Practitioner no esté ya vinculado a otro usuario.")]
    [Tags("HÍBRIDO - Usuarios Keycloak / Practitioner FHIR")]
    [ProducesResponseType(typeof(KeycloakUserDto),  StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser(
        [FromBody] UserCreateDto dto,
        CancellationToken ct)
    {
        var created = await _kcAdmin.CreateUserAsync(
            User,
            dto.Username,
            dto.PractitionerId,
            dto.Email,
            dto.Password,
            dto.Roles,
            ct);
        return CreatedAtAction(
            nameof(GetUserById),
            new { id = created.Id },
            created);
    }

    // ============================================================
    // OBTENER USUARIO POR ID
    // ============================================================

    /// <summary>
    /// Obtiene un usuario por su UUID de Keycloak.
    /// </summary>
    /// <param name="id">UUID del usuario en Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("{id}")]
    [EndpointName("GetUserById")]
    [EndpointSummary("Obtener usuario por ID")]
    [EndpointDescription("Obtiene un usuario específico utilizando su UUID de Keycloak.")]
    [Tags("HÍBRIDO - Usuarios Keycloak / Practitioner FHIR")]
    [ProducesResponseType(typeof(KeycloakUserDto),  StatusCodes.Status200OK)]
    public async Task<ActionResult<KeycloakUserDto>> GetUserById(
        string id,
        CancellationToken ct)
    {
        var user = await _kcAdmin.GetUserByIdAsync(id, ct);
        return Ok(user);
    }

    // ============================================================
    // OBTENER MÚLTIPLES USUARIOS POR IDS
    // ============================================================

    /// <summary>
    /// Obtiene en una sola petición varios usuarios a partir de una lista de IDs.
    /// </summary>
    /// <remarks>
    /// Usa la sintaxis nativa <c>id:uuid1 uuid2 …</c> de Keycloak 26.3+.
    /// Los IDs no encontrados son ignorados silenciosamente.
    /// </remarks>
    /// <param name="ids">Lista de UUIDs de Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-ids")]
    [EndpointName("GetUserListByIds")]
    [EndpointSummary("Obtener múltiples usuarios por sus IDs")]
    [EndpointDescription("Obtiene en una sola petición varios usuarios a partir de una lista de IDs. Usa la sintaxis nativa id:uuid1 uuid2 … de Keycloak 26.3+. Los IDs no encontrados son ignorados silenciosamente.")]
    [Tags("HÍBRIDO - Usuarios Keycloak / Practitioner FHIR")]
    [ProducesResponseType(typeof(List<KeycloakUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<KeycloakUserDto>>> GetUsersByIds(
        [FromQuery, Required] List<string> ids,
        CancellationToken ct)
    {
        var users = await _kcAdmin.GetUsersByIdsAsync(ids, ct);
        return Ok(users);
    }

    // ============================================================
    // OBTENER USUARIO POR PRACTITIONER
    // ============================================================

    /// <summary>
    /// Busca el usuario de Keycloak vinculado a un Practitioner FHIR.
    /// </summary>
    /// <param name="practitionerId">ID del Practitioner FHIR.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-practitioner/{practitionerId}")]
    [EndpointName("GetUserByPractitionerId")]
    [EndpointSummary("Obtener usuario por Practitioner ID")]
    [EndpointDescription("Busca el usuario de Keycloak que se encuentra vinculado a un Practitioner FHIR específico.")]
    [Tags("HÍBRIDO - Usuarios Keycloak / Practitioner FHIR")]
    [ProducesResponseType(typeof(KeycloakUserDto),  StatusCodes.Status200OK)]

    public async Task<ActionResult<KeycloakUserDto>> GetUserByPractitionerId(
        string practitionerId,
        CancellationToken ct)
    {
        var user = await _kcAdmin.GetUserByPractitionerIdAsync(practitionerId, ct);
        return Ok(user);
    }

    // ============================================================
    // VERIFICAR VINCULACIÓN DE PRACTITIONER
    // ============================================================

    /// <summary>
    /// Verifica si un Practitioner FHIR ya está vinculado a algún usuario de Keycloak.
    /// </summary>
    /// <param name="practitionerId">ID del Practitioner FHIR.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("exists/practitioner/{practitionerId}")]
    [EndpointName("GetUserPractitionerHasUser")]
    [EndpointSummary("Verificar vinculación de un Practitioner")]
    [EndpointDescription("Verifica si un Practitioner FHIR ya está vinculado a algún usuario existente en Keycloak.")]
    [Tags("HÍBRIDO - Usuarios Keycloak / Practitioner FHIR")]
    [ProducesResponseType(typeof(bool),           StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> PractitionerHasUser(
        string practitionerId,
        CancellationToken ct)
    {
        var hasUser = await _kcAdmin.PractitionerHasUserAsync(practitionerId, ct);
        return Ok(hasUser);
    }

    // ============================================================
    // VERIFICAR DISPONIBILIDAD DE USERNAME
    // ============================================================

    /// <summary>
    /// Verifica si un username ya está en uso y retorna usernames similares encontrados.
    /// </summary>
    /// <remarks>
    /// La búsqueda interna de Keycloak es amplia (aplica sobre nombre, email, username),
    /// por lo que la lista de similares puede incluir resultados que coincidan en otros campos.
    /// La propiedad <c>ExistName</c> indica únicamente si hay coincidencia exacta de username.
    /// </remarks>
    /// <param name="username">Username a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin},{RolesConstants.auditor}")]
    [HttpGet("exists/username")]
    [EndpointName("GetUserVerifyUsernames")]
    [EndpointSummary("Verificar disponibilidad de Username")]
    [EndpointDescription("Verifica si un username ya está en uso y retorna usernames similares. La búsqueda interna de Keycloak es amplia (nombre, email, username). La propiedad ExistName indica únicamente coincidencia exacta.")]
    [Tags("HÍBRIDO - Usuarios Keycloak / Practitioner FHIR")]
    [ProducesResponseType(typeof(KeycloakUsernameDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<KeycloakUsernameDto>> ExistUsername(
        [FromQuery, Required] string username,
        CancellationToken ct)
    {
        var result = await _kcAdmin.ExistUserNameAsync(username, ct);
        return Ok(result);
    }

    // ============================================================
    // LISTAR USUARIOS PAGINADOS
    // ============================================================

    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtro opcional por username o término de búsqueda.
    /// </summary>
    /// <remarks>
    /// El tamaño de página está limitado a 30 elementos máximo. Keycloak no expone el total
    /// real de registros en listas filtradas, por lo que <c>TotalItems</c> y <c>TotalPages</c>
    /// siempre serán <c>null</c>.
    /// </remarks>
    /// <param name="filter">Parámetros de paginación y filtrado.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin},{RolesConstants.auditor}")]
    [HttpGet]
    [EndpointName("GetUserList")]
    [EndpointSummary("Obtener usuarios paginados")]
    [EndpointDescription("Obtiene una lista paginada de usuarios con filtro opcional por username o término de búsqueda.")]
    [Tags("HÍBRIDO - Usuarios Keycloak / Practitioner FHIR")]
    [ProducesResponseType(typeof(PagedResultDto<KeycloakUserDto>), StatusCodes.Status200OK)]

    public async Task<ActionResult<PagedResultDto<KeycloakUserDto>>> GetUsersList(
        [FromQuery] KeycloakFilter filter,
        CancellationToken ct)
    {
        var result = await _kcAdmin.GetUsersListAsync(filter);
        return Ok(result);
    }

    // ============================================================
    // TOGGLE DE ESTADO (ACTIVAR / DESACTIVAR)
    // ============================================================

    /// <summary>
    /// Alterna el estado activo/inactivo de un usuario.
    /// </summary>
    /// <remarks>
    /// Un usuario no puede cambiar su propio estado.
    /// Solo roles <c>ti</c> y <c>admin</c> pueden ejecutar esta operación.
    /// </remarks>
    /// <param name="id">UUID del usuario cuyo estado se va a alternar.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpPatch("{id}/toggle-status")]
    [EndpointName("UpdateUserToggleStatus")]
    [EndpointSummary("Activar o desactivar usuario")]
    [EndpointDescription("Alterna el estado activo/inactivo de un usuario. Un usuario no puede cambiar su propio estado. Requiere rol 'ti' o 'admin'.")]
    [Tags("HÍBRIDO - Usuarios Keycloak / Practitioner FHIR")]
    [ProducesResponseType(typeof(bool),           StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> ToggleUserStatus(
        string id,
        CancellationToken ct)
    {
        var newStatus = await _kcAdmin.ToggleUserStatusAsync(User, id, ct);
        return Ok(newStatus);
    }

    // ============================================================
    // EDITAR USUARIO
    // ============================================================

    /// <summary>
    /// Actualiza los datos de un usuario existente (sin contraseña ni username).
    /// </summary>
    /// <remarks>
    /// Solo los campos con valor no nulo en el body son modificados.
    /// Si se incluye <c>NewRoleName</c>, el rol actual del usuario es reemplazado
    /// respetando las reglas de jerarquía.
    /// </remarks>
    /// <param name="id">UUID del usuario a editar.</param>
    /// <param name="dto">Campos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpPut("{id}")]
    [EndpointName("UpdateUser")]
    [EndpointSummary("Actualizar datos de un usuario")]
    [EndpointDescription("Actualiza los datos de un usuario existente. Solo se modifican los campos no nulos. Si se incluye NewRoleName, el rol actual se reemplaza respetando la jerarquía.")]
    [Tags("Users")]
    [ProducesResponseType(typeof(KeycloakUserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<KeycloakUserDto>> UpdateUser(
        string id,
        [FromBody] KeycloakUpdateUserDto dto,
        CancellationToken ct)
    {
        var updated = await _kcAdmin.UpdateUserAsync(User, id, dto, ct);
        return Ok(updated);
    }

    // ============================================================
    // ELIMINAR USUARIO
    // ============================================================

    /// <summary>
    /// Elimina permanentemente un usuario del realm de Keycloak.
    /// </summary>
    /// <remarks>
    /// Un usuario no puede eliminarse a sí mismo.
    /// Solo roles <c>ti</c> y <c>admin</c> pueden ejecutar esta operación.
    /// </remarks>
    /// <param name="id">UUID del usuario a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpDelete("{id}")]
    [EndpointName("DeleteUser")]
    [EndpointSummary("Eliminar un usuario")]
    [EndpointDescription("Elimina permanentemente un usuario del realm de Keycloak. Un usuario no puede eliminarse a sí mismo.")]
    [Tags("Users")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(
        string id,
        CancellationToken ct)
    {
        await _kcAdmin.DeleteUserAsync(User, id, ct);
        return NoContent();
    }
}
