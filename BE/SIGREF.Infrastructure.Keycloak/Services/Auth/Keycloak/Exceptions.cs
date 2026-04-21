namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

/// <summary>
/// Excepción base para errores producidos al interactuar con la Admin REST API de Keycloak.
/// </summary>
/// <remarks>
/// Todas las excepciones específicas de Keycloak heredan de esta clase,
/// lo que permite capturarlas de forma unificada con un único bloque <c>catch</c>.
/// </remarks>
public class KeycloakApiException : Exception
{
    /// <summary>Código HTTP devuelto por Keycloak, si aplica.</summary>
    public int? StatusCode { get; }
 
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="KeycloakApiException"/>.
    /// </summary>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <param name="statusCode">Código HTTP devuelto por Keycloak (opcional).</param>
    /// <param name="inner">Excepción causante (opcional).</param>
    public KeycloakApiException(string message, int? statusCode = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }
}
/// <summary>
/// Excepción lanzada cuando el cliente de SIGREF no tiene permisos suficientes
/// para realizar una operación en Keycloak (HTTP 403 Forbidden).
/// </summary>
public class KeycloakAccessForbiddenException : KeycloakApiException
{
    public KeycloakAccessForbiddenException(string message) 
        : base(message, 403) { }

    public KeycloakAccessForbiddenException(string roleName, string detail)
        : base($"Acceso denegado al gestionar el rol '{roleName}'. Detalle: {detail}", 403) { }
}
/// <summary>
/// Excepción lanzada cuando un usuario no es encontrado en Keycloak (HTTP 404).
/// </summary>
/// <remarks>
/// Se utiliza para diferenciar la ausencia de un recurso de otros errores de API,
/// evitando el uso silencioso de valores <c>null</c> en flujos donde la existencia
/// del usuario es un prerequisito obligatorio.
/// </remarks>
public class KeycloakUserNotFoundException : KeycloakApiException
{
    /// <summary>ID del usuario que no fue encontrado.</summary>
    public string UserId { get; }
 
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="KeycloakUserNotFoundException"/>.
    /// </summary>
    /// <param name="userId">UUID del usuario que no fue encontrado.</param>
    public KeycloakUserNotFoundException(string userId)
        : base($"El usuario con ID '{userId}' no existe en Keycloak.", statusCode: 404)
    {
        UserId = userId;
    }
}
 
/// <summary>
/// Excepción lanzada cuando un rol de realm no es encontrado en Keycloak.
/// </summary>
public class KeycloakRoleNotFoundException : KeycloakApiException
{
    /// <summary>Nombre del rol que no fue encontrado.</summary>
    public string RoleName { get; }
 
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="KeycloakRoleNotFoundException"/>.
    /// </summary>
    /// <param name="roleName">Nombre del rol que no fue encontrado.</param>
    public KeycloakRoleNotFoundException(string roleName)
        : base($"El rol '{roleName}' no existe en el realm de Keycloak.", statusCode: 404)
    {
        RoleName = roleName;
    }
}