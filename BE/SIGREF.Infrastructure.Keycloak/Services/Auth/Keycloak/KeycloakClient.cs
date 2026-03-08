using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SIGREF.Common.Models;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

public partial class KeycloakClient : IKeycloakClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _realm;
    private readonly string _clientId;
    private readonly string _clientSecret;

    // Cache de Token con bloqueo para hilos
    private string? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public KeycloakClient(HttpClient http, IOptions<KeycloakOptions> options)
    {
        _http = http;
        var settings = options.Value;

        _baseUrl = settings.BaseUrl;
        _realm = settings.RealmName;
        _clientId = settings.AdminClientId;
        _clientSecret = settings.AdminClientSecret;

        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
            throw new Exception("Faltan credenciales de Admin en la configuración de Keycloak.");
    }

    // Helper central para inyectar el token en cada petición (Thread-Safe)
    private async Task<HttpResponseMessage> SendAuthenticatedAsync(
        HttpMethod method, 
        string url, 
        object? body = null, 
        CancellationToken ct = default)
    {
        var token = await GetAdminTokenAsync(ct);
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body != null)
            request.Content = JsonContent.Create(body);

        return await _http.SendAsync(request, ct);
    }

    public async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiresAt)
            return _cachedToken;

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_cachedToken != null && DateTime.UtcNow < _tokenExpiresAt)
                return _cachedToken;

            var dict = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _clientId },
                { "client_secret", _clientSecret }
            };

            var res = await _http.PostAsync($"{_baseUrl}/realms/{_realm}/protocol/openid-connect/token", 
                new FormUrlEncodedContent(dict), ct);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadFromJsonAsync<JsonElement>(ct);
            _cachedToken = json.GetProperty("access_token").GetString();
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(json.GetProperty("expires_in").GetInt32() - 10);

            return _cachedToken!;
        }
        finally
        {
            _tokenLock.Release();
        }
    }
}