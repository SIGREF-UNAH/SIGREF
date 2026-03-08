namespace SIGREF.Common.Models;

public class KeycloakOptions
{
    public const string SectionName = "keycloak"; // Debe coincidir con el registrado en el api.sigref.appsettings.json

    public string Url { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string RealmName { get; set; } = string.Empty;
    public string AdminClientId { get; set; } = string.Empty;
    public string AdminClientSecret { get; set; } = string.Empty;
    
}
