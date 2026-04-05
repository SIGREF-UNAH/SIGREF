using System.Security.Claims;
using System.Text.Json;
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
/// de negocio (permisos por rol, integración FHIR) antes de delegar en el cliente HTTP.
/// </para>
/// <para>
/// Las reglas de jerarquía de roles se centralizan en <see cref="RoleRules"/>:
/// cada rol solo puede crear/asignar roles que estén en su lista permitida.
/// </para>
/// </remarks>
public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IKeycloakClient _kc;
    private readonly IFhirPractitionerService _fhirBridge;
 
    /// <summary>
    /// Mapa de jerarquía de roles: cada clave es un rol creador, y su valor
    /// es el conjunto de roles que ese creador puede asignar.
    /// </summary>
    private static readonly Dictionary<string, string[]> RoleRules = new()
    {
        { "ti",      ["ti", "admin", "auditor", "cashier"] },
        { "admin",   ["auditor", "cashier"] },
        { "auditor", [] },
        { "cashier", [] }
    };
 
    /// <summary>
    /// Roles que tienen permiso para modificar el estado y los datos de otros usuarios.
    /// </summary>
    private static readonly HashSet<string> ManagementRoles = ["ti", "admin"];
 
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="KeycloakAdminService"/>.
    /// </summary>
    /// <param name="kc">Cliente HTTP de Keycloak.</param>
    /// <param name="fhirBridge">Servicio puente hacia FHIR para validación de Practitioners.</param>
    public KeycloakAdminService(IKeycloakClient kc, IFhirPractitionerService fhirBridge)
    {
        _kc         = kc;
        _fhirBridge = fhirBridge;
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
        // 1. Validar rol del creador
        var creatorRole = creator.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
 
        if (string.IsNullOrEmpty(creatorRole))
            return Fail<KeycloakUserDto>(401, "No se pudo determinar el rol del creador.");
 
        if (!RoleRules.TryGetValue(creatorRole, out var allowedRoles)
            || roles.Any(r => !allowedRoles.Contains(r)))
            return Fail<KeycloakUserDto>(403, "El creador no tiene permiso para asignar esos roles.");
 
        // 2. Validar que el Practitioner exista y esté activo en FHIR
        var (exists, firstName, lastName, isActive) =
            await _fhirBridge.GetBasicDataAsync(practitionerId);
 
        if (!exists)
            return Fail<KeycloakUserDto>(404,
                $"El Practitioner '{practitionerId}' no existe en el sistema de salud.");
 
        if (!isActive)
            return Fail<KeycloakUserDto>(400,
                "No se puede crear un usuario para un médico que está inactivo en FHIR.");
 
        // 3. Validar que el Practitioner NO esté ya vinculado en Keycloak
        var matches = await _kc.SearchUsersAsync(practitionerId, ct);
 
        bool isAlreadyLinked = matches.Any(u =>
            u.TryGetProperty("attributes", out var attrs)
            && attrs.TryGetProperty("practitionerId", out var pid)
            && pid.ValueKind == JsonValueKind.Array
            && pid[0].GetString() == practitionerId);
 
        if (isAlreadyLinked)
            return Fail<KeycloakUserDto>(400,
                "El Practitioner ya está vinculado a un usuario existente.");
 
        // 4. Preparar datos (Si FHIR no devuelve nombres, se usa el username como fallback)
        string fn          = firstName ?? username;
        string ln          = lastName  ?? string.Empty;
        string displayName = $"{fn} {ln}".Trim();
 
        // 5. Construir payload de Keycloak
        var kcUser = new
        {
            username,
            email,
            enabled       = true,
            emailVerified = true,
            firstName     = fn,
            lastName      = ln,
            credentials   = new[] { new { type = "password", value = password, temporary = false } },
            attributes    = new Dictionary<string, object?>
            {
                { "practitionerId", practitionerId },
                { "displayName",    displayName    }
            }
        };
 
        // 6. Crear usuario en Keycloak
        var userId = await _kc.CreateUserAsync(kcUser, ct);
 
        if (userId is null)
            return Fail<KeycloakUserDto>(500,
                "Error de comunicación con el servidor de identidad al crear el usuario.");
 
        // 7. Asignar roles
        foreach (var role in roles)
            await _kc.AssignRoleAsync(userId, role, ct);
 
        // 8. Retornar usuario final mapeado (sabemos que existe porque acabamos de crearlo)
        JsonElement created = await _kc.GetUserByIdAsync(userId, ct)
            ?? throw new InvalidOperationException(
                $"El usuario '{userId}' fue creado pero no pudo ser recuperado inmediatamente.");
 
        return Ok("Usuario creado y vinculado correctamente.", KeycloakUserMapper.ToDto(created)!);
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
            u.TryGetProperty("attributes", out var attrs)
            && attrs.TryGetProperty("practitionerId", out var pid)
            && pid.ValueKind == JsonValueKind.Array
            && pid[0].GetString() == practitionerId);
 
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
            u.TryGetProperty("attributes", out var attrs)
            && attrs.TryGetProperty("practitionerId", out var pid)
            && pid.ValueKind == JsonValueKind.Array
            && pid[0].GetString() == practitionerId);
 
        return Ok(
            exists ? "El médico ya tiene un usuario asignado."
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
            exactExists ? "El nombre de usuario ya está en uso."
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
        int max      = pageSize + 1; // +1 para detectar si hay página siguiente sin contar el total
 
        // Search tiene precedencia sobre UserName; si ninguno está presente no se aplica filtro
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
                TotalItems  = null, // Keycloak no devuelve el total real en listas filtradas
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
        // 1. Validar que el solicitante tenga un rol de gestión
        var requestorRole = requestor.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
 
        if (string.IsNullOrEmpty(requestorRole) || !ManagementRoles.Contains(requestorRole))
            return Fail<bool>(403, "No tiene permisos para activar o desactivar usuarios.");
 
        // 2. Evitar auto-desactivación
        var requestorId = requestor.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
 
        if (requestorId == targetUserId)
            return Fail<bool>(400, "Un usuario no puede cambiar su propio estado.");
 
        // 3. Ejecutar el toggle
        bool newStatus = await _kc.ToggleUserStatusAsync(targetUserId, ct);
 
        return Ok(
            newStatus ? "Usuario activado correctamente."
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
        // 1. Validar permisos del solicitante
        var requestorRole = requestor.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
 
        if (string.IsNullOrEmpty(requestorRole) || !ManagementRoles.Contains(requestorRole))
            return Fail<KeycloakUserDto>(403, "No tiene permisos para editar usuarios.");
 
        // 2. Si se solicita cambio de rol, verificar jerarquía
        if (!string.IsNullOrWhiteSpace(updateDto.NewRoleName))
        {
            if (!RoleRules.TryGetValue(requestorRole, out var allowedRoles)
                || !allowedRoles.Contains(updateDto.NewRoleName))
                return Fail<KeycloakUserDto>(403,
                    $"No tiene permisos para asignar el rol '{updateDto.NewRoleName}'.");
        }
 
        // 3. Ejecutar la actualización (lanza KeycloakUserNotFoundException si no existe)
        await _kc.UpdateUserAsync(targetUserId, updateDto, ct);
 
        // 4. Retornar el usuario con los datos actualizados
        JsonElement updated = await _kc.GetUserByIdAsync(targetUserId, ct)
            ?? throw new KeycloakUserNotFoundException(targetUserId);
 
        return Ok("Usuario actualizado correctamente.", KeycloakUserMapper.ToDto(updated)!);
    }
 
    // ============================================================
    // HELPERS DE RESPUESTA
    // ============================================================
 
    /// <summary>
    /// Crea un <see cref="ResponseDto{T}"/> de éxito con HTTP 200.
    /// </summary>
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
    private static ResponseDto<T> Fail<T>(int statusCode, string message) => new()
    {
        Status     = false,
        StatusCode = statusCode,
        Message    = message,
        Data       = default
    };
}