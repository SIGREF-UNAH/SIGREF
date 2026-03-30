using Hl7.Fhir.Model;
using SIGREF.API.Services.Common;
using SIGREF.Common.Constants;

namespace SIGREF.API.Services.FhirUtils;

/// <summary>
/// SearchParameters propios de Patient.
/// Usa Expression directa porque son campos nativos del recurso FHIR,
/// no extensiones custom.
/// </summary>
public class PatientSearchParameterInitializer : BaseFhirSearchParameterInitializer
{
    public PatientSearchParameterInitializer(
        FhirService fhirService,
        ILogger<PatientSearchParameterInitializer> logger)
        : base(fhirService.GetFhirClient(), logger)
    {
    }
 
    protected override IEnumerable<SearchParameterDefinition> GetDefinitions() =>
    [
        new(
            Code:        "identifier-value",
            Name:        "PatientIdentifierValueString",
            Type:        SearchParamType.String,
            Base:        ResourceType.Patient,
            Description: "Busqueda por prefijo en el valor del identificador del paciente",
            Url:         FhirNamespaces.SpPatientIdentifierValue,
            Expression:  "Patient.identifier.value"
        ),
        new(
            Code:        "identifier-type",
            Name:        "PatientIdentifierType",
            Type:        SearchParamType.Token,
            Base:        ResourceType.Patient,
            Description: "Busqueda por el tipo de identificador del paciente",
            Url:         FhirNamespaces.SpPatientIdentifierType,
            Expression:  "Patient.identifier.type.coding.code"
        ),
    ];
}