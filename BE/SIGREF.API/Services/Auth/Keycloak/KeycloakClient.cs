using System.Net.Http.Headers;
using System.Text.Json;

namespace SIGREF.API.Services.Auth.Keycloak;

public class KeycloakClient : IKeycloakClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _realm;
    private readonly string _clientId;
    private readonly string _clientSecret;
    
    
    // ================================
    // TOKEN CACHE (COMPARTIDO)
    // ================================
    private static string? _cachedToken;
    private static DateTime _tokenExpiresAt = DateTime.MinValue;
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

    public KeycloakClient(HttpClient http, IConfiguration config)
    {
        _http = http;

        // BASE URL DEL SERVIDOR KEYCLOAK (sin /realms)
        _baseUrl = config["Keycloak:BaseUrl"]
                   ?? config["Keycloak:Url"]        
                   ?? "http://localhost:8080";      


        // NOMBRE DEL REALM
        _realm = config["Keycloak:Realm"]
                 ?? config["Keycloak:RealmName"]    
                 ?? "sigref";


        // CREDENCIALES DEL CLIENTE ADMIN (OBLIGATORIO)
        _clientId = config["Keycloak:AdminClientId"]
                    ?? throw new Exception("AdminClientId missing in configuration.");

        _clientSecret = config["Keycloak:AdminClientSecret"]
                        ?? throw new Exception("AdminClientSecret missing in configuration.");
    }


    // ======================================================
    // TOKEN (OPTIMIZADO CON CACHE)
    // ======================================================
    public async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
        // Si existe un token válido, se reutiliza
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiresAt)
            return _cachedToken;

        await _tokenLock.WaitAsync(ct);
        try
        {
            // Revísalo de nuevo por si otro thread ya lo renovó
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiresAt)
                return _cachedToken;

            // Solicitar token nuevo
            var url = $"{_baseUrl}/realms/{_realm}/protocol/openid-connect/token";

            var form = new Dictionary<string, string>
            {
                ["grant_type"]    = "client_credentials",
                ["client_id"]     = _clientId,
                ["client_secret"] = _clientSecret
            };

            var response = await _http.PostAsync(url, new FormUrlEncodedContent(form), ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

            _cachedToken = json.GetProperty("access_token").GetString()!;
            var expiresIn = json.GetProperty("expires_in").GetInt32();

            // Renovar un poco antes del expiry 
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 10);

            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task SetAuth(CancellationToken ct)
    {
        var token = await GetAdminTokenAsync(ct);

        // limpiar antes
        _http.DefaultRequestHeaders.Authorization = null;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // ======================================================
    // SEARCH — optimizado
    // ======================================================
    public async Task<List<JsonElement>> SearchUsersAsync(string search, CancellationToken ct)
    {
        await SetAuth(ct);

        var url = $"{_baseUrl}/admin/realms/{_realm}/users?search={Uri.EscapeDataString(search)}";
        var result = await _http.GetFromJsonAsync<List<JsonElement>>(url, ct);

        return result ?? new List<JsonElement>();
    }

    // ======================================================
    // SEARCH BY USERNAME
    // ======================================================
    public async Task<JsonElement?> SearchUserByUsernameAsync(string username, CancellationToken ct)
    {
        await SetAuth(ct);

        var url = $"{_baseUrl}/admin/realms/{_realm}/users?username={Uri.EscapeDataString(username)}";

        var list = await _http.GetFromJsonAsync<List<JsonElement>>(url, ct);
        return list?.FirstOrDefault();
    }

    // ======================================================
    // SEARCH BY EMAIL
    // ======================================================
    public async Task<JsonElement?> SearchUserByEmailAsync(string email, CancellationToken ct)
    {
        await SetAuth(ct);

        var url = $"{_baseUrl}/admin/realms/{_realm}/users?email={Uri.EscapeDataString(email)}";

        var list = await _http.GetFromJsonAsync<List<JsonElement>>(url, ct);
        return list?.FirstOrDefault();
    }

    // ======================================================
    // PAGINACIÓN REAL
    // ======================================================
    public async Task<List<JsonElement>> GetUsersPaginatedAsync(int first, int max, CancellationToken ct)
    {
        await SetAuth(ct);

        var url = $"{_baseUrl}/admin/realms/{_realm}/users?first={first}&max={max}";
        var list = await _http.GetFromJsonAsync<List<JsonElement>>(url, ct);

        return list ?? new List<JsonElement>();
    }

    // ======================================================
    // GET BY ID
    // ======================================================
    public async Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct)
    {
        await SetAuth(ct);

        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}";

        try
        {
            return await _http.GetFromJsonAsync<JsonElement>(url, ct);
        }
        catch
        {
            return null;
        }
    }

    // ======================================================
    // CREATE USER
    // ======================================================
    public async Task<string?> CreateUserAsync(object kcUser, CancellationToken ct)
    {
        await SetAuth(ct);

        var url = $"{_baseUrl}/admin/realms/{_realm}/users";

        var res = await _http.PostAsJsonAsync(url, kcUser, ct);

        if (!res.IsSuccessStatusCode)
            return null;

        var location = res.Headers.Location?.ToString();
        return location?.Split('/').Last();
    }

    // ======================================================
    // ASSIGN ROLE
    // ======================================================
    public async Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        await SetAuth(ct);

        var roleUrl = $"{_baseUrl}/admin/realms/{_realm}/roles/{roleName}";
        var role = await _http.GetFromJsonAsync<JsonElement>(roleUrl, ct);

        var mappingUrl =
            $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";

        var response = await _http.PostAsJsonAsync(mappingUrl, new[] { role }, ct);

        return response.IsSuccessStatusCode;
    }
}
