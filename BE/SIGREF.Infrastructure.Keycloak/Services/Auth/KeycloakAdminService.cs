using System.Security.Claims;
using System.Text.Json;
using SIGREF.Common.Dtos;
using SIGREF.Common.Interfaces;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IKeycloakClient _kc;
    private readonly IFhirPractitionerService _fhirBridge;

    private static readonly Dictionary<string, string[]> RoleRules = new()
    {
        { "ti",      new[] { "ti", "admin", "auditor", "cashier" } },
        { "admin",   new[] { "auditor", "cashier" } },
        { "auditor", Array.Empty<string>() },
        { "cashier", Array.Empty<string>() }
    };

    public KeycloakAdminService(IKeycloakClient kc, IFhirPractitionerService fhirBridge)
    {
        _kc = kc;
        _fhirBridge = fhirBridge;
    }

    // ============================================================
    // CREATE USER
    // ============================================================
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
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 401,
                Message = "No se pudo determinar el rol del creador.",
                Data = null
            };
        }

        if (!RoleRules.TryGetValue(creatorRole, out var allowedRoles)
            || roles.Any(r => !allowedRoles.Contains(r)))
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 403,
                Message = "El creador no tiene permiso para asignar esos roles.",
                Data = null
            };
        }

        // 2. Validar que el Practitioner exista y esté activo en FHIR vía Bridge
        var (exists, firstName, lastName, isActive) = await _fhirBridge.GetBasicDataAsync(practitionerId);

        if (!exists)
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 404,
                Message = $"El Practitioner '{practitionerId}' no existe en el sistema de salud.",
                Data = null
            };
        }

        if (!isActive)
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "No se puede crear un usuario para un médico que está inactivo en FHIR.",
                Data = null
            };
        }

        // 3. Validar que ese Practitioner NO esté vinculado ya en Keycloak
        var matches = await _kc.SearchUsersAsync(practitionerId, ct);

        bool isAlreadyLinked = matches.Any(u =>
            u.TryGetProperty("attributes", out var attrs)
            && attrs.TryGetProperty("practitionerId", out var pid)
            && pid.ValueKind == JsonValueKind.Array
            && pid[0].GetString() == practitionerId
        );

        if (isAlreadyLinked)
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "El Practitioner ya está vinculado a un usuario existente.",
                Data = null
            };
        }

        // 4. Preparar datos (Si FHIR no devuelve nombres, usamos el username)
        string fn = firstName ?? username;
        string ln = lastName ?? "";
        string displayName = $"{fn} {ln}".Trim();

        // 5. Crear objeto del usuario para Keycloak
        var kcUser = new
        {
            username,
            email,
            enabled = true,
            emailVerified = true,
            firstName = fn,
            lastName = ln,
            credentials = new[]
            {
                new { type = "password", value = password, temporary = false }
            },
            attributes = new Dictionary<string, object?>
            {
                { "practitionerId", practitionerId },
                { "displayName", displayName }
            }
        };

        // 6. Ejecutar creación
        var userId = await _kc.CreateUserAsync(kcUser, ct);

        if (userId is null)
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 500,
                Message = "Error de comunicación con el servidor de identidad al crear el usuario.",
                Data = null
            };
        }

        // 7. Asignar roles
        foreach (var role in roles)
            await _kc.AssignRoleAsync(userId, role, ct);

        // 8. Retornar el usuario final mapeado
        var raw = await _kc.GetUserByIdAsync(userId, ct);

        return new ResponseDto<KeycloakUserDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Usuario creado y vinculado correctamente.",
            Data = KeycloakUserMapper.ToDto(raw!.Value)
        };
    }

    // ============================================================
    // GET USER BY ID
    // ============================================================
    public async Task<ResponseDto<KeycloakUserDto?>> GetUserByIdAsync(
        string keycloakUserId,
        CancellationToken ct = default)
    {
        var raw = await _kc.GetUserByIdAsync(keycloakUserId, ct);

        if (raw is null)
        {
            return new ResponseDto<KeycloakUserDto?>
            {
                Status = true,
                StatusCode = 200,
                Message = "Usuario no encontrado.",
                Data = null
            };
        }

        return new ResponseDto<KeycloakUserDto?>
        {
            Status = true,
            StatusCode = 200,
            Message = "Usuario encontrado.",
            Data = KeycloakUserMapper.ToDto(raw.Value)
        };
    }

    // ============================================================
    // GET USER BY PRACTITIONER — OPTIMIZADO
    // ============================================================
    public async Task<ResponseDto<KeycloakUserDto?>> GetUserByPractitionerIdAsync(
        string practitionerId,
        CancellationToken ct = default)
    {
        var results = await _kc.SearchUsersAsync(practitionerId, ct);

        var found = results.FirstOrDefault(u =>
            u.TryGetProperty("attributes", out var attrs)
            && attrs.TryGetProperty("practitionerId", out var pid)
            && pid.ValueKind == JsonValueKind.Array
            && pid[0].GetString() == practitionerId
        );

        if (found.ValueKind == default)
        {
            return new ResponseDto<KeycloakUserDto?>
            {
                Status = true,
                StatusCode = 200,
                Message = "Usuario no encontrado para este médico.",
                Data = null
            };
        }

        return new ResponseDto<KeycloakUserDto?>
        {
            Status = true,
            StatusCode = 200,
            Message = "Usuario encontrado.",
            Data = KeycloakUserMapper.ToDto(found)
        };
    }

    public async Task<ResponseDto<bool>> PractitionerHasUserAsync(
        string practitionerId,
        CancellationToken ct = default)
    {
        var results = await _kc.SearchUsersAsync(practitionerId, ct);

        bool exists = results.Any(u =>
            u.TryGetProperty("attributes", out var attrs)
            && attrs.TryGetProperty("practitionerId", out var pid)
            && pid.ValueKind == JsonValueKind.Array
            && pid[0].GetString() == practitionerId
        );

        return new ResponseDto<bool>
        {
            Status = true,
            StatusCode = 200,
            Message = exists
                ? "El médico ya tiene un usuario asignado."
                : "El médico no tiene usuario vinculado.",
            Data = exists
        };
    }

    public async Task<ResponseDto<KeycloakUsernameDto>> ExistUserNameAsync(
        string username,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new ResponseDto<KeycloakUsernameDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "El nombre de usuario es requerido.",
                Data = null
            };
        }

        var matches = await _kc.SearchUsernamesAsync(username, ct);

        bool exactExists = matches.Any(u => 
            u.Equals(username, StringComparison.OrdinalIgnoreCase));

        var dto = new KeycloakUsernameDto
        {
            ExistName = exactExists,
            NumberList = matches.Count,
            Usernames = matches
        };

        return new ResponseDto<KeycloakUsernameDto>
        {
            Status = true,
            StatusCode = 200,
            Message = exactExists
                ? "El nombre de usuario ya está en uso."
                : "Nombre de usuario disponible.",
            Data = dto
        };
    }

    public async Task<ResponseDto<PagedResultDto<KeycloakUserDto>>> GetUsersListAsync(KeycloakFilter filter)
    {
        int page = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        int pageSize = filter.PageSize <= 0 ? 10 : (filter.PageSize > 30 ? 30 : filter.PageSize);

        int first = (page - 1) * pageSize;
        int max = pageSize + 1; 

        var users = await _kc.GetUsersFilteredAsync(
            first,
            max,
            filter.UserName, 
            CancellationToken.None
        );

        bool hasNext = users.Count > pageSize;

        if (hasNext)
            users.RemoveAt(users.Count - 1);

        var pagination = new PaginationDto
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = null, // Keycloak admin API no devuelve el total real en listas filtradas fácilmente
            TotalPages = null, // No existen en keycloak
            HasPrevious = page > 1,
            HasNext = hasNext
        };

        var paged = new PagedResultDto<KeycloakUserDto>
        {
            Items = users,
            Pagination = pagination
        };

        return new ResponseDto<PagedResultDto<KeycloakUserDto>>
        {
            Status = true,
            StatusCode = 200,
            Message = "Lista de usuarios obtenida.",
            Data = paged
        };
    }
}