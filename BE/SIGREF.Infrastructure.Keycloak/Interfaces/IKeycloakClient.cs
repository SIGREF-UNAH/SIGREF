using System.Text.Json;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;

namespace SIGREF.Infrastructure.Keycloak.Interfaces;
 
/// <summary>
/// Contrato principal para interactuar con la Admin REST API de Keycloak.
/// </summary>
/// <remarks>
/// <para>
/// Expone operaciones de autenticación, búsqueda, creación, edición, eliminación
/// y gestión del ciclo de vida de usuarios dentro de un realm específico.
/// </para>
/// <para>
/// Todas las operaciones son asíncronas y respetan el <see cref="CancellationToken"/>
/// recibido. Los métodos de este contrato no aplican lógica de negocio; esa
/// responsabilidad recae en <see cref="IKeycloakAdminService"/>.
/// </para>
/// <para>
/// Los errores de infraestructura (HTTP no exitoso, usuario/rol inexistente) se
/// propagan como excepciones tipadas del namespace
/// <c>SIGREF.Infrastructure.Keycloak.Exceptions</c>.
/// </para>
/// </remarks>
public interface IKeycloakClient
{
    // =========================================================
    // TOKENS
    // =========================================================
 
    /// <summary>
    /// Obtiene (o renueva desde caché) un token de administrador válido para el realm.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El JWT de acceso (<c>access_token</c>) como <see cref="string"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si Keycloak no devuelve el campo <c>access_token</c> en la respuesta.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Si la petición al endpoint de token falla con un código HTTP no exitoso.
    /// </exception>
    Task<string> GetAdminTokenAsync(CancellationToken ct);


    Task<List<string>> GetUserRolesAsync(string userId, CancellationToken ct);
 
    // =========================================================
    // BÚSQUEDAS
    // =========================================================
 
    /// <summary>
    /// Realiza una búsqueda de texto libre sobre los usuarios del realm usando
    /// el índice interno de Keycloak.
    /// </summary>
    /// <remarks>
    /// Keycloak aplica la búsqueda simultáneamente sobre los campos
    /// <c>username</c>, <c>email</c>, <c>firstName</c> y <c>lastName</c>.
    /// Los resultados no están mapeados a DTOs; el mapping es responsabilidad
    /// del llamador.
    /// </remarks>
    /// <param name="search">Término de búsqueda libre (puede ser parcial).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Lista de usuarios como <see cref="JsonElement"/> sin mapear.
    /// Retorna una lista vacía si no hay coincidencias.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición HTTP a Keycloak falla con un código no exitoso.
    /// </exception>
    Task<List<JsonElement>> SearchUsersAsync(string search, CancellationToken ct);
 
    /// <summary>
    /// Busca hasta 20 usernames que contengan el término indicado.
    /// </summary>
    /// <remarks>
    /// Útil para verificar disponibilidad de username y ofrecer sugerencias
    /// similares al usuario final.
    /// </remarks>
    /// <param name="username">Username completo o parcial a buscar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Lista de usernames encontrados como cadenas de texto.
    /// Retorna una lista vacía si no hay coincidencias.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición HTTP a Keycloak falla con un código no exitoso.
    /// </exception>
    Task<List<string>> SearchUsernamesAsync(string username, CancellationToken ct);
 
    /// <summary>
    /// Busca un usuario por su dirección de correo electrónico de forma exacta.
    /// </summary>
    /// <remarks>
    /// Utiliza el parámetro <c>exact=true</c> de la Admin REST API para garantizar
    /// que solo se retorne el usuario cuyo email coincida exactamente con el valor
    /// proporcionado (sin búsqueda parcial ni case-insensitive).
    /// </remarks>
    /// <param name="email">Email exacto del usuario a buscar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El primer <see cref="JsonElement"/> cuyo email coincida exactamente,
    /// o <see langword="null"/> si no existe ningún usuario con ese email.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición HTTP a Keycloak falla con un código no exitoso.
    /// </exception>
    Task<JsonElement?> SearchUserByEmailAsync(string email, CancellationToken ct);
 
    // =========================================================
    // CRUD
    // =========================================================
 
    /// <summary>
    /// Crea un nuevo usuario en el realm configurado.
    /// </summary>
    /// <remarks>
    /// <para>
    /// El cuerpo de la petición debe ser compatible con el esquema de la
    /// Admin REST API de Keycloak para creación de usuarios. Se recomienda
    /// utilizar un objeto anónimo o un DTO con los campos
    /// <c>username</c>, <c>email</c>, <c>firstName</c>, <c>lastName</c>,
    /// <c>enabled</c>, <c>credentials</c> y <c>attributes</c>.
    /// </para>
    /// <para>
    /// Si la creación es exitosa, Keycloak responde con HTTP 201 y el header
    /// <c>Location</c> apuntando a la URL del nuevo usuario. El ID se extrae
    /// del último segmento de esa URL.
    /// </para>
    /// </remarks>
    /// <param name="kcUser">
    /// Objeto anónimo o tipado compatible con el cuerpo esperado por la
    /// Admin REST API de Keycloak para la creación de usuarios.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El UUID del usuario creado extraído del header <c>Location</c>,
    /// o <see langword="null"/> si la respuesta no incluye dicho header.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si Keycloak responde con un código HTTP no exitoso (p. ej. 409 si el
    /// username o email ya existen).
    /// </exception>
    Task<string?> CreateUserAsync(object kcUser, CancellationToken ct);
 
    /// <summary>
    /// Asigna un rol de realm a un usuario existente.
    /// </summary>
    /// <remarks>
    /// El rol debe existir previamente en el realm. Internamente se consulta
    /// la representación JSON del rol mediante <c>FetchRealmRoleAsync</c>
    /// para obtener su <c>id</c> requerido por la Admin REST API.
    /// </remarks>
    /// <param name="userId">UUID del usuario en Keycloak al que se asignará el rol.</param>
    /// <param name="roleName">Nombre exacto del rol de realm a asignar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see langword="true"/> si la asignación fue exitosa.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakRoleNotFoundException">
    /// Si el rol especificado no existe en el realm.
    /// </exception>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición HTTP de asignación falla con un código no exitoso.
    /// </exception>
    Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct);
 
    /// <summary>
    /// Obtiene la representación completa de un usuario a partir de su ID de Keycloak.
    /// </summary>
    /// <remarks>
    /// HTTP 404 es el único caso tratado silenciosamente: se retorna
    /// <see langword="null"/> en lugar de lanzar una excepción, permitiendo al
    /// llamador decidir cómo manejar la ausencia del usuario.
    /// Cualquier otro código de error sí lanza excepción.
    /// </remarks>
    /// <param name="userId">UUID del usuario en Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El <see cref="JsonElement"/> con todos los campos del usuario,
    /// o <see langword="null"/> si no existe (HTTP 404).
    /// </returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición falla con un código HTTP distinto a 200 o 404.
    /// </exception>
    Task<JsonElement?> GetUserByIdAsync(string userId, CancellationToken ct);
 
    /// <summary>
    /// Elimina permanentemente un usuario del realm de Keycloak.
    /// </summary>
    /// <remarks>
    /// <para>
    /// La eliminación es <b>irreversible</b>. Se recomienda utilizarla únicamente
    /// como operación de rollback (compensación transaccional) tras un fallo en
    /// la creación del usuario, o cuando se requiera una baja definitiva del sistema.
    /// </para>
    /// <para>
    /// Si el usuario no existe (HTTP 404), la operación se considera exitosa y
    /// retorna <see langword="true"/> sin lanzar excepción, garantizando la
    /// idempotencia de la operación.
    /// </para>
    /// </remarks>
    /// <param name="userId">UUID del usuario a eliminar en Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// <see langword="true"/> si el usuario fue eliminado o ya no existía;
    /// <see langword="false"/> si la petición no pudo completarse correctamente.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si Keycloak responde con un código HTTP no exitoso distinto a 404.
    /// </exception>
    Task<bool> DeleteUserAsync(string userId, CancellationToken ct);
 
    // =========================================================
    // TOGGLE DE ESTADO
    // =========================================================
 
    /// <summary>
    /// Alterna el estado activo/inactivo (<c>enabled</c>) de un usuario en el realm.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Si el usuario está habilitado (<c>enabled: true</c>), lo deshabilita, y viceversa.
    /// La operación siempre invierte el estado actual; no es posible forzar un estado
    /// específico mediante este método.
    /// </para>
    /// <para>
    /// Un usuario deshabilitado no puede autenticarse en el realm, pero sus datos
    /// y sesiones activas se conservan hasta que expire el token o se invalide manualmente.
    /// </para>
    /// </remarks>
    /// <param name="userId">UUID del usuario cuyo estado se va a alternar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El nuevo estado del usuario: <see langword="true"/> si quedó habilitado,
    /// <see langword="false"/> si fue deshabilitado.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakUserNotFoundException">
    /// Si el usuario con el <paramref name="userId"/> indicado no existe en Keycloak.
    /// </exception>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición PATCH/PUT a Keycloak falla con un código HTTP no exitoso.
    /// </exception>
    Task<bool> ToggleUserStatusAsync(string userId, CancellationToken ct);
 
    // =========================================================
    // OBTENER MÚLTIPLES USUARIOS POR IDS
    // =========================================================
 
    /// <summary>
    /// Obtiene una lista de usuarios a partir de una colección de IDs en una
    /// única petición HTTP.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Utiliza la sintaxis de búsqueda nativa <c>id:uuid1 uuid2 uuid3</c>
    /// introducida en Keycloak <b>26.3.0</b>, resolviendo todos los IDs en
    /// <b>una sola petición HTTP</b> en lugar de N peticiones paralelas.
    /// </para>
    /// <para>
    /// Los IDs inexistentes son ignorados silenciosamente por Keycloak;
    /// el resultado solo contiene los usuarios que efectivamente se encontraron.
    /// </para>
    /// <para>
    /// Si la colección <paramref name="userIds"/> está vacía o solo contiene
    /// entradas en blanco, retorna una lista vacía sin realizar ninguna petición.
    /// </para>
    /// </remarks>
    /// <param name="userIds">
    /// Colección de UUIDs de usuarios a consultar. Las entradas vacías o
    /// en blanco se filtran automáticamente.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Lista de <see cref="KeycloakUserDto"/> para los IDs encontrados.
    /// El orden no está garantizado.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición HTTP a Keycloak falla con un código no exitoso.
    /// </exception>
    Task<List<KeycloakUserDto>> GetUsersByIdsAsync(IEnumerable<string> userIds, CancellationToken ct);
 
    // =========================================================
    // EDICIÓN DE USUARIO
    // =========================================================
 
    /// <summary>
    /// Actualiza los datos principales de un usuario (sin contraseña ni username).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Realiza un merge entre los datos actuales del usuario y los nuevos valores
    /// provistos en <paramref name="updateDto"/>. Solo los campos con valor no nulo
    /// en el DTO sobreescriben los valores existentes; los campos omitidos se conservan.
    /// </para>
    /// <para>
    /// El <c>username</c> y la contraseña no son modificables mediante este método
    /// por diseño, para evitar cambios accidentales en credenciales de acceso.
    /// </para>
    /// <para>
    /// Si <paramref name="updateDto"/> incluye un <c>NewRoleName</c> distinto al
    /// rol actual, se eliminan todos los roles existentes del usuario y se asigna
    /// el nuevo de forma secuencial (no atómica; en caso de fallo parcial se propaga
    /// la excepción correspondiente).
    /// </para>
    /// </remarks>
    /// <param name="userId">UUID del usuario a actualizar en Keycloak.</param>
    /// <param name="updateDto">DTO con los campos a actualizar. Los campos nulos se ignoran.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="Exceptions.KeycloakUserNotFoundException">
    /// Si el usuario con el <paramref name="userId"/> indicado no existe en Keycloak.
    /// </exception>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición PUT a Keycloak falla con un código HTTP no exitoso.
    /// </exception>
    Task UpdateUserAsync(string userId, KeycloakUpdateUserDto updateDto, CancellationToken ct);
 
    /// <summary>
    /// Reemplaza el rol de realm de un usuario eliminando los actuales y asignando el nuevo.
    /// </summary>
    /// <remarks>
    /// <para>
    /// El proceso es: (1) verificar que el usuario exista, (2) obtener sus roles actuales,
    /// (3) eliminar todos los roles encontrados y (4) asignar el nuevo rol.
    /// Los pasos 3 y 4 no son atómicos; si el paso 4 falla, el usuario podría quedar
    /// sin ningún rol asignado.
    /// </para>
    /// <para>
    /// Si el usuario no tiene roles asignados actualmente, se omite el paso de eliminación
    /// y se procede directamente con la asignación del nuevo rol.
    /// </para>
    /// </remarks>
    /// <param name="userId">UUID del usuario al que se le cambiará el rol.</param>
    /// <param name="newRoleName">Nombre exacto del rol de realm a asignar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="Exceptions.KeycloakUserNotFoundException">
    /// Si el usuario con el <paramref name="userId"/> indicado no existe en Keycloak.
    /// </exception>
    /// <exception cref="Exceptions.KeycloakRoleNotFoundException">
    /// Si el rol <paramref name="newRoleName"/> no existe en el realm.
    /// </exception>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si alguna de las operaciones HTTP intermedias falla con un código no exitoso.
    /// </exception>
    Task UpdateUserRoleAsync(string userId, string newRoleName, CancellationToken ct);
 
    // =========================================================
    // LISTAR USUARIOS CON PAGINACIÓN
    // =========================================================
 
    /// <summary>
    /// Obtiene una página de usuarios con soporte de paginación y filtro opcional.
    /// </summary>
    /// <remarks>
    /// <para>
    /// La paginación es <b>offset-based</b>: <paramref name="first"/> indica el índice
    /// del primer elemento (0-based) y <paramref name="max"/> la cantidad máxima de
    /// elementos a retornar.
    /// </para>
    /// <para>
    /// Cuando se provee <paramref name="usernameFilter"/>, Keycloak aplica una búsqueda
    /// parcial sobre el campo <c>username</c>. Si el parámetro es <see langword="null"/>
    /// o vacío, se retornan todos los usuarios sin filtrar.
    /// </para>
    /// <para>
    /// Keycloak no incluye el total real de registros en respuestas de lista filtrada,
    /// por lo que no es posible calcular <c>TotalPages</c> ni <c>TotalItems</c> desde
    /// este método.
    /// </para>
    /// </remarks>
    /// <param name="first">Índice de inicio (0-based) para la paginación.</param>
    /// <param name="max">Cantidad máxima de usuarios a retornar en esta página.</param>
    /// <param name="usernameFilter">
    /// Filtro parcial sobre el campo <c>username</c>. Pasar <see langword="null"/>
    /// o cadena vacía para no aplicar filtro.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Lista paginada de <see cref="KeycloakUserDto"/>. Puede retornar hasta
    /// <paramref name="max"/> elementos.
    /// </returns>
    /// <exception cref="Exceptions.KeycloakApiException">
    /// Si la petición HTTP a Keycloak falla con un código no exitoso.
    /// </exception>
    Task<List<KeycloakUserDto>> GetUsersFilteredAsync(
        int first,
        int max,
        string? usernameFilter,
        CancellationToken ct = default);
}