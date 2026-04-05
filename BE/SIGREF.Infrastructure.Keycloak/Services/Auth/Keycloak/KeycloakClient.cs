using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SIGREF.Common.Models;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

/// <summary>
/// Cliente HTTP para la Admin REST API de Keycloak.
/// </summary>
/// <remarks>
/// <para>
/// Implementa <see cref="IKeycloakClient"/> usando la estrategia de <c>partial class</c>
/// para separar la lógica de autenticación (este archivo) de los servicios de usuario
/// (<c>KeycloakClientServices.cs</c>).
/// </para>
/// <para>
/// El token de administrador se gestiona mediante un caché interno con renovación automática.
/// La estrategia de <em>double-check locking</em> garantiza que solo un hilo solicite
/// un nuevo token cuando el actual expira.
/// </para>
/// </remarks>
public partial class KeycloakClient : IKeycloakClient
{
    private readonly HttpClient _http;
    private readonly KeycloakOptions _settings;
 
    // --- Caché de Token con control de concurrencia ---
    private string? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;
 
    /// <summary>
    /// Semáforo binario que garantiza que solo un hilo renueve el token a la vez.
    /// </summary>
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
 
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="KeycloakClient"/>.
    /// </summary>
    /// <param name="http">
    /// Cliente HTTP inyectado (debe estar configurado con la <c>BaseAddress</c> del servidor Keycloak).
    /// </param>
    /// <param name="options">Opciones de configuración de Keycloak (<see cref="KeycloakOptions"/>).</param>
    /// <exception cref="ArgumentNullException">
    /// Si <paramref name="http"/> u <paramref name="options"/> son nulos.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Si <c>AdminClientId</c> o <c>AdminClientSecret</c> están vacíos en la configuración.
    /// </exception>
    public KeycloakClient(HttpClient http, IOptions<KeycloakOptions> options)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(options?.Value);
 
        _http     = http;
        _settings = options.Value;
 
        if (string.IsNullOrWhiteSpace(_settings.AdminClientId) ||
            string.IsNullOrWhiteSpace(_settings.AdminClientSecret))
            throw new InvalidOperationException(
                "Faltan credenciales de Admin en la configuración de Keycloak. " +
                "Verifique 'AdminClientId' y 'AdminClientSecret'.");
    }
 
    // =========================================================
    // HELPER CENTRAL DE PETICIONES AUTENTICADAS
    // =========================================================
 
    /// <summary>
    /// Envía una petición HTTP autenticada con el token de administrador vigente.
    /// </summary>
    /// <remarks>
    /// Este método centraliza la inyección del header <c>Authorization: Bearer {token}</c>
    /// en todas las llamadas a la Admin API. Si el token está expirado, lo renueva
    /// automáticamente antes de enviar la petición.
    /// </remarks>
    /// <param name="method">Verbo HTTP a usar (<c>GET</c>, <c>POST</c>, <c>PUT</c>, etc.).</param>
    /// <param name="url">URL completa del endpoint de Keycloak.</param>
    /// <param name="body">
    /// Cuerpo de la petición. Se serializa automáticamente como JSON.
    /// Puede ser <c>null</c> para peticiones sin body (GET, DELETE).
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La <see cref="HttpResponseMessage"/> sin validar (el caller decide cómo manejarla).</returns>
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
 
    // =========================================================
    // TOKEN CON CACHÉ Y DOUBLE-CHECK LOCKING
    // =========================================================
 
    /// <inheritdoc/>
    public async Task<string> GetAdminTokenAsync(CancellationToken ct = default)
    {
        // Verificación rápida fuera del lock para evitar contención innecesaria
        if (IsTokenValid())
            return _cachedToken!;
 
        await _tokenLock.WaitAsync(ct);
        try
        {
            // Segunda verificación dentro del lock: otro hilo pudo haber renovado el token
            // mientras este esperaba el semáforo
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
                throw new KeycloakApiException(
                    $"Error al obtener token de administrador. Keycloak respondió con HTTP {(int)response.StatusCode}.",
                    (int)response.StatusCode);
 
            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
 
            _cachedToken = json.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException(
                    "La respuesta del token de Keycloak no contiene el campo 'access_token'.");
 
            var expiresIn = json.GetProperty("expires_in").GetInt32();
 
            // Margen de seguridad de 30 segundos para evitar usar un token en el límite de expiración
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 30);
 
            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }
 
    // =========================================================
    // HELPERS PRIVADOS
    // =========================================================
 
    /// <summary>
    /// Determina si el token en caché sigue siendo válido.
    /// </summary>
    /// <returns>
    /// <c>true</c> si existe un token y su fecha de expiración (con margen) no ha sido alcanzada;
    /// <c>false</c> en caso contrario.
    /// </returns>
    private bool IsTokenValid() =>
        _cachedToken is not null && DateTime.UtcNow < _tokenExpiresAt;
}