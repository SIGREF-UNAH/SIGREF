using Microsoft.Extensions.Options;
using SIGREF.Common.Constants;

namespace SIGREF.API.Helpers;

public class FhirNamespaceService(IOptions<HospitalOptions> options) : IFhirNamespaceService
{
    private readonly HospitalOptions _options = options.Value;

    public string Base => _options.BaseUrl;
    public string AuditSystem => $"{Base}/audit-id";

    public string GetUserSource(Guid userId) => 
        string.Format(_options.UserSourceSchema, _options.HospitalCode.ToLower(), userId);

    public string HealthcareServiceAbbreviation => $"{Base}/StructureDefinition/healthcareservice-abbreviation";
    public string HealthcareServiceScope => $"{Base}/StructureDefinition/healthcareservice-scope";
    
    public string SpPatientIdentifierValue => $"{Base}/SearchParameter/identifier-value";
    public string SpPatientIdentifierType => $"{Base}/SearchParameter/identifier-type";
}