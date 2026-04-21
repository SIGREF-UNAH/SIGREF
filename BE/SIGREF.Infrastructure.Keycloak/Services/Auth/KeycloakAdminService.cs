using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SIGREF.Common.Dtos;
using SIGREF.Common.Interfaces;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth;

/// <summary>
/// Implementación de la capa de aplicación para la gestión de usuarios en Keycloak.
/// </summary>
/// <remarks>
/// <para>
/// Actúa como fachada sobre <see cref="IKeycloakClient"/>, orquestando validaciones
/// de negocio (permisos por rol, integración FHIR, restricciones de jerarquía)
/// antes de delegar las operaciones de infraestructura en el cliente HTTP.
/// </para>
/// <para>
/// Las reglas de jerarquía de roles se centralizan en el campo estático
/// <see cref="RoleRules"/>: cada rol solo puede crear/asignar roles que estén
/// en su lista permitida. Esta restricción aplica tanto en
/// <see cref="CreateUserAsync"/> como en <see cref="UpdateUserAsync"/>.
/// </para>
/// <para>
/// Todas las respuestas están normalizadas mediante <see cref="ResponseDto{T}"/>;
/// ningún método expone excepciones de infraestructura al llamador salvo que
/// sean errores verdaderamente inesperados.
/// </para>
/// </remarks>
public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IKeycloakClient            _kc;
    private readonly IFhirPractitionerService   _fhirBridge;
    private readonly ILogger<KeycloakAdminService> _logger;
 
    /// <summary>
    /// Mapa de jerarquía de roles: cada clave es el rol del creador/editor, y su
    /// valor es el conjunto de roles que ese creador puede asignar a otros usuarios.
    /// </summary>
    /// <remarks>
    /// Un rol ausente de este mapa no tiene permiso para crear usuarios.
    /// Un rol con array vacío puede autenticarse pero no puede asignar roles.
    /// </remarks>
    private static readonly Dictionary<string, string[]> RoleRules = new()
    {
        { "ti",      ["ti", "admin", "auditor", "cashier"] },
        { "admin",   ["auditor", "cashier"] },
        { "auditor", [] },
        { "cashier", [] }
    };
 
    /// <summary>
    /// Conjunto de roles que tienen permiso para modificar el estado, los datos
    /// y el ciclo de vida de otros usuarios (activar, desactivar, editar, eliminar).
    /// </summary>
    private static readonly HashSet<string> ManagementRoles = ["ti", "admin"];
 
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="KeycloakAdminService"/>.
    /// </summary>
    /// <param name="kc">
    /// Cliente HTTP de Keycloak. Debe implementar <see cref="IKeycloakClient"/>
    /// y, concretamente, ser una instancia de <see cref="KeycloakClient"/> para
    /// que las validaciones previas de roles mediante
    /// <c>FetchRealmRoleAsync</c> funcionen correctamente.
    /// </param>
    /// <param name="fhirBridge">
    /// Servicio puente hacia FHIR para validar la existencia y estado de
    /// los Practitioners antes de crear o modificar usuarios.
    /// </param>
    /// <param name="logger">
    /// Logger para registrar errores críticos, rollbacks y eventos relevantes
    /// del ciclo de vida de usuarios en Keycloak.
    /// </param>
    public KeycloakAdminService(
        IKeycloakClient kc,
        IFhirPractitionerService fhirBridge,
        ILogger<KeycloakAdminService> logger)
    {
        _kc         = kc;
        _fhirBridge = fhirBridge;
        _logger     = logger;
    }
 
    // ============================================================
    // CREAR USUARIO
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<KeycloakUserDto>> CreateUserAsync(
        ClaimsPrincipal creator,
        string username,
        string practitionerId,
        string email,
        string password,
        string[] roles,
        CancellationToken ct = default)
    {
        // 1. Extraer y validar el rol del creador.
        var creatorRole = creator.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(creatorRole))
            return Fail<KeycloakUserDto>(401, "No se pudo determinar el rol del creador.");
 
        if (!RoleRules.TryGetValue(creatorRole, out var allowedRoles)
            || roles.Any(r => !allowedRoles.Contains(r)))
            return Fail<KeycloakUserDto>(403, "El creador no tiene permiso para asignar esos roles.");
 
        // 2. Validar Practitioner en FHIR.
        var (exists, firstName, lastName, isActive) = await _fhirBridge.GetBasicDataAsync(practitionerId);
        if (!exists)
            return Fail<KeycloakUserDto>(404, $"El Practitioner '{practitionerId}' no existe.");
        if (!isActive)
            return Fail<KeycloakUserDto>(400, "El médico está inactivo en FHIR.");
 
        // 3. Validar existencia y accesibilidad de los roles ANTES de crear el usuario.
        //    Esto evita crear un usuario y luego fallar al asignar un rol inexistente.
        try
        {
            // _kc es KeycloakClient (implementación concreta); FetchRealmRoleAsync es internal.
            var kcClient = (KeycloakClient)_kc;
            foreach (var role in roles)
                await kcClient.FetchRealmRoleAsync(role, ct);
        }
        catch (KeycloakAccessForbiddenException ex)
        {
            return Fail<KeycloakUserDto>(403, ex.Message);
        }
        catch (KeycloakRoleNotFoundException ex)
        {
            return Fail<KeycloakUserDto>(404, ex.Message);
        }
 
        // 4. Verificar que el Practitioner no esté ya vinculado a otro usuario.
        var matches = await _kc.SearchUsersAsync(practitionerId, ct);
        if (matches.Any(u =>
                u.TryGetProperty("attributes", out var attrs) &&
                attrs.TryGetProperty("practitionerId", out var pid) &&
                pid[0].GetString() == practitionerId))
        {
            return Fail<KeycloakUserDto>(400, "El Practitioner ya está vinculado a un usuario existente.");
        }
 
        string? createdUserId = null;
        try
        {
            // 5. Construir el payload y crear el usuario en Keycloak.
            var kcUser = new
            {
                username    = username,
                email       = email,
                firstName   = firstName,
                lastName    = lastName,
                enabled     = true,
                credentials = new[]
                {
                    new { type = "password", value = password, temporary = false }
                },
                attributes = new Dictionary<string, string[]>
                {
                    ["practitionerId"] = [practitionerId],
                    ["displayName"]    = [$"{firstName} {lastName}".Trim()]
                }
            };
 
            createdUserId = await _kc.CreateUserAsync(kcUser, ct);
 
            if (createdUserId is null)
                return Fail<KeycloakUserDto>(500, "Error al crear el usuario en el servidor de identidad.");
 
            // 6. Asignar roles (los roles ya fueron validados en el paso 3).
            foreach (var role in roles)
                await _kc.AssignRoleAsync(createdUserId, role, ct);
 
            // 7. Recuperar y retornar el usuario creado con todos sus datos.
            JsonElement created = await _kc.GetUserByIdAsync(createdUserId, ct)
                ?? throw new InvalidOperationException("No se pudo recuperar el usuario recién creado.");
 
            return Ok("Usuario creado correctamente.", KeycloakUserMapper.ToDto(created)!);
        }
        catch (Exception ex)
        {
            // ----------------------------------------------------------------
            // ROLLBACK: si el usuario llegó a crearse en Keycloak pero algo
            // falló después (asignación de roles, recuperación, etc.),
            // se elimina para evitar registros huérfanos sin roles.
            // Se usa CancellationToken.None para garantizar que la limpieza
            // se intente incluso si el cliente canceló la petición original.
            // ----------------------------------------------------------------
            if (!string.IsNullOrEmpty(createdUserId))
            {
                try
                {
                    await _kc.DeleteUserAsync(createdUserId, CancellationToken.None);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(
                        rollbackEx,
                        "Fallo durante el rollback. El usuario '{UserId}' pudo quedar huérfano en Keycloak.",
                        createdUserId);
                }
            }
 
            _logger.LogError(
                ex,
                "Error crítico al crear usuario '{Username}'. Se aplicó rollback para el ID: {UserId}",
                username,
                createdUserId ?? "no asignado");
 
            return Fail<KeycloakUserDto>(500, $"Error interno durante la creación: {ex.Message}");
        }
    }
 
    // ============================================================
    // OBTENER USUARIO POR ID
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<KeycloakUserDto?>> GetUserByIdAsync(
        string keycloakUserId,
        CancellationToken ct = default)
    {
        var raw = await _kc.GetUserByIdAsync(keycloakUserId, ct);
 
        if (raw is null)
            return Ok<KeycloakUserDto?>("Usuario no encontrado.", null);
 
        return Ok<KeycloakUserDto?>("Usuario encontrado.", KeycloakUserMapper.ToDto(raw.Value));
    }
 
    // ============================================================
    // OBTENER MÚLTIPLES USUARIOS POR IDS
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<List<KeycloakUserDto>>> GetUsersByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default)
    {
        var users = await _kc.GetUsersByIdsAsync(userIds, ct);
        return Ok($"{users.Count} usuario(s) encontrado(s).", users);
    }
 
    // ============================================================
    // OBTENER USUARIO POR PRACTITIONER
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<KeycloakUserDto?>> GetUserByPractitionerIdAsync(
        string practitionerId,
        CancellationToken ct = default)
    {
        var results = await _kc.SearchUsersAsync(practitionerId, ct);
 
        var found = results.FirstOrDefault(u =>
            u.TryGetProperty("attributes", out var attrs) &&
            attrs.TryGetProperty("practitionerId", out var pid) &&
            pid.ValueKind == JsonValueKind.Array &&
            pid[0].GetString() == practitionerId);
 
        if (found.ValueKind == default)
            return Ok<KeycloakUserDto?>("Usuario no encontrado para este médico.", null);
 
        return Ok<KeycloakUserDto?>("Usuario encontrado.", KeycloakUserMapper.ToDto(found));
    }
 
    // ============================================================
    // VERIFICAR SI PRACTITIONER TIENE USUARIO
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<bool>> PractitionerHasUserAsync(
        string practitionerId,
        CancellationToken ct = default)
    {
        var results = await _kc.SearchUsersAsync(practitionerId, ct);
 
        bool exists = results.Any(u =>
            u.TryGetProperty("attributes", out var attrs) &&
            attrs.TryGetProperty("practitionerId", out var pid) &&
            pid.ValueKind == JsonValueKind.Array &&
            pid[0].GetString() == practitionerId);
 
        return Ok(
            exists
                ? "El médico ya tiene un usuario asignado."
                : "El médico no tiene usuario vinculado.",
            exists);
    }
 
    // ============================================================
    // VERIFICAR DISPONIBILIDAD DE USERNAME
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<KeycloakUsernameDto>> ExistUserNameAsync(
        string username,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Fail<KeycloakUsernameDto>(400, "El nombre de usuario es requerido.");
 
        var matches = await _kc.SearchUsernamesAsync(username, ct);
 
        bool exactExists = matches.Any(u =>
            u.Equals(username, StringComparison.OrdinalIgnoreCase));
 
        var dto = new KeycloakUsernameDto
        {
            ExistName  = exactExists,
            NumberList = matches.Count,
            Usernames  = matches
        };
 
        return Ok(
            exactExists
                ? "El nombre de usuario ya está en uso."
                : "Nombre de usuario disponible.",
            dto);
    }
 
    // ============================================================
    // LISTAR USUARIOS PAGINADOS
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<PagedResultDto<KeycloakUserDto>>> GetUsersListAsync(
        KeycloakFilter filter)
    {
        int page     = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        int pageSize = filter.PageSize   <= 0 ? 10 : Math.Min(filter.PageSize, 30);
        int first    = (page - 1) * pageSize;
        int max      = pageSize + 1; // +1 para detectar si hay página siguiente sin contar el total.
 
        // Search tiene precedencia sobre UserName; si ninguno está presente no se aplica filtro.
        string? searchFilter = !string.IsNullOrWhiteSpace(filter.Search)
            ? filter.Search
            : filter.UserName;
 
        var users = await _kc.GetUsersFilteredAsync(first, max, searchFilter);
 
        bool hasNext = users.Count > pageSize;
        if (hasNext) users.RemoveAt(users.Count - 1);
 
        var paged = new PagedResultDto<KeycloakUserDto>
        {
            Items      = users,
            Pagination = new PaginationDto
            {
                CurrentPage = page,
                PageSize    = pageSize,
                TotalItems  = null, // Keycloak no devuelve el total real en listas filtradas.
                TotalPages  = null,
                HasPrevious = page > 1,
                HasNext     = hasNext
            }
        };
 
        return Ok("Lista de usuarios obtenida.", paged);
    }
 
    // ============================================================
    // TOGGLE DE ESTADO (ACTIVAR / DESACTIVAR)
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<bool>> ToggleUserStatusAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        CancellationToken ct = default)
    {
        // 1. Validar que el solicitante tenga un rol de gestión.
        var requestorRole = requestor.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
 
        if (string.IsNullOrEmpty(requestorRole) || !ManagementRoles.Contains(requestorRole))
            return Fail<bool>(403, "No tiene permisos para activar o desactivar usuarios.");
 
        // 2. Evitar auto-desactivación.
        var requestorId = requestor.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
 
        if (requestorId == targetUserId)
            return Fail<bool>(400, "Un usuario no puede cambiar su propio estado.");
 
        // 3. Ejecutar el toggle en Keycloak.
        bool newStatus = await _kc.ToggleUserStatusAsync(targetUserId, ct);
 
        return Ok(
            newStatus
                ? "Usuario activado correctamente."
                : "Usuario desactivado correctamente.",
            newStatus);
    }
 
    // ============================================================
    // EDITAR USUARIO
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<KeycloakUserDto>> UpdateUserAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        KeycloakUpdateUserDto updateDto,
        CancellationToken ct = default)
    {
        // 1. Validar que el solicitante tenga un rol de gestión.
        var requestorRole = requestor.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
 
        if (string.IsNullOrEmpty(requestorRole) || !ManagementRoles.Contains(requestorRole))
            return Fail<KeycloakUserDto>(403, "No tiene permisos para editar usuarios.");
 
        // 2. Si se solicita cambio de rol, verificar jerarquía de permisos.
        if (!string.IsNullOrWhiteSpace(updateDto.NewRoleName)
            && (!RoleRules.TryGetValue(requestorRole, out var allowedRoles)
                || !allowedRoles.Contains(updateDto.NewRoleName)))
        {
            return Fail<KeycloakUserDto>(403,
                $"No tiene permisos para asignar el rol '{updateDto.NewRoleName}'.");
        }
 
        // 3. Ejecutar la actualización; lanza KeycloakUserNotFoundException si no existe.
        await _kc.UpdateUserAsync(targetUserId, updateDto, ct);
 
        // 4. Retornar el usuario con los datos ya actualizados.
        JsonElement updated = await _kc.GetUserByIdAsync(targetUserId, ct)
            ?? throw new KeycloakUserNotFoundException(targetUserId);
 
        return Ok("Usuario actualizado correctamente.", KeycloakUserMapper.ToDto(updated)!);
    }
 
    // ============================================================
    // ELIMINAR USUARIO
    // ============================================================
 
    /// <inheritdoc/>
    public async Task<ResponseDto<bool>> DeleteUserAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        CancellationToken ct = default)
    {
        // 1. Validar que el solicitante tenga un rol de gestión.
        var requestorRole = requestor.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
 
        if (string.IsNullOrEmpty(requestorRole) || !ManagementRoles.Contains(requestorRole))
            return Fail<bool>(403, "No tiene permisos para eliminar usuarios.");
 
        // 2. Evitar auto-eliminación.
        var requestorId = requestor.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
 
        if (requestorId == targetUserId)
            return Fail<bool>(400, "Un usuario no puede eliminarse a sí mismo.");
 
        // 3. Verificar que el usuario objetivo exista antes de intentar eliminarlo.
        var existing = await _kc.GetUserByIdAsync(targetUserId, ct);
        if (existing is null)
            return Fail<bool>(404, $"El usuario '{targetUserId}' no existe en Keycloak.");
 
        // 4. Ejecutar la eliminación en Keycloak.
        await _kc.DeleteUserAsync(targetUserId, ct);
 
        _logger.LogInformation(
            "Usuario '{TargetUserId}' eliminado por '{RequestorId}' con rol '{Role}'.",
            targetUserId,
            requestorId ?? "desconocido",
            requestorRole);
 
        return Ok("Usuario eliminado correctamente.", true);
    }
 
    // ============================================================
    // HELPERS DE RESPUESTA
    // ============================================================
 
    /// <summary>
    /// Crea un <see cref="ResponseDto{T}"/> de éxito con código HTTP 200.
    /// </summary>
    /// <typeparam name="T">Tipo del dato encapsulado en la respuesta.</typeparam>
    /// <param name="message">Mensaje descriptivo del resultado.</param>
    /// <param name="data">Dato de resultado a incluir en la respuesta.</param>
    /// <returns>Un <see cref="ResponseDto{T}"/> con <c>Status = true</c> y <c>StatusCode = 200</c>.</returns>
    private static ResponseDto<T> Ok<T>(string message, T data) => new()
    {
        Status     = true,
        StatusCode = 200,
        Message    = message,
        Data       = data
    };
 
    /// <summary>
    /// Crea un <see cref="ResponseDto{T}"/> de error con el código y mensaje indicados.
    /// </summary>
    /// <typeparam name="T">Tipo del dato encapsulado en la respuesta (será el valor por defecto).</typeparam>
    /// <param name="statusCode">Código HTTP de error (p. ej. 400, 403, 404, 500).</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <returns>Un <see cref="ResponseDto{T}"/> con <c>Status = false</c> y <c>Data = default</c>.</returns>
    private static ResponseDto<T> Fail<T>(int statusCode, string message) => new()
    {
        Status     = false,
        StatusCode = statusCode,
        Message    = message,
        Data       = default
    };
}