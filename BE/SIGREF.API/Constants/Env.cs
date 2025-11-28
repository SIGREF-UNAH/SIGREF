namespace SIGREF.API.Constants;

public class Env
{
    public PhirConfig Phir { get; set; } = new();
    public FhirConfig Fhir { get; set; } = new();
    public KeycloakConfig Keycloak { get; set; } = new();
}




public class FhirConfig
{
    public string BaseUrl { get; set; } = string.Empty;
}
public class PhirConfig
{
    public string BaseUrl { get; set; } = string.Empty;
}

public class MongoSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
}
public class KeycloakConfig
{
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttps { get; set; } = false;

    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = "sigref";
    public string AdminClientId { get; set; } = string.Empty;
    public string AdminClientSecret { get; set; } = string.Empty;
}