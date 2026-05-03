using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SIGREF.Common.Exceptions;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

public partial class KeycloakClient : IKeycloakClient
{
    // ======================================================
    // BÚSQUEDAS
    // ======================================================

    public async Task<List<JsonElement>> SearchUsersAsync(string search, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?search={Uri.EscapeDataString(search)}";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_SEARCH_USERS_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "searchTerm", search },
                    { "httpStatus", (int)response.StatusCode }
                });

        return await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];
    }

    /// <summary>
    /// Obtiene el número total de usuarios que coinciden con los filtros indicados.
    /// Usa el endpoint nativo /users/count de la Admin REST API.
    /// </summary>
    public async Task<int> GetUsersCountAsync(
        string? usernameFilter,
        CancellationToken ct)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(usernameFilter))
            query.Add($"search={Uri.EscapeDataString(usernameFilter)}");

        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/count" +
                  (query.Count > 0 ? $"?{string.Join("&", query)}" : string.Empty);

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_GET_USERS_COUNT_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "filter", usernameFilter ?? "(sin filtro)" },
                    { "httpStatus", (int)response.StatusCode }
                });

        return await response.Content.ReadFromJsonAsync<int>(ct);
    }

    public async Task<List<string>> SearchUsernamesAsync(string username, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?search={Uri.EscapeDataString(username)}&first=0&max=20";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_SEARCH_USERNAMES_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "searchTerm", username },
                    { "httpStatus", (int)response.StatusCode }
                });

        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];

        return list
            .Select(u => u.TryGetProperty("username", out var un) ? un.GetString() : null)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u!)
            .ToList();
    }

    public async Task<JsonElement?> SearchUserByEmailAsync(string email, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?email={Uri.EscapeDataString(email)}&exact=true";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_SEARCH_USER_BY_EMAIL_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "email", email },
                    { "httpStatus", (int)response.StatusCode }
                });

        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct);
        return list?.FirstOrDefault();
    }

    // ======================================================
    // OBTENER POR ID
    // ======================================================

    public async Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        // HTTP 404 es el único caso silencioso: el caller decide si lanzar NotFoundException.
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_GET_USER_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "httpStatus", (int)response.StatusCode }
                });

        return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    public async Task<List<string>> GetUserRolesAsync(string userId, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}/role-mappings/realm";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, null, ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_GET_USER_ROLES_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "httpStatus", (int)response.StatusCode }
                });

        var rolesJson = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];

        return rolesJson
            .Select(r => r.GetProperty("name").GetString()!)
            .Where(roleName =>
                !roleName.StartsWith("default-roles-", StringComparison.OrdinalIgnoreCase) &&
                roleName != "offline_access" &&
                roleName != "uma_authorization")
            .ToList();
    }

    // ======================================================
    // OBTENER MÚLTIPLES USUARIOS POR IDS
    // ======================================================

    public async Task<List<KeycloakUserDto>> GetUsersByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken ct)
    {
        var idList = userIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();

        if (idList.Count == 0)
            return [];

        var searchQuery = "id:" + string.Join(" ", idList);

        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?search={Uri.EscapeDataString(searchQuery)}&max={idList.Count}";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_GET_USERS_BY_IDS_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "requestedCount", idList.Count },
                    { "httpStatus", (int)response.StatusCode }
                });

        var rawUsers = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];

        return rawUsers
            .Select(x => KeycloakUserMapper.ToDto(x, null))
            .Where(dto => dto is not null)
            .ToList()!;
    }

    // ======================================================
    // CREAR USUARIO
    // ======================================================

    public async Task<string?> CreateUserAsync(object kcUser, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users";

        var response = await SendAuthenticatedAsync(HttpMethod.Post, url, kcUser, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new ConflictException(
                "KEYCLOAK_USER_ALREADY_EXISTS",
                extraData: new Dictionary<string, object>
                {
                    { "httpStatus", (int)response.StatusCode }
                });

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_CREATE_USER_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "httpStatus", (int)response.StatusCode }
                });

        return response.Headers.Location?.ToString().Split('/').Last();
    }

    // ======================================================
    // ELIMINAR USUARIO
    // ======================================================

    public async Task<bool> DeleteUserAsync(string userId, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";

        var response = await SendAuthenticatedAsync(HttpMethod.Delete, url, ct: ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return true;

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_DELETE_USER_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "httpStatus", (int)response.StatusCode }
                });

        return true;
    }

    // ======================================================
    // TOGGLE DE ESTADO (ACTIVAR / DESACTIVAR)
    // ======================================================

    public async Task<bool> ToggleUserStatusAsync(string userId, CancellationToken ct)
    {
        JsonElement existing = await GetUserByIdAsync(userId, ct)
                               ?? throw new NotFoundException(
                                   "KEYCLOAK_USER_NOT_FOUND",
                                   extraData: new Dictionary<string, object> { { "userId", userId } });

        bool currentEnabled = existing.TryGetProperty("enabled", out var en) && en.GetBoolean();
        bool newEnabled = !currentEnabled;

        var updateDto = new KeycloakUpdateUserDto { Enabled = newEnabled };
        var updatedPayload = BuildUpdatePayload(existing, updateDto);

        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";

        var response = await SendAuthenticatedAsync(HttpMethod.Put, url, updatedPayload, ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_TOGGLE_USER_STATUS_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "newStatus", newEnabled },
                    { "httpStatus", (int)response.StatusCode }
                });

        return newEnabled;
    }

    // ======================================================
    // ROLES
    // ======================================================

    public async Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var role = await FetchRealmRoleAsync(roleName, ct);

        var mappingUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}" +
                         $"/users/{userId}/role-mappings/realm";

        var response = await SendAuthenticatedAsync(HttpMethod.Post, mappingUrl, new[] { role }, ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_ASSIGN_ROLE_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "roleName", roleName },
                    { "httpStatus", (int)response.StatusCode }
                });

        return true;
    }

    public async Task UpdateUserRoleAsync(string userId, string newRoleName, CancellationToken ct)
    {
        _ = await GetUserByIdAsync(userId, ct)
            ?? throw new NotFoundException(
                "KEYCLOAK_USER_NOT_FOUND",
                extraData: new Dictionary<string, object> { { "userId", userId } });

        var mappingUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}" +
                         $"/users/{userId}/role-mappings/realm";

        var currentRolesResponse = await SendAuthenticatedAsync(HttpMethod.Get, mappingUrl, ct: ct);

        if (!currentRolesResponse.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_GET_USER_ROLES_FAILED",
                statusCode: (int)currentRolesResponse.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "httpStatus", (int)currentRolesResponse.StatusCode }
                });

        var currentRoles = await currentRolesResponse.Content
            .ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];

        if (currentRoles.Count > 0)
        {
            var deleteResponse = await SendAuthenticatedAsync(
                HttpMethod.Delete, mappingUrl, currentRoles, ct);

            if (!deleteResponse.IsSuccessStatusCode)
                throw new ExternalServiceException(
                    "KEYCLOAK_REMOVE_USER_ROLES_FAILED",
                    statusCode: (int)deleteResponse.StatusCode,
                    extraData: new Dictionary<string, object>
                    {
                        { "userId", userId },
                        { "httpStatus", (int)deleteResponse.StatusCode }
                    });
        }

        await AssignRoleAsync(userId, newRoleName, ct);
    }

    // ======================================================
    // EDICIÓN DE USUARIO
    // ======================================================

    public async Task UpdateUserAsync(string userId, KeycloakUpdateUserDto updateDto, CancellationToken ct)
    {
        JsonElement existing = await GetUserByIdAsync(userId, ct)
                               ?? throw new NotFoundException(
                                   "KEYCLOAK_USER_NOT_FOUND",
                                   extraData: new Dictionary<string, object> { { "userId", userId } });

        var updatedPayload = BuildUpdatePayload(existing, updateDto);

        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";

        var response = await SendAuthenticatedAsync(HttpMethod.Put, url, updatedPayload, ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_UPDATE_USER_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "httpStatus", (int)response.StatusCode }
                });

        if (!string.IsNullOrWhiteSpace(updateDto.NewRoleName))
            await UpdateUserRoleAsync(userId, updateDto.NewRoleName, ct);
    }

    // ======================================================
    // LISTA PAGINADA DE USUARIOS
    // ======================================================

    public async Task<List<KeycloakUserDto>> GetUsersFilteredAsync(
        int first,
        int max,
        string? search,
        string? username,
        string? email,
        string? firstName,
        string? lastName,
        CancellationToken ct)
    {
        var query = new List<string>
        {
            $"first={first}",
            $"max={max}"
        };

        // search es amplio (aplica sobre todos los campos de texto en Keycloak)
        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");

        // Filtros específicos: solo se aplican si search está vacío para evitar conflictos
        if (string.IsNullOrWhiteSpace(search))
        {
            if (!string.IsNullOrWhiteSpace(username))
                query.Add($"username={Uri.EscapeDataString(username)}");

            if (!string.IsNullOrWhiteSpace(email))
                query.Add($"email={Uri.EscapeDataString(email)}");

            if (!string.IsNullOrWhiteSpace(firstName))
                query.Add($"firstName={Uri.EscapeDataString(firstName)}");

            if (!string.IsNullOrWhiteSpace(lastName))
                query.Add($"lastName={Uri.EscapeDataString(lastName)}");
        }

        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?{string.Join("&", query)}";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_GET_USERS_FILTERED_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "httpStatus", (int)response.StatusCode }
                });

        var rawUsers = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];

        return rawUsers
            .Select(x => KeycloakUserMapper.ToDto(x, null))
            .Where(dto => dto is not null)
            .ToList()!;
    }

// El método de conteo usa exactamente los mismos parámetros para garantizar consistencia
    public async Task<int> GetUsersCountAsync(
        string? search,
        string? username,
        string? email,
        string? firstName,
        string? lastName,
        CancellationToken ct)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");

        if (string.IsNullOrWhiteSpace(search))
        {
            if (!string.IsNullOrWhiteSpace(username))
                query.Add($"username={Uri.EscapeDataString(username)}");

            if (!string.IsNullOrWhiteSpace(email))
                query.Add($"email={Uri.EscapeDataString(email)}");

            if (!string.IsNullOrWhiteSpace(firstName))
                query.Add($"firstName={Uri.EscapeDataString(firstName)}");

            if (!string.IsNullOrWhiteSpace(lastName))
                query.Add($"lastName={Uri.EscapeDataString(lastName)}");
        }

        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/count" +
                  (query.Count > 0 ? $"?{string.Join("&", query)}" : string.Empty);

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_GET_USERS_COUNT_FAILED",
                statusCode: (int)response.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "httpStatus", (int)response.StatusCode }
                });

        return await response.Content.ReadFromJsonAsync<int>(ct);
    }

    // ======================================================
    // HELPERS PRIVADOS
    // ======================================================

    internal async Task<JsonElement> FetchRealmRoleAsync(string roleName, CancellationToken ct)
    {
        var roleUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/roles/{roleName}";

        var roleResponse = await SendAuthenticatedAsync(HttpMethod.Get, roleUrl, ct: ct);

        if (roleResponse.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException(
                "KEYCLOAK_ROLE_NOT_FOUND",
                extraData: new Dictionary<string, object> { { "roleName", roleName } });
        if (roleResponse.StatusCode == HttpStatusCode.Forbidden)
            throw new ForbiddenException(
                "KEYCLOAK_ROLE_ACCESS_FORBIDDEN",
                extraData: new Dictionary<string, object>
                {
                    { "roleName", roleName },
                    { "hint", "El Service Account requiere el rol 'view-realm' en realm-management." }
                });

        if (!roleResponse.IsSuccessStatusCode)
            throw new ExternalServiceException(
                "KEYCLOAK_FETCH_ROLE_FAILED",
                statusCode: (int)roleResponse.StatusCode,
                extraData: new Dictionary<string, object>
                {
                    { "roleName", roleName },
                    { "httpStatus", (int)roleResponse.StatusCode }
                });

        return await roleResponse.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    private static object BuildUpdatePayload(JsonElement existing, KeycloakUpdateUserDto dto)
    {
        var currentFirstName = existing.TryGetProperty("firstName", out var fn) ? fn.GetString() : null;
        var currentLastName = existing.TryGetProperty("lastName", out var ln) ? ln.GetString() : null;
        var currentEmail = existing.TryGetProperty("email", out var em) ? em.GetString() : null;
        var currentEnabled = existing.TryGetProperty("enabled", out var en) && en.GetBoolean();

        string? currentDisplayName = null;
        string? currentPractitionerId = null;

        if (existing.TryGetProperty("attributes", out var attrs))
        {
            if (attrs.TryGetProperty("displayName", out var dn) && dn.GetArrayLength() > 0)
                currentDisplayName = dn[0].GetString();

            if (attrs.TryGetProperty("practitionerId", out var pid) && pid.GetArrayLength() > 0)
                currentPractitionerId = pid[0].GetString();
        }

        return new
        {
            enabled = dto.Enabled ?? currentEnabled,
            firstName = dto.FirstName ?? currentFirstName,
            lastName = dto.LastName ?? currentLastName,
            email = dto.Email ?? currentEmail,
            attributes = new Dictionary<string, string[]>
            {
                ["displayName"] = [dto.DisplayName ?? currentDisplayName ?? ""],
                ["practitionerId"] = [dto.PractitionerId ?? currentPractitionerId ?? ""],
                ["lastModifiedAt"] = [DateTimeOffset.UtcNow.ToString("O")]
            }
        };
    }
}