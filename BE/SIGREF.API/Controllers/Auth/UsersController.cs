using System.ComponentModel.DataAnnotations;
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
//[Authorize(AuthenticationSchemes = "Bearer")]
public class UsersController : ControllerBase
{
    private readonly IKeycloakAdminService _kcAdmin;
 
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UsersController"/>.
    /// </summary>
    /// <param name="kcAdmin">Servicio de administración de usuarios de Keycloak.</param>
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
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpPost("create")]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>),  StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>),  StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>),  StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>),  StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>),  StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
    {
        var response = await _kcAdmin.CreateUserAsync(
            User,
            dto.Username,
            dto.PractitionerId,
            dto.Email,
            dto.Password,
            dto.Roles
        );
 
        return StatusCode(response.StatusCode, response);
    }
 
    // ============================================================
    // OBTENER USUARIO POR ID
    // ============================================================
 
    /// <summary>
    /// Obtiene un usuario por su UUID de Keycloak.
    /// </summary>
    /// <param name="id">UUID del usuario en Keycloak.</param>
    //[Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-id/{id}")]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto?>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto?>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserById(string id)
    {
        var response = await _kcAdmin.GetUserByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }
    // PRUEBAS TEST
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
    /// <param name="ids">Lista de UUIDs de Keycloak separados por coma en el query string.</param>
    //[Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-ids")]
    [ProducesResponseType(typeof(ResponseDto<List<KeycloakUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<List<KeycloakUserDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<List<KeycloakUserDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<List<KeycloakUserDto>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<List<KeycloakUserDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsersByIds([FromQuery, Required] List<string> ids)
    {
        if (!ModelState.IsValid || ids.Count == 0)
            return BadRequest(new ResponseDto<List<KeycloakUserDto>>
            {
                Status     = false,
                StatusCode = 400,
                Message    = "Debe proporcionar al menos un ID.",
                Data       = null
            });
 
        var response = await _kcAdmin.GetUsersByIdsAsync(ids);
        return StatusCode(response.StatusCode, response);
    }
 
    // ============================================================
    // OBTENER USUARIO POR PRACTITIONER
    // ============================================================
 
    /// <summary>
    /// Busca el usuario de Keycloak vinculado a un Practitioner FHIR.
    /// </summary>
    /// <param name="practitionerId">ID del Practitioner FHIR.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-practitioner/{practitionerId}")]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto?>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto?>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByPractitionerId(string practitionerId)
    {
        var response = await _kcAdmin.GetUserByPractitionerIdAsync(practitionerId);
        return StatusCode(response.StatusCode, response);
    }
 
    // ============================================================
    // VERIFICAR VINCULACIÓN DE PRACTITIONER
    // ============================================================
 
    /// <summary>
    /// Verifica si un Practitioner FHIR ya está vinculado a algún usuario de Keycloak.
    /// </summary>
    /// <param name="practitionerId">ID del Practitioner FHIR.</param>
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("exists/practitioner/{practitionerId}")]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PractitionerHasUser(string practitionerId)
    {
        var response = await _kcAdmin.PractitionerHasUserAsync(practitionerId);
        return StatusCode(response.StatusCode, response);
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
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin},{RolesConstants.auditor}")]
    [HttpGet("exists/username")]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUsernameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUsernameDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUsernameDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUsernameDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUsernameDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExistUsername([FromQuery, Required] string username)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResponseDto<KeycloakUsernameDto>
            {
                Status     = false,
                StatusCode = 400,
                Message    = "El parámetro 'username' es obligatorio.",
                Data       = null
            });
 
        var response = await _kcAdmin.ExistUserNameAsync(username);
        return StatusCode(response.StatusCode, response);
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
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin},{RolesConstants.auditor}")]
    [HttpGet("list")]
    [ProducesResponseType(typeof(ResponseDto<PagedResultDto<KeycloakUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<PagedResultDto<KeycloakUserDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<PagedResultDto<KeycloakUserDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<PagedResultDto<KeycloakUserDto>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<PagedResultDto<KeycloakUserDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsersList([FromQuery] KeycloakFilter filter)
    {
        var response = await _kcAdmin.GetUsersListAsync(filter);
        return StatusCode(response.StatusCode, response);
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
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpPatch("{id}/toggle-status")]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var response = await _kcAdmin.ToggleUserStatusAsync(User, id);
        return StatusCode(response.StatusCode, response);
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
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<KeycloakUserDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] KeycloakUpdateUserDto dto)
    {
        var response = await _kcAdmin.UpdateUserAsync(User, id, dto);
        return StatusCode(response.StatusCode, response);
    }
}