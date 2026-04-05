using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

/// <summary>
/// Implementación de los servicios de usuario de la Admin REST API de Keycloak.
/// </summary>
/// <remarks>
/// Este archivo es la parte de servicios de la <c>partial class</c> <see cref="KeycloakClient"/>.
/// Contiene toda la lógica de búsqueda, creación, edición y gestión del ciclo de vida
/// de los usuarios en el realm configurado.
/// </remarks>
public partial class KeycloakClient : IKeycloakClient
{
    // ======================================================
    // BÚSQUEDAS
    // ======================================================
 
    /// <inheritdoc/>
    public async Task<List<JsonElement>> SearchUsersAsync(string search, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?search={Uri.EscapeDataString(search)}";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al buscar usuarios con término '{search}'. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        return await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];
    }
 
    /// <inheritdoc/>
    public async Task<List<string>> SearchUsernamesAsync(string username, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?search={Uri.EscapeDataString(username)}&first=0&max=20";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al buscar usernames con término '{username}'. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];
 
        return list
            .Select(u => u.TryGetProperty("username", out var un) ? un.GetString() : null)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u!)
            .ToList();
    }
 
    /// <inheritdoc/>
    public async Task<JsonElement?> SearchUserByEmailAsync(string email, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?email={Uri.EscapeDataString(email)}&exact=true";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al buscar usuario por email '{email}'. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct);
        return list?.FirstOrDefault();
    }
 
    // ======================================================
    // OBTENER POR ID
    // ======================================================
 
    /// <inheritdoc/>
    public async Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
 
        // HTTP 404 es el único caso silencioso: el usuario simplemente no existe
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al obtener usuario con ID '{userId}'. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
    }
 
    // ======================================================
    // OBTENER MÚLTIPLES USUARIOS POR IDS
    // ======================================================
 
    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Utiliza el endpoint de búsqueda de Keycloak con la sintaxis <c>id:uuid1 uuid2 uuid3</c>,
    /// disponible desde la versión <b>26.3.0</b>. Esto resuelve todos los IDs en
    /// <b>una única petición HTTP</b>, a diferencia del enfoque de N peticiones paralelas.
    /// </para>
    /// <para>
    /// Los IDs que no existan en Keycloak son ignorados silenciosamente por el propio servidor;
    /// el resultado solo contiene los usuarios encontrados.
    /// </para>
    /// <para>
    /// Si la colección <paramref name="userIds"/> está vacía, retorna una lista vacía
    /// sin realizar ninguna petición.
    /// </para>
    /// </remarks>
    public async Task<List<KeycloakUserDto>> GetUsersByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken ct)
    {
        var idList = userIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
 
        if (idList.Count == 0)
            return [];
 
        // Sintaxis nativa de Keycloak 26.3+: "id:uuid1 uuid2 uuid3"
        // El primer ID lleva el prefijo "id:", los restantes se separan por espacio.
        var searchQuery = "id:" + string.Join(" ", idList);
 
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?search={Uri.EscapeDataString(searchQuery)}&max={idList.Count}";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al obtener usuarios por IDs. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        var rawUsers = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];
 
        return rawUsers
            .Select(KeycloakUserMapper.ToDto)
            .Where(dto => dto is not null)
            .ToList()!;
    }
 
    // ======================================================
    // CREAR USUARIO
    // ======================================================
 
    /// <inheritdoc/>
    public async Task<string?> CreateUserAsync(object kcUser, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Post, url, kcUser, ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al crear el usuario en Keycloak. HTTP {(int)response.StatusCode}. " +
                $"Verifique que no exista un usuario con el mismo username o email.",
                (int)response.StatusCode);
 
        // Keycloak devuelve el ID del nuevo usuario en el header Location: .../users/{id}
        return response.Headers.Location?.ToString().Split('/').Last();
    }
 
    // ======================================================
    // TOGGLE DE ESTADO (ACTIVAR / DESACTIVAR)
    // ======================================================
 
    /// <inheritdoc/>
    public async Task<bool> ToggleUserStatusAsync(string userId, CancellationToken ct)
    {
        // Obtener estado actual del usuario
        JsonElement existing = await GetUserByIdAsync(userId, ct)
            ?? throw new KeycloakUserNotFoundException(userId);
 
        var currentEnabled = existing.GetProperty("enabled").GetBoolean();
        var newEnabled      = !currentEnabled;
 
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";
 
        var patch = new { enabled = newEnabled };
        var response = await SendAuthenticatedAsync(HttpMethod.Put, url, patch, ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al cambiar el estado del usuario '{userId}' a {(newEnabled ? "activo" : "inactivo")}. " +
                $"HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        return newEnabled;
    }
 
    // ======================================================
    // ROLES
    // ======================================================
 
    /// <inheritdoc/>
    public async Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var role = await FetchRealmRoleAsync(roleName, ct);
 
        var mappingUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}" +
                         $"/users/{userId}/role-mappings/realm";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Post, mappingUrl, new[] { role }, ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al asignar el rol '{roleName}' al usuario '{userId}'. " +
                $"HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        return true;
    }
 
    /// <inheritdoc/>
    public async Task UpdateUserRoleAsync(string userId, string newRoleName, CancellationToken ct)
    {
        // Verificar que el usuario exista antes de cualquier operación
        _ = await GetUserByIdAsync(userId, ct)
            ?? throw new KeycloakUserNotFoundException(userId);
 
        var mappingUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}" +
                         $"/users/{userId}/role-mappings/realm";
 
        // 1. Obtener los roles actuales del usuario
        var currentRolesResponse = await SendAuthenticatedAsync(HttpMethod.Get, mappingUrl, ct: ct);
 
        if (!currentRolesResponse.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al obtener los roles actuales del usuario '{userId}'. " +
                $"HTTP {(int)currentRolesResponse.StatusCode}.",
                (int)currentRolesResponse.StatusCode);
 
        var currentRoles = await currentRolesResponse.Content
            .ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];
 
        // 2. Eliminar todos los roles actuales (si los tiene)
        if (currentRoles.Count > 0)
        {
            var deleteResponse = await SendAuthenticatedAsync(
                HttpMethod.Delete, mappingUrl, currentRoles, ct);
 
            if (!deleteResponse.IsSuccessStatusCode)
                throw new KeycloakApiException(
                    $"Error al eliminar los roles actuales del usuario '{userId}'. " +
                    $"HTTP {(int)deleteResponse.StatusCode}.",
                    (int)deleteResponse.StatusCode);
        }
 
        // 3. Asignar el nuevo rol
        await AssignRoleAsync(userId, newRoleName, ct);
    }
 
    // ======================================================
    // EDICIÓN DE USUARIO
    // ======================================================
 
    /// <inheritdoc/>
    public async Task UpdateUserAsync(string userId, KeycloakUpdateUserDto updateDto, CancellationToken ct)
    {
        // Obtener datos actuales para hacer un merge y no pisar campos no enviados
        JsonElement existing = await GetUserByIdAsync(userId, ct)
            ?? throw new KeycloakUserNotFoundException(userId);
 
        // Construir el payload de actualización combinando datos existentes con los nuevos
        var updatedPayload = BuildUpdatePayload(existing, updateDto);
 
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Put, url, updatedPayload, ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al actualizar el usuario '{userId}'. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        // Actualizar el rol si se especificó uno nuevo
        if (!string.IsNullOrWhiteSpace(updateDto.NewRoleName))
            await UpdateUserRoleAsync(userId, updateDto.NewRoleName, ct);
    }
 
    // ======================================================
    // LISTA PAGINADA DE USUARIOS
    // ======================================================
 
    /// <inheritdoc/>
    public async Task<List<KeycloakUserDto>> GetUsersFilteredAsync(
        int first,
        int max,
        string? usernameFilter,
        CancellationToken ct)
    {
        var query = new List<string>
        {
            $"first={first}",
            $"max={max}"
        };
 
        if (!string.IsNullOrWhiteSpace(usernameFilter))
            query.Add($"search={Uri.EscapeDataString(usernameFilter)}");
 
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?{string.Join("&", query)}";
 
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
 
        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al obtener la lista paginada de usuarios. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);
 
        var rawUsers = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];
 
        return rawUsers
            .Select(KeycloakUserMapper.ToDto)
            .Where(dto => dto is not null)
            .ToList()!;
    }
 
    // ======================================================
    // HELPERS PRIVADOS
    // ======================================================
 
    /// <summary>
    /// Obtiene la representación JSON de un rol de realm por su nombre.
    /// </summary>
    /// <param name="roleName">Nombre exacto del rol a buscar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El <see cref="JsonElement"/> con los datos del rol (id, name, etc.).</returns>
    /// <exception cref="KeycloakRoleNotFoundException">Si el rol no existe en el realm.</exception>
    /// <exception cref="KeycloakApiException">Si la petición falla con un error HTTP no esperado.</exception>
    private async Task<JsonElement> FetchRealmRoleAsync(string roleName, CancellationToken ct)
    {
        var roleUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/roles/{roleName}";
 
        var roleResponse = await SendAuthenticatedAsync(HttpMethod.Get, roleUrl, ct: ct);
 
        if (roleResponse.StatusCode == HttpStatusCode.NotFound)
            throw new KeycloakRoleNotFoundException(roleName);
 
        if (!roleResponse.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al consultar el rol '{roleName}' en Keycloak. HTTP {(int)roleResponse.StatusCode}.",
                (int)roleResponse.StatusCode);
 
        return await roleResponse.Content.ReadFromJsonAsync<JsonElement>(ct);
    }
 
    /// <summary>
    /// Construye el payload de actualización de usuario mezclando los datos existentes
    /// con los nuevos valores del DTO, respetando los campos no modificados.
    /// </summary>
    /// <remarks>
    /// Solo los campos con valor no nulo en <paramref name="dto"/> sobrescriben los valores actuales.
    /// Los atributos personalizados (displayName, practitionerId) se gestionan bajo la clave
    /// <c>attributes</c> del payload.
    /// </remarks>
    /// <param name="existing">Datos actuales del usuario obtenidos de Keycloak.</param>
    /// <param name="dto">DTO con los nuevos valores a aplicar.</param>
    /// <returns>Objeto anónimo listo para serializar y enviar a la Admin REST API.</returns>
    private static object BuildUpdatePayload(JsonElement existing, KeycloakUpdateUserDto dto)
    {
        // Leer valores actuales como fallback para campos no modificados
        var currentFirstName = existing.TryGetProperty("firstName", out var fn)
            ? fn.GetString() : null;
        var currentLastName = existing.TryGetProperty("lastName", out var ln)
            ? ln.GetString() : null;
        var currentEmail = existing.TryGetProperty("email", out var em)
            ? em.GetString() : null;
 
        // Leer atributos actuales
        string? currentDisplayName    = null;
        string? currentPractitionerId = null;
 
        if (existing.TryGetProperty("attributes", out var attrs))
        {
            if (attrs.TryGetProperty("displayName", out var dn) && dn.GetArrayLength() > 0)
                currentDisplayName = dn[0].GetString();
 
            if (attrs.TryGetProperty("practitionerId", out var pid) && pid.GetArrayLength() > 0)
                currentPractitionerId = pid[0].GetString();
        }
 
        // Aplicar nuevos valores (si no se proveen, se conservan los actuales)
        var finalFirstName      = dto.FirstName      ?? currentFirstName;
        var finalLastName       = dto.LastName       ?? currentLastName;
        var finalEmail          = dto.Email          ?? currentEmail;
        var finalDisplayName    = dto.DisplayName    ?? currentDisplayName;
        var finalPractitionerId = dto.PractitionerId ?? currentPractitionerId;
 
        return new
        {
            firstName = finalFirstName,
            lastName  = finalLastName,
            email     = finalEmail,
            attributes = new Dictionary<string, string[]>
            {
                ["displayName"]    = [finalDisplayName    ?? ""],
                ["practitionerId"] = [finalPractitionerId ?? ""],
            }
        };
    }
}