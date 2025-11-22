using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Constants;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.UserLink;
using SIGREF.API.Extensions;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;

namespace SIGREF.API.Services.Auth;

public class KeycloakAdminService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly SIGREFContext _sigrefDb;
    private readonly FhirClient _fhirService;

    // Cache local del token admin
    private static string? _cachedToken;
    private static DateTime _tokenExpiry = DateTime.MinValue;

    private readonly string _kcBaseUrl;
    private readonly string _kcRealm;

    // =================================================
    // Reglas de creacion de roles
    //====================================================
    private static readonly Dictionary<string, string[]> RoleCreationRules = new()
    {
        {
            RolesConstants.ti,
            new[] { RolesConstants.ti, RolesConstants.admin, RolesConstants.auditor, RolesConstants.cashier }
        },
        { RolesConstants.admin, new[] { RolesConstants.auditor, RolesConstants.cashier } },
        { RolesConstants.auditor, Array.Empty<string>() },
        { RolesConstants.cashier, Array.Empty<string>() }
    };
    
    

    public KeycloakAdminService(
        HttpClient http,
        IConfiguration config,
        SIGREFContext db,
        FhirClient fhirService)
    {

        _http = http;
        _config = config;
        _sigrefDb = db;
        _fhirService = fhirService;

        //Fhir client


        // Urls de sigref 
        _kcBaseUrl = _config["Keycloak:BaseUrl"] ?? "http://localhost:8081";
        _kcRealm = _config["Keycloak:Realm"] ?? "sigref";
    }

    // ===============================================================
    // TOKEN ADMIN 
    // Admite cache por si hay multiples pedidos en un mismo instante
    // ===============================================================
    private async Task<string> GetAdminTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        var clientId = _config["Keycloak:AdminClientId"] ?? "sigref-admin-api";
        var clientSecret = _config["Keycloak:AdminClientSecret"] ?? "sigref-admin-secret";

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };

        var tokenUrl = $"{_kcBaseUrl}/realms/{_kcRealm}/protocol/openid-connect/token";
        var res = await _http.PostAsync(tokenUrl, new FormUrlEncodedContent(form));
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        _cachedToken = json.GetProperty("access_token").GetString();
        var expiresIn = json.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 30);

        return _cachedToken!;
    }


    private void SetAuthHeader(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    //=============================================
    // Para validar el rol que se asigna y si tiene permisos de asignar dicho rol
    //==============================================
    private static bool CanCreate(string creatorRole, string[] requestedRoles)
    {
        return RoleCreationRules.TryGetValue(creatorRole, out var allowed)
               && requestedRoles.All(allowed.Contains);
    }

    // ===============================================================
    // CREAR USUARIO (Keycloak + tabla local UserLink)
    // Se crea el usuario en Keycloak y luego se agrega en la tabla UserLink interna
    // ===============================================================
    public async Task<ResponseDto<UserLinkDto>> CreateUserAsync(
        ClaimsPrincipal creator,
        string username,
        string practitionerId,
        string email,
        string password,
        string[] roles)
    {
        // ================================================================
        // 0. Validar rol del creador
        // ================================================================
        var creatorRole = creator.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (creatorRole is null)
            return new ResponseDto<UserLinkDto>
            {
                Message = "No se pudo determinar el rol del creador.",
                StatusCode = 401,
                Status = false,
                Data = null
            };

        if (!CanCreate(creatorRole, roles))
            return new ResponseDto<UserLinkDto>
            {
                Message = $"El rol '{creatorRole}' no tiene permisos para crear usuarios con esos roles.",
                StatusCode = 403,
                Status = false,
                Data = null
            };

        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);


        // 1. Validar si el practitioner YA esta vinculado a un usuario
        // ================================================================
        var linkedUser = await _sigrefDb.UserLinks
            .FirstOrDefaultAsync(u => u.PractitionerId == practitionerId);

        if (linkedUser is not null)
            return new ResponseDto<UserLinkDto>
            {
                Message = $"El Practitioner '{practitionerId}' ya está vinculado al usuario '{linkedUser.Username}'.",
                StatusCode = 400,
                Status = false,
                Data = null
            };
        // ================================================================
        // 2. Validar Practitioner en HAPI FHIR
        // ================================================================
        FhirPractitioner? practitioner = null;

        try
        {
            practitioner = await _fhirService.ReadAsync<FhirPractitioner>($"Practitioner/{practitionerId}");
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            return new ResponseDto<UserLinkDto>
            {
                Message = $"El Practitioner con ID '{practitionerId}' no existe en el servidor FHIR.",
                StatusCode = 404,
                Status = false,
                Data = null
            };
        }

        // ================================================================
        // 3. Verificar si usuario ya existe en Keycloak
        // ================================================================
        var searchUrl = $"{_kcBaseUrl}/admin/realms/{_kcRealm}/users?search={username}";
        var existingUsers = await _http.GetFromJsonAsync<List<JsonElement>>(searchUrl);

        var alreadyExists = existingUsers?.Any(u =>
        {
            var uname = u.GetProperty("username").GetString();
            var mail = u.TryGetProperty("email", out var e) ? e.GetString() : null;

            return string.Equals(uname, username, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(mail, email, StringComparison.OrdinalIgnoreCase);

        }) ?? false;

        if (alreadyExists)
            return new ResponseDto<UserLinkDto>
            {
                Message = $"Ya existe un usuario con ese nombre o correo ({username} / {email}).",
                StatusCode = 400,
                Status = false,
                Data = null
            };

        // ================================================================
        //  Extraer nombres desde Practitioner FHIR
        // ================================================================
        string firstName = "";
        string middleNames = "";
        string lastName = "";
        string displayName;

        if (practitioner?.Name != null && practitioner.Name.Any())
        {
            var name = practitioner.Name.First();

            var given = name.Given?.ToList() ?? new List<string>();
            lastName = name.Family ?? "";

            if (given.Count > 0)
                firstName = given[0];

            if (given.Count > 1)
                middleNames = string.Join(" ", given.Skip(1));

            displayName = ($"{string.Join(" ", given)} {lastName}").Trim();
        }
        else
        {
            // fallback
            displayName = username;
            firstName = username;
        }

        // ================================================================
        // 4. Crear usuario en Keycloak
        // ================================================================
        var kcUser = new
        {
            username,
            email,
            enabled = true,
            emailVerified = true,
            firstName,
            lastName,

            // Atributos extra para visualizar desde el panel de keycloack
            attributes = new Dictionary<string, object?>
            {
                { "displayName", displayName },
                { "practitionerId", practitionerId },
                { "createdBy", "SIGREF_API" }
            },

            credentials = new[]
            {
                new { type = "password", value = password, temporary = false }
            }
        };


        var res = await _http.PostAsJsonAsync($"{_kcBaseUrl}/admin/realms/{_kcRealm}/users", kcUser);

        if (!res.IsSuccessStatusCode)
            return new ResponseDto<UserLinkDto>
            {
                Message = $"Error HTTP {(int)res.StatusCode} al crear el usuario en Keycloak.",
                StatusCode = (int)res.StatusCode,
                Status = false,
                Data = null
            };

        var location = res.Headers.Location?.ToString();
        var userId = location?.Split('/').Last();


        // ================================================================
        // 5. Asignar roles
        // ================================================================
        foreach (var roleName in roles)
        {
            try
            {
                var roleInfo = await _http.GetFromJsonAsync<JsonElement>(
                    $"{_kcBaseUrl}/admin/realms/{_kcRealm}/roles/{roleName}");

                await _http.PostAsJsonAsync(
                    $"{_kcBaseUrl}/admin/realms/{_kcRealm}/users/{userId}/role-mappings/realm",
                    new[] { roleInfo }
                );
            }
            catch
            {
                // Ignorar si el rol no existe
            }
        }


        // ================================================================
        // 6. Crear registro local en UserLinks
        // ================================================================
        var newLocalUser = new UserLinkEntity
        {
            KeycloakUserId = userId!,
            PractitionerId = practitionerId,
            Username = username,
            Email = email,
            DisplayName = displayName,
            Active = true,
            SyncStatus = "Created", // pasar a constante
            LastSync = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _sigrefDb.UserLinks.Add(newLocalUser);
        await _sigrefDb.SaveChangesAsync();


        // ================================================================
        // Respojse
        // ================================================================
        return new ResponseDto<UserLinkDto>
        {
            Message = $"Usuario '{username}' creado correctamente y vinculado al Practitioner '{practitionerId}'.",
            Status = true,
            StatusCode = 200,
            Data = newLocalUser.ToDto()
        };
    }


    // ============================================================
    // BUSCAR USUARIOS (SOLO TABLA LOCAL user_links)
    // =============================================================
    public async Task<ResponseDto<PaginationDtoSigref<UserLinkDto>>> SearchUsersAsync(
        string? search = null,
        bool? enabled = null,
        int first = 0,
        int max = 20)
    {

        if (max <= 0) max = 20;
        if (max > 200) max = 200;
        if (first < 0) first = 0;

        var query = _sigrefDb.UserLinks.AsQueryable();

        // ======================================================
        // Filtro por activo / inactivo
        // ======================================================
        if (enabled.HasValue)
            query = query.Where(u => u.Active == enabled.Value);

        // ======================================================
        // Busqueda general
        // ======================================================
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();

            query = query.Where(u =>
                (u.Username != null && EF.Functions.ILike(u.Username, $"%{s}%")) ||
                (u.Email != null && EF.Functions.ILike(u.Email, $"%{s}%")) ||
                (u.DisplayName != null && EF.Functions.ILike(u.DisplayName, $"%{s}%"))
            );
        }

        // ======================================================
        // Total sin paginar
        // ======================================================
        var total = await query.CountAsync();

        // ======================================================
        // Traer usuarios paginados DIRECTO a DTO
        // ======================================================
        var users = await query
            .OrderBy(u => u.Username)
            .Skip(first)
            .Take(max)
            .Select(UserLinkExtensions.ToDtoProjection())
            .ToListAsync();

        // ======================================================
        // Calcular paginacion
        // ======================================================
        var currentPage = (first / max) + 1;
        var totalPages = (int)Math.Ceiling((double)total / max);

        // ======================================================
        // Respuesta final
        // ======================================================
        return new ResponseDto<PaginationDtoSigref<UserLinkDto>>
        {
            Status = true,
            StatusCode = 200,
            Message = $"Usuarios encontrados: {users.Count}",
            Data = new PaginationDtoSigref<UserLinkDto>
            {
                TotalItems = total,
                CurrentPage = currentPage,
                PageSize = max,
                TotalPages = totalPages,
                Items = users
            }
        };
    }
}