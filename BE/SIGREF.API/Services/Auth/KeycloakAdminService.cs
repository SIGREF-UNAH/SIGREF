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

}
