using System.Security.Claims;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;

namespace SIGREF.Infrastructure.Keycloak.Interfaces;

/// <summary>
///     Contrato de la capa de aplicación para la gestión de usuarios en Keycloak.
/// </summary>
/// <remarks>
///     <para>
///         Esta interfaz actúa como fachada sobre <see cref="IKeycloakClient" />, añadiendo
///         lógica de negocio: validación de permisos por rol, integración con FHIR,
///     </para>
///     <para>
///         Las reglas de jerarquía de roles determinan qué operaciones puede realizar
///         cada tipo de usuario (p. ej., solo <c>ti</c> puede crear otros usuarios con
///         rol <c>admin</c>).
///     </para>
///     <para>
///         Ningún método de esta interfaz expone <see cref="System.Text.Json.JsonElement" />
///         al llamador; todas las respuestas están mapeadas a DTOs tipados.
///     </para>
/// </remarks>
public interface IKeycloakAdminService
{
    // =========================================================
    // CREAR USUARIO
    // =========================================================

    /// <summary>
    ///     Crea un usuario en Keycloak vinculado a un Practitioner FHIR.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         La creación sigue un flujo de validación estricto antes de ejecutar
    ///         cualquier escritura en Keycloak:
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 El creador debe tener un rol válido en el sistema y dicho rol debe
    ///                 permitir la asignación de cada uno de los roles solicitados según
    ///                 las reglas de jerarquía.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 El Practitioner debe existir y estar activo en el servidor FHIR.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 El Practitioner no debe tener ya un usuario de Keycloak vinculado
    ///                 (un Practitioner → un usuario, relación 1:1).
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         Si la creación del usuario falla tras haber sido registrado en Keycloak
    ///         (p. ej. al asignar roles), se ejecuta automáticamente un rollback que
    ///         elimina el usuario creado para evitar registros huérfanos.
    ///     </para>
    /// </remarks>
    /// <param name="creator">
    ///     Claims del usuario autenticado que realiza la operación. Se usan para
    ///     extraer el rol del creador y validar su jerarquía.
    /// </param>
    /// <param name="username">Nombre de usuario único en el realm (case-insensitive en Keycloak).</param>
    /// <param name="practitionerId">ID del Practitioner FHIR a vincular al nuevo usuario.</param>
    /// <param name="email">Correo electrónico del nuevo usuario.</param>
    /// <param name="password">Contraseña inicial del usuario (no se marca como temporal).</param>
    /// <param name="roles">
    ///     Arreglo de nombres de roles de realm a asignar. Todos deben existir en el
    ///     realm y ser permitidos por el rol del creador.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con el <see cref="KeycloakUserDto" /> creado en caso
    ///     de éxito, o con el código y mensaje de error correspondiente si alguna validación
    ///     falla (401, 403, 404, 400, 500).
    /// </returns>
    Task<KeycloakUserDto> CreateUserAsync(
        ClaimsPrincipal creator,
        string username,
        string practitionerId,
        string email,
        string password,
        string[] roles,
        CancellationToken ct = default);

    // =========================================================
    // OBTENER USUARIO
    // =========================================================

    /// <summary>
    ///     Obtiene un usuario por su ID interno de Keycloak.
    /// </summary>
    /// <param name="keycloakUserId">UUID del usuario en Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con el <see cref="KeycloakUserDto" /> si el usuario
    ///     existe, o con <c>Data = null</c> si no fue encontrado (HTTP 404 se trata como
    ///     resultado vacío, no como error).
    /// </returns>
    Task<KeycloakUserDto?> GetUserByIdAsync(
        string keycloakUserId,
        CancellationToken ct = default);

    /// <summary>
    ///     Obtiene en una sola petición HTTP varios usuarios a partir de una lista de IDs.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Internamente usa la sintaxis <c>id:uuid1 uuid2 …</c> de Keycloak 26.3+,
    ///         lo que permite resolver todos los IDs en <b>una única petición</b>
    ///         en lugar de N llamadas paralelas.
    ///     </para>
    ///     <para>
    ///         Los IDs no encontrados son ignorados silenciosamente; el resultado
    ///         solo incluye los usuarios efectivamente hallados.
    ///     </para>
    /// </remarks>
    /// <param name="userIds">
    ///     Colección de UUIDs de Keycloak a consultar. Las entradas vacías se descartan.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con la lista de <see cref="KeycloakUserDto" />
    ///     encontrados. La lista puede estar vacía si ningún ID existe.
    /// </returns>
    Task<List<KeycloakUserDto>> GetUsersByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default);

    /// <summary>
    ///     Busca el usuario de Keycloak vinculado a un Practitioner FHIR específico.
    /// </summary>
    /// <remarks>
    ///     La búsqueda se realiza sobre el atributo personalizado <c>practitionerId</c>
    ///     almacenado en Keycloak durante la creación del usuario.
    /// </remarks>
    /// <param name="practitionerId">ID del Practitioner FHIR a buscar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con el <see cref="KeycloakUserDto" /> si se encontró
    ///     un usuario vinculado, o con <c>Data = null</c> si no existe ninguno.
    /// </returns>
    Task<KeycloakUserDto?> GetUserByPractitionerIdAsync(
        string practitionerId,
        CancellationToken ct = default);

    /// <summary>
    ///     Verifica si un Practitioner FHIR ya tiene un usuario de Keycloak vinculado.
    /// </summary>
    /// <remarks>
    ///     Útil para validaciones previas a la creación de usuarios, garantizando la
    ///     restricción de relación 1:1 entre Practitioner y usuario de Keycloak.
    /// </remarks>
    /// <param name="practitionerId">ID del Practitioner FHIR a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con <see langword="true" /> si ya existe un usuario
    ///     vinculado al Practitioner, o <see langword="false" /> si no tiene usuario asignado.
    /// </returns>
    Task<bool> PractitionerHasUserAsync(
        string practitionerId,
        CancellationToken ct = default);

    /// <summary>
    ///     Verifica si un username ya está en uso en el realm y retorna usernames similares.
    /// </summary>
    /// <remarks>
    ///     La comparación exacta es case-insensitive. Además del resultado booleano,
    ///     el DTO incluye una lista de usernames similares que pueden usarse para
    ///     sugerir alternativas al usuario final.
    /// </remarks>
    /// <param name="username">Username a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con un <see cref="KeycloakUsernameDto" /> que indica
    ///     si el username está tomado (<c>ExistName</c>) y lista los usernames similares
    ///     encontrados (<c>Usernames</c>).
    /// </returns>
    Task<KeycloakUsernameDto> ExistUserNameAsync(
        string username,
        CancellationToken ct = default);

    // =========================================================
    // LISTAR USUARIOS
    // =========================================================

    /// <summary>
    ///     Obtiene una página de usuarios del realm con filtros opcionales.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         El tamaño de página está limitado a un máximo de <b>30 elementos</b> para
    ///         proteger el rendimiento del servidor de identidad y evitar respuestas
    ///         excesivamente grandes.
    ///     </para>
    ///     <para>
    ///         Keycloak no devuelve el total real de registros en listas filtradas, por lo
    ///         que <c>TotalItems</c> y <c>TotalPages</c> siempre serán <see langword="null" />
    ///         en el resultado de paginación. La presencia de página siguiente se detecta
    ///         solicitando un elemento extra (<c>pageSize + 1</c>) y verificando si se recibió.
    ///     </para>
    /// </remarks>
    /// <param name="filter">
    ///     Parámetros de paginación y filtrado. Incluye <c>PageNumber</c>, <c>PageSize</c>,
    ///     <c>Search</c> (texto libre con precedencia) y <c>UserName</c> (filtro por username).
    /// </param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con un <see cref="PagedResultDto{T}" /> de
    ///     <see cref="KeycloakUserDto" />. <c>TotalItems</c> y <c>TotalPages</c> son siempre
    ///     <see langword="null" /> por limitación de la API de Keycloak.
    /// </returns>
    Task<PagedResultDto<KeycloakUserDto>> GetUsersListAsync(
        KeycloakFilter filter,
        CancellationToken ct = default);

    // =========================================================
    // TOGGLE DE ESTADO
    // =========================================================

    /// <summary>
    ///     Alterna el estado activo/inactivo de un usuario en el realm.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Solo un usuario con rol <c>ti</c> o <c>admin</c> puede ejecutar esta operación.
    ///     </para>
    ///     <para>
    ///         Un usuario no puede cambiar su propio estado (auto-desactivación prohibida)
    ///         para evitar bloqueos accidentales de la cuenta del operador activo.
    ///     </para>
    /// </remarks>
    /// <param name="requestor">
    ///     Claims del usuario autenticado que solicita el cambio de estado. Se verifican
    ///     su rol y su ID para aplicar las restricciones descritas.
    /// </param>
    /// <param name="targetUserId">UUID del usuario cuyo estado se va a alternar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con <see langword="true" /> si el usuario quedó
    ///     habilitado, o <see langword="false" /> si fue deshabilitado.
    ///     Retorna error 403 si el solicitante no tiene permisos, o 400 si intenta
    ///     modificar su propio estado.
    /// </returns>
    Task<bool> ToggleUserStatusAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        CancellationToken ct = default);

    // =========================================================
    // EDICIÓN DE USUARIO
    // =========================================================

    /// <summary>
    ///     Actualiza los datos de un usuario existente (sin contraseña ni username).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Solo los campos con valor no nulo en <paramref name="updateDto" /> son
    ///         modificados; los demás conservan su valor actual almacenado en Keycloak.
    ///     </para>
    ///     <para>
    ///         Si <paramref name="updateDto" /> incluye <c>NewRoleName</c>, el rol actual del
    ///         usuario es reemplazado. Solo un usuario con rol <c>ti</c> o <c>admin</c> puede
    ///         cambiar roles, y se aplican las mismas restricciones de jerarquía que en
    ///         <see cref="CreateUserAsync" />.
    ///     </para>
    /// </remarks>
    /// <param name="requestor">
    ///     Claims del usuario autenticado que realiza la edición. Se verifican su rol
    ///     y permisos de jerarquía antes de aplicar cambios.
    /// </param>
    /// <param name="targetUserId">UUID del usuario a editar en Keycloak.</param>
    /// <param name="updateDto">
    ///     DTO con los campos a actualizar. Los campos con valor <see langword="null" />
    ///     son ignorados y los valores existentes se conservan.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con el <see cref="KeycloakUserDto" /> actualizado,
    ///     o con el código de error correspondiente si alguna validación falla (403, 404).
    /// </returns>
    Task<KeycloakUserDto> UpdateUserAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        KeycloakUpdateUserDto updateDto,
        CancellationToken ct = default);

    // =========================================================
    // ELIMINAR USUARIO
    // =========================================================

    /// <summary>
    ///     Elimina permanentemente un usuario del realm de Keycloak.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         La eliminación es <b>irreversible</b>. Antes de proceder se validan
    ///         los siguientes puntos en orden:
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 El solicitante debe tener rol <c>ti</c> o <c>admin</c>.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Un usuario no puede eliminarse a sí mismo.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 El usuario objetivo debe existir en Keycloak.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         Se recomienda usar <see cref="ToggleUserStatusAsync" /> para bajas
    ///         lógicas (reversibles). Esta operación debe reservarse para eliminaciones
    ///         definitivas o rollbacks durante la creación de usuarios.
    ///     </para>
    /// </remarks>
    /// <param name="requestor">
    ///     Claims del usuario autenticado que solicita la eliminación. Se verifican
    ///     su rol y su ID para aplicar las restricciones de seguridad.
    /// </param>
    /// <param name="targetUserId">UUID del usuario a eliminar en Keycloak.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    ///     <see cref="ResponseDto{T}" /> con <see langword="true" /> si el usuario fue
    ///     eliminado correctamente.
    ///     Retorna error 403 si el solicitante no tiene permisos o intenta eliminarse
    ///     a sí mismo, o 404 si el usuario objetivo no existe.
    /// </returns>
    Task DeleteUserAsync(
        ClaimsPrincipal requestor,
        string targetUserId,
        CancellationToken ct = default);
}