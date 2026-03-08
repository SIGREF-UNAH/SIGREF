using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SIGREF.Common.Models;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

public partial class KeycloakClient : IKeycloakClient
{
    private readonly HttpClient _http;
    private readonly KeycloakOptions _settings;

    // Cache de Token con bloqueo para hilos
    private string? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public KeycloakClient(HttpClient http, IOptions<KeycloakOptions> options)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(options?.Value);

        _http = http;
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.AdminClientId) || 
            string.IsNullOrWhiteSpace(_settings.AdminClientSecret))
            throw new InvalidOperationException(
                "Faltan credenciales de Admin en la configuración de Keycloak.");
    }

    // Helper central para inyectar el token en cada petición (Thread-Safe)
    private async Task<HttpResponseMessage> SendAuthenticatedAsync(
        HttpMethod method,
        string url,
        object? body = null,
        CancellationToken ct = default)
    {
        var token = await GetAdminTokenAsync(ct);

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        return await _http.SendAsync(request, ct);
    }

    public async Task<string> GetAdminTokenAsync(CancellationToken ct = default)
    {
        // Double-check locking: verificación rápida sin lock
        if (IsTokenValid())
            return _cachedToken!;

        await _tokenLock.WaitAsync(ct);
        try
        {
            // Segunda verificación dentro del lock
            if (IsTokenValid())
                return _cachedToken!;

            var formContent = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type",    "client_credentials"),
                new KeyValuePair<string, string>("client_id",     _settings.AdminClientId),
                new KeyValuePair<string, string>("client_secret", _settings.AdminClientSecret),
            ]);

            var tokenUrl = $"{_settings.BaseUrl}/realms/{_settings.RealmName}" +
                           "/protocol/openid-connect/token";

            var response = await _http.PostAsync(tokenUrl, formContent, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(
                cancellationToken: ct);

            _cachedToken = json.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException(
                    "La respuesta del token no contiene 'access_token'.");

            var expiresIn = json.GetProperty("expires_in").GetInt32();
            // Margen de seguridad de 30s para evitar usar un token a punto de expirar
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 30);

            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private bool IsTokenValid() =>
        _cachedToken is not null && DateTime.UtcNow < _tokenExpiresAt;
}