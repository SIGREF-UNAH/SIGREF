using Hl7.Fhir.Model;

namespace SIGREF.API.Services.FhirUtils;

/// <summary>
/// Define un SearchParameter a registrar en HAPI FHIR.
///
/// Soporta dos modos mutuamente excluyentes:
///   - Expression directa : campos nativos del recurso  → proporcionar Expression
///   - Extension dinámica : campos custom via extensión → proporcionar ExtensionUrl
///
/// Ejemplo con Expression directa (Patient campos nativos):
///   new SearchParameterDefinition(
///       Code: "identifier-value",
///       Name: "PatientIdentifierValueString",
///       Type: SearchParamType.String,
///       Base: ResourceType.Patient,
///       Description: "Busqueda por valor del identificador",
///       Expression: "Patient.identifier.value"
///   )
///
/// Ejemplo con ExtensionUrl (HealthcareService campos custom):
///   new SearchParameterDefinition(
///       Code: "abbreviation",
///       Name: "HealthcareServiceAbbreviation",
///       Type: SearchParamType.Token,
///       Base: ResourceType.HealthcareService,
///       Description: "Busqueda por abreviatura",
///       ExtensionUrl: "http://example.org/fhir/StructureDefinition/abbreviation"
///   )
/// </summary>
public record SearchParameterDefinition(
    string Code,
    string Name,
    SearchParamType Type,
    ResourceType Base,
    string Description,
    string Url,
    string? Expression = null,
    string? ExtensionUrl = null)
{
    /// <summary>
    /// Resuelve la FHIRPath expression final segun el modo configurado.
    /// Lanza InvalidOperationException si ninguno fue proporcionado.
    /// </summary>
    public string ResolveExpression()
    {
        if (Expression is not null && ExtensionUrl is not null)
            throw new InvalidOperationException(
                $"[FHIR-SP] '{Code}': no se puede tener Expression y ExtensionUrl al mismo tiempo. Use solo uno.");
 
        if (Expression is not null)
            return Expression;
 
        if (ExtensionUrl is not null)
            return $"{Base}.extension.where(url='{ExtensionUrl}').value";
 
        throw new InvalidOperationException(
            $"[FHIR-SP] '{Code}': debe proporcionar Expression (campo nativo) o ExtensionUrl (campo custom).");
    }
}