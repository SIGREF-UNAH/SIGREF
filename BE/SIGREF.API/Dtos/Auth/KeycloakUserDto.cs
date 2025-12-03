namespace SIGREF.API.Dtos.Auth;

public class KeycloakUserDto
{
    public string Id { get; set; } = default!;            // ID interno de Keycloak
    public string Username { get; set; } = default!;
    public string? Email { get; set; }
    public string? DisplayName { get; set; }

    // Atributo crítico de negocio
    public string PractitionerId { get; set; } = default!;
}