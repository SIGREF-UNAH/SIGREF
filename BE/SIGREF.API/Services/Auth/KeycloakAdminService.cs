using System.Security.Claims;
using Hl7.Fhir.Rest;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using SIGREF.API.Dtos.Auth;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Services.Auth.Keycloak;
using System.Text.Json;
namespace SIGREF.API.Services.Auth;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IKeycloakClient _kc;
    private readonly FhirClient _fhir;
   // private readonly IConfiguration _config;

    private static readonly Dictionary<string, string[]> RoleRules = new()
    {
        { "ti",     new[] { "ti", "admin", "auditor", "cashier" } },
        { "admin",  new[] { "auditor", "cashier" } },
        { "auditor", Array.Empty<string>() },
        { "cashier", Array.Empty<string>() }
    };

    public KeycloakAdminService(
        IKeycloakClient kc,
        FhirClient fhir
        )
    {
        _kc = kc;
        _fhir = fhir;
        //_config = config;
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

        if (creatorRole is null)
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

        // 2. Validar que el Practitioner exista en FHIR
        FhirPractitioner prac;
        try
        {
            prac = await _fhir.ReadAsync<FhirPractitioner>($"Practitioner/{practitionerId}");
        }
        catch
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 404,
                Message = $"El Practitioner '{practitionerId}' no existe en FHIR.",
                Data = null
            };
        }

        // 3. Validar que ese Practitioner NO esté vinculado ya
        var matches = await _kc.SearchUsersAsync(practitionerId, ct);

        bool exists = matches.Any(u =>
            u.TryGetProperty("attributes", out var attrs)
            && attrs.TryGetProperty("practitionerId", out var pid)
            && pid.ValueKind == JsonValueKind.Array
            && pid[0].GetString() == practitionerId
        );

        if (exists)
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "El Practitioner ya está vinculado a un usuario.",
                Data = null
            };
        }

        // 4. Preparar nombres desde FHIR
        var name = prac!.Name?.FirstOrDefault();

        string fn = name?.Given?.FirstOrDefault() ?? username;
        string ln = name?.Family ?? "";
        string displayName = $"{fn} {ln}".Trim();



        // 5. Crear objeto del usuario en Keycloak
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

        // 6. Crear usuario en Keycloak
        var userId = await _kc.CreateUserAsync(kcUser, ct);

        if (userId is null)
        {
            return new ResponseDto<KeycloakUserDto>
            {
                Status = false,
                StatusCode = 500,
                Message = "No se pudo crear el usuario en Keycloak.",
                Data = null
            };
        }

        // 7. Asignar roles en Keycloak
        foreach (var role in roles)
            await _kc.AssignRoleAsync(userId, role, ct);

        // 8. Obtener usuario final desde Keycloak
        var raw = await _kc.GetUserByIdAsync(userId, ct);

        return new ResponseDto<KeycloakUserDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Usuario creado correctamente.",
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
                Message = "Usuario no encontrado.",
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
        // Buscar por practitionerId usando SEARCH (rápido)
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
                ? "El practitioner ya está asignado a un usuario."
                : "El practitioner está disponible.",
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
                Message = "El username es obligatorio.",
                Data = null
            };
        }
        //  Obtener lista de usernames similares (máx: 5)
        var matches = await _kc.SearchUsernamesAsync(username, ct);

        //  Detectar si el exacto está dentro de esos usuarios
        bool exactExists = matches.Any(u => 
            u.Equals(username, StringComparison.OrdinalIgnoreCase));

        // Crear DTO final
        var dto = new KeycloakUsernameDto
        {
            ExistName = exactExists,
            NumberList = matches.Count,    // cantidad de similares
            Usernames = matches            // lista completa de 5 nombres
        };

        return new ResponseDto<KeycloakUsernameDto>
        {
            Status = true,
            StatusCode = 200,
            Message = exactExists
                ? "Existe un usuario con ese username."
                : "El username exacto NO existe, pero hay similares.",
            Data = dto
        };
    }
    
    public async Task<ResponseDto<PagedResultDto<KeycloakUserDto>>> GetUsersListAsync( KeycloakFilter  filter)
    {
        // ============================
        // VALIDACIONES
        // ============================

        // Si no viene número de página default = 1
        int page = filter.PageNumber <= 0 ? 1 : filter.PageNumber;

        // PageSize por defecto = 10
        int pageSize =
            filter.PageSize <= 0 ? 10 :          // si viene vacío 10
            filter.PageSize > 30 ? 30 :          // si viene > 30  30
            filter.PageSize;                     // si es válido  el mismo

        // ============================
        // CÁLCULO DE PAGINACIÓN REAL
        // ============================

        int first = (page - 1) * pageSize;
        int max = pageSize + 1; // pedimos uno más para detectar "next page"

        // Obtener usuarios paginados desde Keycloak
        var users = await _kc.GetUsersFilteredAsync(
            first,
            max,
            filter.UserName, 
            CancellationToken.None
        );

        // Saber si hay siguiente página (si vinieron más de pageSize)
        bool hasNext = users.Count > filter.PageSize;

        // Si vino el extra, lo eliminamos porque no pertenece a esta página
        if (hasNext)
            users.RemoveAt(users.Count - 1);

        var pagination = new PaginationDto
        {
            CurrentPage = filter.PageNumber,
            PageSize = filter.PageSize,

            // Para Keycloak NO existen estos valores, así que los dejamos null
            TotalItems = null,
            TotalPages = null,

            HasPrevious = filter.PageNumber > 1,
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
            Message = "Usuarios obtenidos correctamente.",
            Data = paged
        };
    }
}
