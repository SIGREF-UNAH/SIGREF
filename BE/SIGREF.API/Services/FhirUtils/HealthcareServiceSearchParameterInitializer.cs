using Hl7.Fhir.Model;
using SIGREF.API.Services.Common;
using SIGREF.Common.Constants;

namespace SIGREF.API.Services.FhirUtils;

/// <summary>
/// SearchParameters propios de HealthcareService.
/// Usa ExtensionUrl porque son campos custom, no nativos del recurso FHIR.
/// </summary>
public class HealthcareServiceSearchParameterInitializer : BaseFhirSearchParameterInitializer
{
    public HealthcareServiceSearchParameterInitializer(
        FhirService fhirService,
        ILogger<HealthcareServiceSearchParameterInitializer> logger)
        : base(fhirService.GetFhirClient(), logger)
    {
    }
 
    protected override IEnumerable<SearchParameterDefinition> GetDefinitions() =>
    [
        new(
            Code:        "abbreviation",
            Name:        "HealthcareServiceAbbreviation",
            Type:        SearchParamType.Token,
            Base:        ResourceType.HealthcareService,
            Description: "Busqueda por abreviatura de HealthcareService",
            Url:          FhirNamespaces.SpHealthcareServiceAbbreviation,
            ExtensionUrl: FhirNamespaces.HealthcareServiceAbbreviation
        ),
        new(
            Code:        "scope",
            Name:        "HealthcareServiceScope",
            Type:        SearchParamType.Token,
            Base:        ResourceType.HealthcareService,
            Description: "Busqueda por alcance del servicio (internal | external)",
            Url:          FhirNamespaces.SpHealthcareServiceScope,
            ExtensionUrl: FhirNamespaces.HealthcareServiceScope
        ),
    ];
}