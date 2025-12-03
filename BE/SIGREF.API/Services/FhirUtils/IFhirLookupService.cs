namespace SIGREF.API.Services.FhirUtils;

public interface IFhirLookupService
{
    /// <summary>
    /// Obtiene nombres de Location desde FHIR utilizando búsquedas batch.
    /// </summary>
    Task<Dictionary<string, string>> GetLocationNamesAsync(IEnumerable<string> fhirIds);

    /// <summary>
    /// Obtiene nombres de HealthcareService desde FHIR utilizando búsquedas batch.
    /// </summary>
    Task<Dictionary<string, string>> GetServiceNamesAsync(IEnumerable<string> fhirIds);

    /// <summary>
    /// Obtiene nombres de Organization (paquetes) desde FHIR utilizando búsquedas batch.
    /// </summary>
    Task<Dictionary<string, string>> GetPackageNamesAsync(IEnumerable<string> fhirIds);
}