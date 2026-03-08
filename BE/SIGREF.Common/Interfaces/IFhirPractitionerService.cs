namespace SIGREF.Common.Interfaces;

public interface IFhirPractitionerService
{
    /// <summary>
    /// Realiza una búsqueda quirúrgica en FHIR para obtener datos básicos.
    /// </summary>
    Task<(bool Exists, string? FirstName, string? LastName, bool IsActive)> GetBasicDataAsync(string practitionerId);
}