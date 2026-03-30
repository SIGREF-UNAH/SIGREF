namespace SIGREF.Common.Constants;


public static class FhirNamespaces
{
    // Base namespace (no requiere dominio real) 
    // Para cuando si este en la web cambiar aqui
    
    public const string Base = "http://sigref.local/fhir";

    // StructureDefinitions
    public const string HealthcareServiceAbbreviation =
        Base + "/StructureDefinition/healthcareservice-abbreviation";

    public const string HealthcareServiceScope =
        Base+ "/StructureDefinition/healthcareservice-scope";
    
    // =========================
    // SearchParameters
    // =========================
 
    // HealthcareService
    public const string SpHealthcareServiceAbbreviation =
        Base + "/SearchParameter/abbreviation";
 
    public const string SpHealthcareServiceScope =
        Base + "/SearchParameter/scope";
 
    // Patient
    public const string SpPatientIdentifierValue =
        Base + "/SearchParameter/identifier-value";
 
    public const string SpPatientIdentifierType =
        Base + "/SearchParameter/identifier-type";
    
    // =========================
    // CodeSystems
    // =========================
    public const string RolesAdminCodeSystem =
        Base + "/CodeSystem/roles-admin";

    public const string TiposUbicacionCodeSystem =
        Base + "/CodeSystem/tipos-ubicacion";

    // =========================
    // ValueSets
    // =========================
    
    // Usados para la busqueda interna de los Value Set de Sigref
    public const string RolesAdminValueSet =
        Base + "/ValueSet/roles-admin";

    public const string TiposUbicacionValueSet =
        Base + "/ValueSet/tipos-ubicacion";
    
    
}