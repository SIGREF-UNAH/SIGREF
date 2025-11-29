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