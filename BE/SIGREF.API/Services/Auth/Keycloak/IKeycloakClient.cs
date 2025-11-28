using System.Text.Json;

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
    Task<JsonElement?> SearchUserByUsernameAsync(string username, CancellationToken ct);

        /// <summary>
        /// Buscar por email exacto.
        ///</summary>
    Task<JsonElement?> SearchUserByEmailAsync(string email, CancellationToken ct);

    // =========================================================
    // LISTAR USUARIOS CON PAGINACIÓN
    // =========================================================

    /// <summary>
    /// Retorna usuarios paginados desde Keycloak usando first + max.
    /// Súper eficiente, recomendado cuando sí se necesita la lista completa.
    /// </summary>
    Task<List<JsonElement>> GetUsersPaginatedAsync(int first, int max, CancellationToken ct);

    // =========================================================
    // CRUD
    // =========================================================

    Task<string?> CreateUserAsync(object kcUser, CancellationToken ct);

    Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct);

    Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct);
}