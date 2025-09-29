namespace SIGREF.API.Constants;

public class Env
{
    public FhirConfig Fhir { get; set; } = new();
}

public class FhirConfig
{
    public string BaseUrl { get; set; } = string.Empty;
}