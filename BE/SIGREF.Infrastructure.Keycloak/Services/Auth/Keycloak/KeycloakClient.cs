using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SIGREF.Common.Exceptions;
using SIGREF.Common.Models;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

public partial class KeycloakClient : IKeycloakClient
{
    private readonly HttpClient _http;
    private readonly KeycloakOptions _settings;

    private string? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public KeycloakClient(HttpClient http, IOptions<KeycloakOptions> options)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(options?.Value);

        _http     = http;
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.AdminClientId) ||
            string.IsNullOrWhiteSpace(_settings.AdminClientSecret))
            throw new ValidationException(
                "KEYCLOAK_ADMIN_CREDENTIALS_MISSING",
                extraData: new Dictionary<string, object>
                {
                    { "missingFields", new[] { "AdminClientId", "AdminClientSecret" } }
                });
    }

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
        if (IsTokenValid())
            return _cachedToken!;

        await _tokenLock.WaitAsync(ct);
        try
        {
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

            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException(
                    "KEYCLOAK_TOKEN_REQUEST_FAILED",
                    statusCode: (int)response.StatusCode,
                    extraData: new Dictionary<string, object>
                    {
                        { "httpStatus", (int)response.StatusCode },
                        { "tokenUrl",   tokenUrl }
                    });

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

            _cachedToken = json.GetProperty("access_token").GetString()
                ?? throw new ExternalServiceException(
                    "KEYCLOAK_TOKEN_MISSING_ACCESS_TOKEN",
                    extraData: new Dictionary<string, object>
                    {
                        { "tokenUrl", tokenUrl }
                    });

            var expiresIn = json.GetProperty("expires_in").GetInt32();
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