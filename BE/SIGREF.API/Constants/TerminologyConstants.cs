namespace SIGREF.API.Constants;

public static class TerminologyConstants
{
    // =========================
    // CodeSystems
    // =========================
    public const string RolesAdminCodeSystemUrl =
        FhirNamespaces.RolesAdminCodeSystem;

    public const string TiposUbicacionCodeSystemUrl =
        FhirNamespaces.TiposUbicacionCodeSystem;

    // =========================
    // ValueSets
    // =========================
    public const string RolesAdminValueSetUrl =
        FhirNamespaces.RolesAdminValueSet;

    public const string TiposUbicacionValueSetUrl =
        FhirNamespaces.TiposUbicacionValueSet;

    // Conceptos: Roles
    public static readonly Dictionary<string, string> RolesAdminConcepts = new()
    {
        // Llave: Constante en Roles.cs
        // Roles de usuarios
        [RolesConstants.cashier] = "aux-recepcion",
        [RolesConstants.admin] = "admin-fondos",
        [RolesConstants.ti] = "personal-tic",
        [RolesConstants.auditor] = "auditor-sistema",

        // Roles de empleados
        ["doctor"] = "Doctor",
        ["nurse"] = "Enfermero",
        ["pharmacist"] = "Farmacéutico",
        ["receptionist"] = "Recepcionista",
        ["laboratory-technician"] = "Técnico de Laboratorio",
        ["physiotherapist"] = "Fisioterapeuta",
        ["dietitian"] = "Dietista",
        ["optometrist"] = "Optometrista",
        ["dentist"] = "Dentista",
        ["psychologist"] = "Psicólogo",
        ["midwife"] = "Partera",
        ["paramedic"] = "Paramédico",
        ["anesthetist"] = "Anestesista",
        ["cardiologist"] = "Cardiólogo",
        ["radiologist"] = "Radiólogo"
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