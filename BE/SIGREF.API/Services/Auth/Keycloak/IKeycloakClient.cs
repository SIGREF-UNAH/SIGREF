using System.Text.Json;
using SIGREF.API.Dtos.Auth;

namespace SIGREF.API.Services.Auth.Keycloak;

public interface IKeycloakClient
{
    // =========================================================
    // TOKENS
    // =========================================================
    Task<string> GetAdminTokenAsync(CancellationToken ct);

    // =========================================================
    // BÚSQUEDAS
    // =========================================================

    /// <summary>
    /// Búsqueda optimizada por texto (usa el índice interno de Keycloak).
    /// </summary>
    Task<List<JsonElement>> SearchUsersAsync(string search, CancellationToken ct);

    /// <summary>
    /// Buscar por username exacto.
    ///</summary>
    Task<List<string>> SearchUsernamesAsync(string username, CancellationToken ct);

    /// <summary>
    /// Buscar por email exacto.
    ///</summary>
    Task<JsonElement?> SearchUserByEmailAsync(string email, CancellationToken ct);


    // =========================================================
    // CRUD
    // =========================================================

    Task<string?> CreateUserAsync(object kcUser, CancellationToken ct);

    Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct);

    Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct);

    // =========================================================
    // LISTAR USUARIOS CON PAGINACIÓN
    // =========================================================

    Task<List<KeycloakUserDto>> GetUsersFilteredAsync(
        int first,
        int max,
        string? usernameFilter,
        CancellationToken ct =  default);
}