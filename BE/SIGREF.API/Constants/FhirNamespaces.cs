namespace SIGREF.API.Constants;


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
    // CodeSystems
    // =========================
    public const string RolesAdminCodeSystem =
        Base + "/CodeSystem/roles-admin";

    public const string TiposUbicacionCodeSystem =
        Base + "/CodeSystem/tipos-ubicacion";

    // =========================
    // ValueSets
    // =========================
    public const string RolesAdminValueSet =
        Base + "/ValueSet/roles-admin";

    public const string TiposUbicacionValueSet =
        Base + "/ValueSet/tipos-ubicacion";
}