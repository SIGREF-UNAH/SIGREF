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
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Fecha de la última actualización de perfil/credenciales.
    /// Nota: atributo custom
    /// </summary>
    public DateTimeOffset? LastModifiedAt { get; set; }
}


public class KeycloakUsernameDto
{
    public bool ExistName { get; set; }                 // Hay coincidencias?
    public int? NumberList { get; set; }                // Cantidad de coincidencias
    public List<string>? Usernames { get; set; }        // Lista de usernames similares
}





public class KeycloakFilter 
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? UserName { get; set; }
    public string? Search { get; set; } // Para buscar por nombre, email o username
}
