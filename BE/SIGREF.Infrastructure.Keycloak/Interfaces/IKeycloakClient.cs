using System.Text.Json;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;

namespace SIGREF.Infrastructure.Keycloak.Interfaces;
 
/// <summary>
/// Contrato principal para interactuar con la Admin REST API de Keycloak.
/// </summary>
/// <remarks>
/// Expone operaciones de autenticación, búsqueda, creación, edición y gestión
/// del ciclo de vida de usuarios dentro de un realm específico.
/// Todas las operaciones son asíncronas y respetan el <see cref="CancellationToken"/> recibido.
/// </remarks>
public interface IKeycloakClient
{
    // =========================================================
    // TOKENS
    // =========================================================
 
    /// <summary>
    /// Obtiene (o renueva desde caché) un token de administrador válido.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El JWT de acceso como <see cref="string"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si Keycloak no devuelve el campo <c>access_token</c> en la respuesta.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Si la petición al endpoint de token falla con un código HTTP no exitoso.
    /// </exception>
    Task<string> GetAdminTokenAsync(CancellationToken ct);
 
    // =========================================================
    // BÚSQUEDAS
    // =========================================================
 
    /// <summary>
    /// Búsqueda optimizada por texto libre usando el índice interno de Keycloak.
    /// </summary>
    /// <remarks>
    /// Keycloak aplica la búsqueda sobre los campos <c>username</c>, <c>email</c>,
    /// <c>firstName</c> y <c>lastName</c> simultáneamente.
    /// </remarks>
    /// <param name="search">Término de búsqueda libre.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de usuarios como <see cref="JsonElement"/> sin mapear.</returns>
    Task<List<JsonElement>> SearchUsersAsync(string search, CancellationToken ct);
 
    /// <summary>
    /// Busca hasta 20 usernames que coincidan con el término indicado.
    /// </summary>
    /// <param name="username">Username completo o parcial a buscar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de usernames encontrados como cadenas.</returns>
    Task<List<string>> SearchUsernamesAsync(string username, CancellationToken ct);
 
    /// <summary>
    /// Busca un usuario por su dirección de email de forma exacta.
    /// </summary>
    /// <param name="email">Email exacto del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El primer <see cref="JsonElement"/> que coincida, o <c>null</c> si no existe.
    /// </returns>
    Task<JsonElement?> SearchUserByEmailAsync(string email, CancellationToken ct);
 
    // =========================================================
    // CRUD
    // =========================================================
 
    /// <summary>
    /// Crea un nuevo usuario en el realm configurado.
    /// </summary>
    /// <param name="kcUser">
    /// Objeto anónimo o tipado compatible con el cuerpo de la Admin REST API de Keycloak
    /// para creación de usuarios.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El ID (UUID) del usuario creado extraído del header <c>Location</c>,
    /// o <c>null</c> si la creación falló.
    /// </returns>
    Task<string?> CreateUserAsync(object kcUser, CancellationToken ct);
 
    /// <summary>
    /// Asigna un rol de realm a un usuario existente.
    /// </summary>
    /// <param name="userId">UUID del usuario en Keycloak.</param>
    /// <param name="roleName">Nombre exacto del rol de realm a asignar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si la asignación fue exitosa; <c>false</c> en caso contrario.</returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si el rol especificado no existe en el realm.
    /// </exception>
    Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct);
 
    /// <summary>
    /// Obtiene los datos completos de un usuario por su ID.
    /// </summary>
    /// <param name="userId">UUID del usuario en Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El <see cref="JsonElement"/> con todos los campos del usuario,
    /// o <c>null</c> si no existe (HTTP 404).
    /// </returns>
    Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct);
 
    // =========================================================
    // TOGGLE DE ESTADO
    // =========================================================
 
    /// <summary>
    /// Alterna el estado activo/inactivo de un usuario.
    /// </summary>
    /// <remarks>
    /// Si el usuario está habilitado (<c>enabled: true</c>), lo deshabilita, y viceversa.
    /// La operación es idempotente respecto al estado actual: siempre invierte el valor.
    /// </remarks>
    /// <param name="userId">UUID del usuario cuyo estado se va a alternar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El nuevo estado del usuario: <c>true</c> si quedó habilitado, <c>false</c> si fue deshabilitado.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakUserNotFoundException">
    /// Si el usuario con el <paramref name="userId"/> indicado no existe en Keycloak.
    /// </exception>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición PATCH a Keycloak falla con un código HTTP no exitoso.
    /// </exception>
    Task<bool> ToggleUserStatusAsync(string userId, CancellationToken ct);
 
    // =========================================================
    // OBTENER MÚLTIPLES USUARIOS POR IDS
    // =========================================================
 
    /// <summary>
    /// Obtiene en paralelo una lista de usuarios a partir de sus IDs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Utiliza la sintaxis de búsqueda nativa <c>id:uuid1 uuid2 uuid3</c> introducida en
    /// Keycloak <b>26.3.0</b>, resolviendo todos los IDs en <b>una sola petición HTTP</b>.
    /// </para>
    /// <para>
    /// Los IDs inexistentes son ignorados silenciosamente por Keycloak;
    /// el resultado solo contiene los usuarios encontrados.
    /// </para>
    /// </remarks>
    /// <param name="userIds">Colección de UUIDs de usuarios a consultar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de <see cref="KeycloakUserDto"/> para los IDs encontrados.</returns>
    Task<List<KeycloakUserDto>> GetUsersByIdsAsync(IEnumerable<string> userIds, CancellationToken ct);
 
    // =========================================================
    // EDICIÓN DE USUARIO
    // =========================================================
 
    /// <summary>
    /// Actualiza los datos principales de un usuario (sin contraseña ni username).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Solo modifica los campos provistos en <paramref name="updateDto"/>. El username
    /// y la contraseña no son editables mediante este método por diseño.
    /// </para>
    /// <para>
    /// Si <paramref name="updateDto"/> incluye un nuevo rol y este difiere del actual,
    /// se elimina el rol anterior y se asigna el nuevo de forma atómica.
    /// </para>
    /// </remarks>
    /// <param name="userId">UUID del usuario a actualizar.</param>
    /// <param name="updateDto">DTO con los campos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="Exceptions.KeycloakUserNotFoundException">
    /// Si el usuario con el <paramref name="userId"/> indicado no existe.
    /// </exception>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición PUT a Keycloak falla con un código HTTP no exitoso.
    /// </exception>
    Task UpdateUserAsync(string userId, KeycloakUpdateUserDto updateDto, CancellationToken ct);
 
    /// <summary>
    /// Reemplaza el rol de realm de un usuario: elimina los roles actuales y asigna el nuevo.
    /// </summary>
    /// <param name="userId">UUID del usuario al que se le cambiará el rol.</param>
    /// <param name="newRoleName">Nombre exacto del rol de realm a asignar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="Exceptions.KeycloakUserNotFoundException">
    /// Si el usuario con el <paramref name="userId"/> indicado no existe.
    /// </exception>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si el rol especificado no existe o si alguna operación HTTP falla.
    /// </exception>
    Task UpdateUserRoleAsync(string userId, string newRoleName, CancellationToken ct);
 
    // =========================================================
    // LISTAR USUARIOS CON PAGINACIÓN
    // =========================================================
 
    /// <summary>
    /// Obtiene una página de usuarios con soporte de paginación y filtro opcional por username.
    /// </summary>
    /// <param name="first">Índice de inicio (0-based) para la paginación.</param>
    /// <param name="max">Cantidad máxima de usuarios a retornar.</param>
    /// <param name="usernameFilter">
    /// Filtro parcial sobre el username. Si es <c>null</c> o vacío, no se aplica filtro.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista paginada de <see cref="KeycloakUserDto"/>.</returns>
    Task<List<KeycloakUserDto>> GetUsersFilteredAsync(
        int first,
        int max,
        string? usernameFilter,
        CancellationToken ct = default);
}