namespace SIGREF.API.Constants;

public class Env
{
    public PhirConfig Fhir { get; set; } = new();
    public FhirConfig Phir { get; set; } = new();
    public MongoDbConfig MongoDB { get; set; } = new();
}

public class FhirConfig
{
    public string BaseUrl { get; set; } = string.Empty;
}
public class PhirConfig
{
    public string BaseUrl { get; set; } = string.Empty;
}

public class MongoDbConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string AuditLogsCollection { get; set; } = string.Empty;
}