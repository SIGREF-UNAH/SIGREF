using System.Net.Http.Json;
using System.Text.Json;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

public partial class KeycloakClient: IKeycloakClient
{
     // ======================================================
    // SEARCH — optimizado
    // ======================================================
    public async Task<List<JsonElement>> SearchUsersAsync(string search, CancellationToken ct)
    {
        var url = $"{_baseUrl}/admin/realms/{_realm}/users?search={Uri.EscapeDataString(search)}";
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct);

        return list ?? new List<JsonElement>();
    }

    public async Task<List<string>> SearchUsernamesAsync(string username, CancellationToken ct)
    {
        // Keycloak buscará en username, email, firstName, lastName… 
        // pero vamos a filtrar SOLO username en el backend.
        var url =
            $"{_baseUrl}/admin/realms/{_realm}/users?" +
            $"search={Uri.EscapeDataString(username)}&first=0&max=20";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct)
                   ?? new List<JsonElement>();

        // Convertir solo a lista de usernames
        var usernames = list
            .Select(u =>
            {
                if (u.TryGetProperty("username", out var un))
                    return un.GetString();
                return null;
            })
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u!)
            .ToList();

        return usernames;
    }



    public async Task<JsonElement?> SearchUserByEmailAsync(string email, CancellationToken ct)
    {
        var url = $"{_baseUrl}/admin/realms/{_realm}/users?email={Uri.EscapeDataString(email)}";
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct);

        return list?.FirstOrDefault();
    }
    

    public async Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct)
    {
        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}";

        try
        {
            var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        }
        catch
        {
            return null;
        }
    }
    

    public async Task<string?> CreateUserAsync(object kcUser, CancellationToken ct)
    {
        var url = $"{_baseUrl}/admin/realms/{_realm}/users";
        var response = await SendAuthenticatedAsync(HttpMethod.Post, url, kcUser, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var location = response.Headers.Location?.ToString();
        return location?.Split('/').Last();
    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var roleUrl = $"{_baseUrl}/admin/realms/{_realm}/roles/{roleName}";
        var roleResponse = await SendAuthenticatedAsync(HttpMethod.Get, roleUrl, ct: ct);
        roleResponse.EnsureSuccessStatusCode();
        var role = await roleResponse.Content.ReadFromJsonAsync<JsonElement>(ct);

        var mappingUrl =
            $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";

        var res = await SendAuthenticatedAsync(HttpMethod.Post, mappingUrl, new[] { role }, ct);

        return res.IsSuccessStatusCode;
    }
    
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

        var url = $"{_baseUrl}/admin/realms/{_realm}/users?{string.Join("&", query)}";

        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, ct: ct);
        response.EnsureSuccessStatusCode();
        var rawUsers = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct)
                       ?? new List<JsonElement>();

        return rawUsers
            .Select(KeycloakUserMapper.ToDto)
            .Where(dto => dto != null)
            .ToList()!;
    }

    
    
}