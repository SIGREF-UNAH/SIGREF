using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

public partial class KeycloakClient : IKeycloakClient
{
    // ======================================================
    // SEARCH
    // ======================================================

    public async Task<List<JsonElement>> SearchUsersAsync(string search, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?search={Uri.EscapeDataString(search)}";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct)
               ?? [];
    }

    public async Task<List<string>> SearchUsernamesAsync(string username, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users" +
                  $"?search={Uri.EscapeDataString(username)}&first=0&max=20";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
        response.EnsureSuccessStatusCode();

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
        response.EnsureSuccessStatusCode();

        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct);
        return list?.FirstOrDefault();
    }

    // ======================================================
    // GET BY ID 
    // ======================================================

    public async Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);

        // Único caso silencioso: el usuario simplemente no existe
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        // Cualquier otro error HTTP se propaga al caller
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    // ======================================================
    // CREATE
    // ======================================================

    public async Task<string?> CreateUserAsync(object kcUser, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users";

        var response = await SendAuthenticatedAsync(HttpMethod.Post, url, kcUser, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return response.Headers.Location?.ToString().Split('/').Last();
    }

    // ======================================================
    // ROLES
    // ======================================================

    public async Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var roleUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/roles/{roleName}";

        var roleResponse = await SendAuthenticatedAsync(HttpMethod.Get, roleUrl, ct: ct);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<JsonElement>(ct);

        var mappingUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}" +
                         $"/users/{userId}/role-mappings/realm";

        var response = await SendAuthenticatedAsync(HttpMethod.Post, mappingUrl, new[] { role }, ct);

        return response.IsSuccessStatusCode;
    }

    // ======================================================
    // FILTERED LIST
    // ======================================================

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
        response.EnsureSuccessStatusCode();

        var rawUsers = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];

        return rawUsers
            .Select(KeycloakUserMapper.ToDto)
            .Where(dto => dto is not null)
            .ToList()!;
    }
}