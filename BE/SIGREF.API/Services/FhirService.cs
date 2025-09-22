using Hl7.Fhir.Rest;
using Microsoft.Extensions.Options;
using SIGREF.API.Constants;

namespace SIGREF.API.Services;

public class FhirService
{
    private readonly FhirClient _fhirClient;

    public FhirService(IOptions<Env> env)
    {
        var fhirEndpoint = env.Value;
        _fhirClient = new FhirClient(fhirEndpoint.Phir.BaseUrl, new FhirClientSettings()

            {
                Timeout = 10000,
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                ReturnPreference = ReturnPreference.Minimal,
                 
            }
        );
    }


    public FhirClient GetFhirClient()
    {
        return _fhirClient;
    }
}