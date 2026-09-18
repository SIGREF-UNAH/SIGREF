using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SIGREF.Common.Dtos;
using SIGREF.Common.Exceptions;
using SIGREF.Common.Interfaces;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth;

public class KeycloakAdminService : IKeycloakAdminService
{
    private static readonly Dictionary<string, string[]> RoleRules = new()
    {
        { "ti", ["ti", "admin", "auditor", "cashier"] },
        { "admin", ["auditor", "cashier"] },
        { "auditor", [] },
        { "cashier", [] }
    };

    private static readonly HashSet<string> ManagementRoles = ["ti", "admin"];
    private readonly IFhirPractitionerService _fhirBridge;
    private readonly IKeycloakClient _kc;
    private readonly ILogger<KeycloakAdminService> _logger;

    public KeycloakAdminService(
        IKeycloakClient kc,
        IFhirPractitionerService fhirBridge,
        ILogger<KeycloakAdminService> logger)
    {
        _kc = kc;
        _fhirBridge = fhirBridge;
        _logger = logger;
    }

    // ============================================================
    // CREAR USUARIO
    // ============================================================

    public async Task<KeycloakUserDto> CreateUserAsync(
        ClaimsPrincipal creator,
        string username,
        string practitionerId,
        string email,
        string password,
        string[] roles,
        CancellationToken ct = default)
    {
        // Extraer y validar el rol del creador.
        var creatorRole = ExtractRole(creator)
                          ?? throw new ForbiddenException(
                              "CREATOR_ROLE_UNDETERMINED",
                              new Dictionary<string, object>
                              {
                                  { "hint", "El token no contiene un claim de rol válido." }
                              });

        //  Verificar jerarquía: el creador solo puede asignar roles permitidos.
        if (!RoleRules.TryGetValue(creatorRole, out var allowedRoles)
            || roles.Any(r => !allowedRoles.Contains(r)))
            throw new ForbiddenException(
                "ROLE_ASSIGNMENT_NOT_ALLOWED",
                new Dictionary<string, object>
                {
                    { "creatorRole", creatorRole },
                    { "requestedRoles", roles },
                    { "allowedRoles", allowedRoles ?? [] }
                });

        // Validar Practitioner en FHIR.
        var (exists, firstName, lastName, isActive) = await _fhirBridge.GetBasicDataAsync(practitionerId);

        // Entidad no encontrada en FHIR 
        if (!exists)
            throw new NotFoundException(
                "FHIR_PRACTITIONER_NOT_FOUND",
                new Dictionary<string, object> { { "practitionerId", practitionerId } });

        // Regla de negocio practitioner inactivo no puede tener usuario 
        if (!isActive)
            throw new BusinessRuleException(
                "FHIR_PRACTITIONER_INACTIVE",
                new Dictionary<string, object> { { "practitionerId", practitionerId } });

        // 4. Validar existencia de los roles en Keycloak ANTES de crear el usuario.
        await ValidateRolesExistAsync(roles, ct);

        // Verificar que el Practitioner no esté ya vinculado a otro usuario.
        // Vinculación duplicada → ConflictException (409)
        if (await FindUserByPractitionerIdAsync(practitionerId, ct) is not null)
            throw new ConflictException(
                "PRACTITIONER_ALREADY_LINKED",
                new Dictionary<string, object> { { "practitionerId", practitionerId } });

        string? createdUserId = null;
        try
        {
            var kcUser = new
            {
                username,
                email,
                firstName,
                lastName,
                enabled = true,
                credentials = new[]
                {
                    new { type = "password", value = password, temporary = false }
                },
                attributes = new Dictionary<string, string[]>
                {
                    ["practitionerId"] = [practitionerId],
                    ["displayName"] = [$"{firstName} {lastName}".Trim()]
                }
            };

            // Keycloak no devolvió ID 
            createdUserId = await _kc.CreateUserAsync(kcUser, ct)
                            ?? throw new ExternalServiceException(
                                "KEYCLOAK_CREATE_USER_NO_ID_RETURNED",
                                extraData: new Dictionary<string, object> { { "username", username } });

            foreach (var role in roles)
                await _kc.AssignRoleAsync(createdUserId, role, ct);

            // No se pudo recuperar el usuario tras crearlo 
            var created = await _kc.GetUserByIdAsync(createdUserId, ct)
                          ?? throw new ExternalServiceException(
                              "KEYCLOAK_CREATED_USER_NOT_RETRIEVABLE",
                              extraData: new Dictionary<string, object> { { "createdUserId", createdUserId } });

            return KeycloakUserMapper.ToDto(created)!;
        }
        catch
        {
            // ROLLBACK: eliminar el usuario si algo falló tras su creación.
            if (!string.IsNullOrEmpty(createdUserId))
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

            _logger.LogError(
                "Error crítico al crear usuario '{Username}'. Se aplicó rollback para el ID: {UserId}",
                username,
                createdUserId ?? "no asignado");

            throw;
        }
    }

    // ============================================================
    // OBTENER USUARIO POR ID
    // ============================================================

    public async Task<KeycloakUserDto?> GetUserByIdAsync(
        string keycloakUserId,
        CancellationToken ct = default)
    {
        var raw = await _kc.GetUserByIdAsync(keycloakUserId, ct);
        if (raw is null)
            return null;

        var roles = await _kc.GetUserRolesAsync(keycloakUserId, ct);
        return KeycloakUserMapper.ToDto(raw.Value, roles);
    }

    // ============================================================
    // OBTENER MÚLTIPLES USUARIOS POR IDS
    // ============================================================

    public async Task<List<KeycloakUserDto>> GetUsersByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default)
    {
        return await _kc.GetUsersByIdsAsync(userIds, ct);
    }

    // ============================================================
    // OBTENER USUARIO POR PRACTITIONER
    // ============================================================

    public async Task<KeycloakUserDto?> GetUserByPractitionerIdAsync(
        string practitionerId,
        CancellationToken ct = default)
    {
        return await FindUserByPractitionerIdAsync(practitionerId, ct);
    }

    // ============================================================
    // VERIFICAR SI PRACTITIONER TIENE USUARIO
    // ============================================================

    public async Task<bool> PractitionerHasUserAsync(
        string practitionerId,
        CancellationToken ct = default)
    {
        return await FindUserByPractitionerIdAsync(practitionerId, ct) is not null;
    }

    // ============================================================
    // VERIFICAR DISPONIBILIDAD DE USERNAME
    // ============================================================

    public async Task<KeycloakUsernameDto> ExistUserNameAsync(
        string username,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException(
                "USERNAME_REQUIRED",
                new Dictionary<string, object> { { "field", "username" } });

        var matches = await _kc.SearchUsernamesAsync(username, ct);

        var exactExists = matches.Any(u =>
            u.Equals(username, StringComparison.OrdinalIgnoreCase));

        return new KeycloakUsernameDto
        {
            ExistName = exactExists,
            NumberList = matches.Count,
            Usernames = matches
        };
    }

    // ============================================================
    // LISTAR USUARIOS PAGINADOS
    // ============================================================

    public async Task<PagedResultDto<KeycloakUserDto>> GetUsersListAsync(
        KeycloakFilter filter,
        CancellationToken ct = default)
    {
        var page = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize <= 0 ? 10 : Math.Min(filter.PageSize, 30);
        var first = (page - 1) * pageSize;

        // Search tiene precedencia; si está presente los filtros específicos se ignoran
        // para evitar comportamiento ambiguo en Keycloak.
        var search = !string.IsNullOrWhiteSpace(filter.Search) ? filter.Search : null;
        var username = search is null ? filter.UserName : null;
        var email = search is null ? filter.Email : null;
        var firstName = search is null ? filter.FirstName : null;
        var lastName = search is null ? filter.LastName : null;

        // Ambas peticiones se lanzan en paralelo: Keycloak las procesa de forma independiente.
        var usersTask = _kc.GetUsersFilteredAsync(
            first, pageSize, search, username, email, firstName, lastName, ct);

        var countTask = _kc.GetUsersCountAsync(
            search, username, email, firstName, lastName, ct);

        await Task.WhenAll(usersTask, countTask);

        var users = await usersTask;
        var totalItems = await countTask;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResultDto<KeycloakUserDto>
        {
            Items = users,
            Pagination = new PaginationDto
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasPrevious = page > 1,
                HasNext = page < totalPages
            }
        };
    }

    // ============================================================
    // TOGGLE DE ESTADO (ACTIVAR / DESACTIVAR)
    // ============================================================

    public async Task<bool> ToggleUserStatusAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        CancellationToken ct = default)
    {
        ValidateManagementRole(requestor, "activar o desactivar usuarios");

        var requestorId = ExtractUserId(requestor);

        if (requestorId == targetUserId)
            throw new BusinessRuleException(
                "USER_CANNOT_CHANGE_OWN_STATUS",
                new Dictionary<string, object> { { "userId", requestorId ?? "unknown" } });

        return await _kc.ToggleUserStatusAsync(targetUserId, ct);
    }

    // ============================================================
    // EDITAR USUARIO
    // ============================================================

    public async Task<KeycloakUserDto> UpdateUserAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        KeycloakUpdateUserDto updateDto,
        CancellationToken ct = default)
    {
        var requestorRole = ValidateManagementRole(requestor, "editar usuarios");

        if (!string.IsNullOrWhiteSpace(updateDto.NewRoleName)
            && (!RoleRules.TryGetValue(requestorRole, out var allowedRoles)
                || !allowedRoles.Contains(updateDto.NewRoleName)))
            throw new ForbiddenException(
                "ROLE_ASSIGNMENT_NOT_ALLOWED",
                new Dictionary<string, object>
                {
                    { "requestorRole", requestorRole },
                    { "requestedRole", updateDto.NewRoleName },
                    { "allowedRoles", allowedRoles ?? [] }
                });

        await _kc.UpdateUserAsync(targetUserId, updateDto, ct);

        var updated = await _kc.GetUserByIdAsync(targetUserId, ct)
                      ?? throw new NotFoundException(
                          "KEYCLOAK_USER_NOT_FOUND",
                          new Dictionary<string, object> { { "userId", targetUserId } });

        return KeycloakUserMapper.ToDto(updated)!;
    }

    // ============================================================
    // ELIMINAR USUARIO
    // ============================================================

    public async Task DeleteUserAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        CancellationToken ct = default)
    {
        var requestorRole = ValidateManagementRole(requestor, "eliminar usuarios");

        var requestorId = ExtractUserId(requestor);

        if (requestorId == targetUserId)
            throw new BusinessRuleException(
                "USER_CANNOT_DELETE_THEMSELVES",
                new Dictionary<string, object> { { "userId", requestorId ?? "unknown" } });

        var existing = await _kc.GetUserByIdAsync(targetUserId, ct);
        if (existing is null)
            throw new NotFoundException(
                "KEYCLOAK_USER_NOT_FOUND",
                new Dictionary<string, object> { { "userId", targetUserId } });

        await _kc.DeleteUserAsync(targetUserId, ct);

        _logger.LogInformation(
            "Usuario '{TargetUserId}' eliminado por '{RequestorId}' con rol '{Role}'.",
            targetUserId,
            requestorId ?? "desconocido",
            requestorRole);
    }

    // ============================================================
    // HELPERS PRIVADOS COMPARTIDOS
    // ============================================================

    private static string? ExtractRole(ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            is { Length: > 0 } role
            ? role
            : null;
    }

    private static string? ExtractUserId(ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }

    private static string ValidateManagementRole(ClaimsPrincipal requestor, string action)
    {
        var role = ExtractRole(requestor);
        if (string.IsNullOrEmpty(role) || !ManagementRoles.Contains(role))
            throw new ForbiddenException(
                "MANAGEMENT_ROLE_REQUIRED",
                new Dictionary<string, object>
                {
                    { "action", action },
                    { "requiredRoles", ManagementRoles }
                });
        return role;
    }

    private async Task<KeycloakUserDto?> FindUserByPractitionerIdAsync(
        string practitionerId,
        CancellationToken ct)
    {
        var results = await _kc.SearchUsersAsync(practitionerId, ct);

        var found = results.FirstOrDefault(u =>
            u.TryGetProperty("attributes", out var attrs) &&
            attrs.TryGetProperty("practitionerId", out var pid) &&
            pid.ValueKind == JsonValueKind.Array &&
            pid[0].GetString() == practitionerId);

        return found.ValueKind == default
            ? null
            : KeycloakUserMapper.ToDto(found);
    }

    private async Task ValidateRolesExistAsync(string[] roles, CancellationToken ct)
    {
        var kcClient = (KeycloakClient)_kc;
        foreach (var role in roles)
            await kcClient.FetchRealmRoleAsync(role, ct);
    }
}