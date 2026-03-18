using System.ComponentModel.DataAnnotations;

namespace SIGREF.Infrastructure.Keycloak.Dtos.Auth;

public class KeycloakUserDto
{
    public string Id { get; set; } = default!;            // ID interno de Keycloak
    public string Username { get; set; } = default!;
    public string? Email { get; set; }
    public string? DisplayName { get; set; }

    // Atributo crítico de negocio
    public string PractitionerId { get; set; } = default!;
    public bool? Enabled { get; set; }

    
    public List<string>? Roles { get; set; }
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
