namespace SIGREF.API.Constants;

public class Env
{
    public PhirConfig Phir { get; set; } = new();
    public FhirConfig Fhir { get; set; } = new();
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