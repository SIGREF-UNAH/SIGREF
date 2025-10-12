namespace SIGREF.API.Constants;
public static class TerminologyConstants
{
    // CodeSystems
    public const string RolesAdminCodeSystemUrl = "https://hospitalpublico.hn/fhir/CodeSystem/roles-admin";
    public const string TiposUbicacionCodeSystemUrl = "https://hospitalpublico.hn/fhir/CodeSystem/tipos-ubicacion";

    // ValueSets
    public const string RolesAdminValueSetUrl = "https://hospitalpublico.hn/fhir/ValueSet/roles-admin";
    public const string TiposUbicacionValueSetUrl = "https://hospitalpublico.hn/fhir/ValueSet/tipos-ubicacion";

    // Conceptos: Roles
    public static readonly Dictionary<string, string> RolesAdminConcepts = new()
    {
        // Llave: Constante en Roles.cs
        [Roles.cashier] = "aux-recepcion",
        [Roles.admin] = "admin-fondos",
        [Roles.ti] = "personal-tic",
        [Roles.auditor] = "auditor-sistema"
    };

    // Conceptos: Ubicaciones
    public static readonly Dictionary<string, string> TiposUbicacionConcepts = new()
    {
        ["emergencias"] = "Área de Emergencias",
        ["consulta-externa"] = "Consulta Externa",
        ["med-hombres"] = "Medicina de Hombres",
        ["med-mujeres"] = "Medicina de Mujeres",
        ["cirugia"] = "Área de Cirugía",
        ["odontologia"] = "Odontología"
    };
}

