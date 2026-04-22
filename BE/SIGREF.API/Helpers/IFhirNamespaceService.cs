namespace SIGREF.API.Helpers;

public interface IFhirNamespaceService
{
    string Base { get; }
    string AuditSystem { get; }
    string GetUserSource(Guid userId);
    // Definiciones de Estructura
    string HealthcareServiceAbbreviation { get; }
    string HealthcareServiceScope { get; }
    // Parámetros de búsqueda
    string SpPatientIdentifierValue { get; }
    string SpPatientIdentifierType { get; }
}