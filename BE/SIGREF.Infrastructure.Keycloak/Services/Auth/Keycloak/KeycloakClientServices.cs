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
/// <para>
/// Este archivo es la parte de servicios de la <c>partial class</c>
/// <see cref="KeycloakClient"/>. Contiene toda la lógica de búsqueda, creación,
/// edición, eliminación y gestión del ciclo de vida de los usuarios en el realm
/// configurado.
/// </para>
/// <para>
/// La autenticación se delega a <c>SendAuthenticatedAsync</c>, definido en la
/// parte principal de la clase parcial, que se encarga de adjuntar el token de
/// administrador a cada petición saliente.
/// </para>
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

        // HTTP 404 es el único caso silencioso: el usuario simplemente no existe.
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al obtener usuario con ID '{userId}'. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);

        return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    /// <summary>
    /// Consulta los nombres de los roles asignados al usuario en el Realm actual.
    /// </summary>
    /// <summary>
    /// Obtiene los nombres de los roles asignados al usuario, filtrando los roles internos de Keycloak.
    /// </summary>
    /// <param name="userId">ID del usuario en Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de nombres de roles (ej: "admin", "ti").</returns>
    public async Task<List<string>> GetUserRolesAsync(string userId, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}/role-mappings/realm";

        // Nota: El tercer parámetro 'null' es crucial para evitar que el 'ct' se serialice como body.
        var response = await SendAuthenticatedAsync(HttpMethod.Get, url, null, ct);

        if (!response.IsSuccessStatusCode)
        {
            //TODO : Exepcion o manejo de error profesional
            return new List<string>();
        }

        var rolesJson = await response.Content.ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];

        return rolesJson
            .Select(r => r.GetProperty("name").GetString()!)
            .Where(roleName => 
                // Filtramos el rol automático de Keycloak (default-roles-sigref)
                !roleName.StartsWith("default-roles-", StringComparison.OrdinalIgnoreCase) && 
                // Filtramos otros roles técnicos estándar de Keycloak
                roleName != "offline_access" && 
                roleName != "uma_authorization")
            .ToList();
    }
    // ======================================================
    // OBTENER MÚLTIPLES USUARIOS POR IDS
    // ======================================================

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Utiliza el endpoint de búsqueda de Keycloak con la sintaxis
    /// <c>id:uuid1 uuid2 uuid3</c>, disponible desde la versión <b>26.3.0</b>.
    /// Esto resuelve todos los IDs en <b>una única petición HTTP</b>,
    /// a diferencia del enfoque de N peticiones paralelas.
    /// </para>
    /// <para>
    /// Los IDs que no existan en Keycloak son ignorados silenciosamente por el
    /// propio servidor; el resultado solo contiene los usuarios encontrados.
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
        // El prefijo "id:" indica a Keycloak que se trata de una búsqueda por IDs exactos.
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
            .Select( x => KeycloakUserMapper.ToDto(x ,null))
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

        // Keycloak responde HTTP 201 y el header Location apunta a la URL del nuevo usuario.
        // El UUID se extrae del último segmento: .../admin/realms/{realm}/users/{uuid}
        return response.Headers.Location?.ToString().Split('/').Last();
    }

    // ======================================================
    // ELIMINAR USUARIO
    // ======================================================

    /// <inheritdoc/>
    public async Task<bool> DeleteUserAsync(string userId, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";

        var response = await SendAuthenticatedAsync(HttpMethod.Delete, url, ct: ct);

        // HTTP 404 se trata como éxito idempotente: si el usuario ya no existe
        // el objetivo de la eliminación se considera cumplido.
        if (response.StatusCode == HttpStatusCode.NotFound)
            return true;

        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al eliminar el usuario '{userId}' en Keycloak. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);

        return true;
    }

    // ======================================================
    // TOGGLE DE ESTADO (ACTIVAR / DESACTIVAR)
    // ======================================================

    /// <summary>
    /// Alterna el estado de habilitación (Enabled/Disabled) de un usuario en Keycloak.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Utiliza un patrón de recuperación y mezcla (Get-Before-Put) para garantizar que los atributos 
    /// personalizados y datos de perfil no se pierdan, dado que Keycloak realiza actualizaciones destructivas vía PUT.
    /// </para>
    /// <para>
    /// Al finalizar la operación, el atributo <c>lastModifiedAt</c> del usuario se actualizará automáticamente.
    /// </para>
    /// </remarks>
    /// <param name="userId">Identificador único (UUID) del usuario objetivo.</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    /// <returns>El nuevo estado booleano asignado al usuario (True: Activo, False: Inactivo).</returns>
    /// <exception cref="KeycloakUserNotFoundException">Lanzada si el usuario especificado no existe en el Realm.</exception>
    /// <exception cref="KeycloakApiException">Lanzada si la API de Keycloak responde con un error de red o de permisos.</exception>
    public async Task<bool> ToggleUserStatusAsync(string userId, CancellationToken ct)
    {
        // 1. Obtener estado actual
        JsonElement existing = await GetUserByIdAsync(userId, ct)
                               ?? throw new KeycloakUserNotFoundException(userId);

        bool currentEnabled = existing.TryGetProperty("enabled", out var en) && en.GetBoolean();
        bool newEnabled = !currentEnabled;

        // 2. Reutilizar la lógica maestra de BuildUpdatePayload
        // Solo pasamos el nuevo estado, el método se encargará de mantener el resto intacto.
        var updateDto = new KeycloakUpdateUserDto { Enabled = newEnabled };
        var updatedPayload = BuildUpdatePayload(existing, updateDto);

        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";

        // 3. Persistencia
        var response = await SendAuthenticatedAsync(HttpMethod.Put, url, updatedPayload, ct);

        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al cambiar estado del usuario '{userId}'. HTTP {(int)response.StatusCode}.",
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
        // Verificar que el usuario exista antes de cualquier operación.
        _ = await GetUserByIdAsync(userId, ct)
            ?? throw new KeycloakUserNotFoundException(userId);

        var mappingUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}" +
                         $"/users/{userId}/role-mappings/realm";

        // 1. Obtener los roles actuales del usuario.
        var currentRolesResponse = await SendAuthenticatedAsync(HttpMethod.Get, mappingUrl, ct: ct);

        if (!currentRolesResponse.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al obtener los roles actuales del usuario '{userId}'. " +
                $"HTTP {(int)currentRolesResponse.StatusCode}.",
                (int)currentRolesResponse.StatusCode);

        var currentRoles = await currentRolesResponse.Content
            .ReadFromJsonAsync<List<JsonElement>>(ct) ?? [];

        // 2. Eliminar todos los roles actuales (si los tiene).
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

        // 3. Asignar el nuevo rol.
        await AssignRoleAsync(userId, newRoleName, ct);
    }

    // ======================================================
    // EDICIÓN DE USUARIO
    // ======================================================

    /// <summary>
    /// Actualiza de forma integral un usuario existente en Keycloak.
    /// </summary>
    /// <remarks>
    /// Este método implementa un patrón de "Merge": recupera el estado actual del usuario desde Keycloak, 
    /// combina los cambios proporcionados en el <paramref name="updateDto"/> y preserva los valores preexistentes 
    /// en los campos no especificados. Además, gestiona la actualización de roles si se detecta un cambio.
    /// </remarks>
    /// <param name="userId">Identificador único (UUID) del usuario en Keycloak.</param>
    /// <param name="updateDto">Objeto que contiene los campos opcionales a actualizar.</param>
    /// <param name="ct">Token de cancelación para abortar la operación asíncrona.</param>
    /// <exception cref="KeycloakUserNotFoundException">Se lanza si el <paramref name="userId"/> no existe en el Realm.</exception>
    /// <exception cref="KeycloakApiException">Se lanza si la Admin REST API devuelve un código de error (distinto de 2xx).</exception>
    public async Task UpdateUserAsync(string userId, KeycloakUpdateUserDto updateDto, CancellationToken ct)
    {
        // Es imperativo obtener el usuario actual para evitar la pérdida de datos (Data Loss)
        // debido a que el endpoint PUT de Keycloak sobrescribe el objeto completo.
        JsonElement existing = await GetUserByIdAsync(userId, ct)
                               ?? throw new KeycloakUserNotFoundException(userId);
        // Se delega la construcción del JSON final a una función pura que combina
        // los datos actuales con los nuevos, incluyendo la marca de tiempo de auditoría.
        var updatedPayload = BuildUpdatePayload(existing, updateDto);

        var url = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/users/{userId}";

        var response = await SendAuthenticatedAsync(HttpMethod.Put, url, updatedPayload, ct);

        if (!response.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Error al actualizar el usuario '{userId}'. HTTP {(int)response.StatusCode}.",
                (int)response.StatusCode);

        // El cambio de rol es una operación atómica separada en la API de Keycloak (mappings).
        // Solo se ejecuta si se ha definido explícitamente un nuevo rol en el DTO.
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
            .Select(x => KeycloakUserMapper.ToDto(x , null))
            .Where(dto => dto is not null)
            .ToList()!;
    }

    // ======================================================
    // HELPERS PRIVADOS
    // ======================================================

    /// <summary>
    /// Obtiene la representación JSON completa de un rol de realm por su nombre.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Este método es utilizado internamente por <see cref="AssignRoleAsync"/> y
    /// puede ser invocado desde <see cref="KeycloakAdminService"/> para realizar
    /// validaciones previas a la creación de usuarios, asegurando que los roles
    /// existen y son accesibles antes de iniciar escrituras en Keycloak.
    /// </para>
    /// <para>
    /// El acceso al endpoint de roles requiere que el Service Account tenga
    /// el rol <c>view-realm</c> asignado en <c>realm-management</c>.
    /// </para>
    /// </remarks>
    /// <param name="roleName">Nombre exacto del rol a buscar en el realm.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El <see cref="JsonElement"/> con los datos del rol (<c>id</c>, <c>name</c>,
    /// <c>composite</c>, etc.), listo para ser incluido en el body de asignación.
    /// </returns>
    /// <exception cref="KeycloakRoleNotFoundException">
    /// Si el rol no existe en el realm (HTTP 404).
    /// </exception>
    /// <exception cref="KeycloakAccessForbiddenException">
    /// Si el Service Account no tiene permisos suficientes en el realm para
    /// consultar roles (HTTP 403). Generalmente indica que falta el rol
    /// <c>view-realm</c> en <c>realm-management</c>.
    /// </exception>
    /// <exception cref="KeycloakApiException">
    /// Si la petición falla con cualquier otro código HTTP no exitoso.
    /// </exception>
    internal async Task<JsonElement> FetchRealmRoleAsync(string roleName, CancellationToken ct)
    {
        var roleUrl = $"{_settings.BaseUrl}/admin/realms/{_settings.RealmName}/roles/{roleName}";

        var roleResponse = await SendAuthenticatedAsync(HttpMethod.Get, roleUrl, ct: ct);

        // Caso 1: El rol no existe en el realm.
        if (roleResponse.StatusCode == HttpStatusCode.NotFound)
            throw new KeycloakRoleNotFoundException(roleName);

        // Caso 2: El Service Account carece de permisos para ver roles.
        if (roleResponse.StatusCode == HttpStatusCode.Forbidden)
            throw new KeycloakAccessForbiddenException(
                roleName,
                "El Service Account carece del rol 'view-realm' en realm-management.");

        // Caso 3: Cualquier otro error HTTP no esperado.
        if (!roleResponse.IsSuccessStatusCode)
            throw new KeycloakApiException(
                $"Fallo inesperado al consultar el rol '{roleName}' en Keycloak. " +
                $"HTTP {(int)roleResponse.StatusCode}.",
                (int)roleResponse.StatusCode);

        return await roleResponse.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    /// <summary>
    /// Construye el payload de actualización de usuario mezclando los datos existentes
    /// con los nuevos valores del DTO, conservando los campos no modificados.
    /// </summary>
    /// <remarks>
    /// <para>
    /// El merge es field-by-field: si el campo en <paramref name="dto"/> tiene valor
    /// no nulo, se usa ese valor; de lo contrario se conserva el valor actual leído
    /// de <paramref name="existing"/>.
    /// </para>
    /// <para>
    /// Los atributos personalizados (<c>displayName</c> y <c>practitionerId</c>)
    /// se gestionan bajo la clave <c>attributes</c> del payload, que Keycloak espera
    /// como un diccionario de <c>string → string[]</c>.
    /// </para>
    /// <para>
    /// El campo <c>username</c> no se incluye en el payload para evitar modificaciones
    /// accidentales de las credenciales de acceso.
    /// </para>
    /// </remarks>
    /// <param name="existing">
    /// Datos actuales del usuario obtenidos de Keycloak. Se usan como valores de
    /// respaldo para los campos no especificados en <paramref name="dto"/>.
    /// </param>
    /// <param name="dto">DTO con los nuevos valores a aplicar. Los campos nulos se ignoran.</param>
    /// <returns>
    /// Objeto anónimo listo para serializar y enviar al endpoint PUT de la Admin REST API.
    /// </returns>
    private static object BuildUpdatePayload(JsonElement existing, KeycloakUpdateUserDto dto)
    {
        // Leer valores actuales como fallback para campos no presentes en el DTO.
        var currentFirstName = existing.TryGetProperty("firstName", out var fn) ? fn.GetString() : null;
        var currentLastName = existing.TryGetProperty("lastName", out var ln) ? ln.GetString() : null;
        var currentEmail = existing.TryGetProperty("email", out var em) ? em.GetString() : null;
        var currentEnabled = existing.TryGetProperty("enabled", out var en) && en.GetBoolean();
        // Leer atributos personalizados actuales.
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
            enabled = dto.Enabled ?? currentEnabled, // Si el DTO no lo trae, preservamos el actual
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