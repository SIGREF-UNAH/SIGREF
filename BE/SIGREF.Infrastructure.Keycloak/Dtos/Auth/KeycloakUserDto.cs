namespace SIGREF.Infrastructure.Keycloak.Dtos.Auth;

public class KeycloakUserDto
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }

    // Atributo crítico de negocio
    public string PractitionerId { get; set; } = default!;
    public bool? Enabled { get; set; }
    
    // Por limitaciones solo se incluye en el get bi ID

    public List<string>? Roles { get; set; }
    
    /// <summary>
    /// Fecha de creación del usuario (mapeado desde 'createdTimestamp' de Keycloak)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Fecha de la última actualización de perfil/credenciales.
    /// Nota: atributo custom
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
}


public class KeycloakUsernameDto
{
    public bool ExistName { get; set; }                 // Hay coincidencias?
    public int? NumberList { get; set; }                // Cantidad de coincidencias
    public List<string>? Usernames { get; set; }        // Lista de usernames similares
}





public class KeycloakFilter
{
    public int     PageNumber { get; set; }
    public int     PageSize   { get; set; }

    // TODO: Crear un atributo de validación (o implementar IValidatableObject) para esta clase. 
    // Requerimiento: Si el cliente envía una búsqueda específica por UserName, Email, FirstName o LastName, 
    // no debe permitirse el uso del campo general 'Search' simultáneamente.
    /// <summary>
    /// Búsqueda amplia: Keycloak aplica sobre username, email, firstName y lastName.
    /// </summary>
    public string? Search    { get; set; }

    /// <summary>
    /// Filtro exacto por username (usa el parámetro ?username= del endpoint).
    /// </summary>
    public string? UserName  { get; set; }

    /// <summary>
    /// Filtro exacto por email (usa el parámetro ?email= del endpoint).
    /// </summary>
    public string? Email     { get; set; }

    /// <summary>
    /// Filtro por firstName.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Filtro por lastName.
    /// </summary>
    public string? LastName  { get; set; }
}
