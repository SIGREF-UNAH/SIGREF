using SIGREF.Common.Constants;

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
        //[RolesConstants.cashier] = "aux-recepcion",
        //[RolesConstants.admin] = "admin-fondos",
        //[RolesConstants.ti] = "personal-tic",
        //[RolesConstants.auditor] = "auditor-sistema",

        // =========================
        // CLÍNICOS
        // =========================
        ["doctor"] = "Doctor",
        ["general-practitioner"] = "Médico General",
        ["specialist-doctor"] = "Médico Especialista",
        ["surgeon"] = "Cirujano",
        ["cardiologist"] = "Cardiólogo",
        ["neurologist"] = "Neurólogo",
        ["oncologist"] = "Oncólogo",
        ["pediatrician"] = "Pediatra",
        ["gynecologist"] = "Ginecólogo",
        ["urologist"] = "Urólogo",
        ["orthopedist"] = "Ortopedista",
        ["anesthetist"] = "Anestesista",
        ["radiologist"] = "Radiólogo",
        ["dermatologist"] = "Dermatólogo",
        ["ophthalmologist"] = "Oftalmólogo",
        ["psychiatrist"] = "Psiquiatra",
        ["psychologist"] = "Psicólogo",
        ["nurse"] = "Enfermero",
        ["head-nurse"] = "Jefe de Enfermería",
        ["midwife"] = "Partera",
        ["paramedic"] = "Paramédico",
        ["physiotherapist"] = "Fisioterapeuta",
        ["dietitian"] = "Dietista",
        ["optometrist"] = "Optometrista",
        ["dentist"] = "Dentista",
        ["podiatrist"] = "Podólogo",
        ["speech-therapist"] = "Fonoaudiólogo",
        ["occupational-therapist"] = "Terapeuta Ocupacional",
        ["respiratory-therapist"] = "Terapeuta Respiratorio",
        ["pharmacist"] = "Farmacéutico",
        ["laboratory-technician"] = "Técnico de Laboratorio",
        ["radiology-technician"] = "Técnico en Radiología",
        ["blood-bank-technician"] = "Técnico de Banco de Sangre",

        // =========================
        // ADMINISTRACIÓN Y GESTIÓN
        // =========================
        ["administrator"] = "Administrador",
        ["general-manager"] = "Gerente General",
        ["medical-director"] = "Director Médico",
        ["nursing-director"] = "Director de Enfermería",
        ["department-head"] = "Jefe de Departamento",
        ["quality-manager"] = "Gestor de Calidad",
        ["risk-manager"] = "Gestor de Riesgos",
        ["compliance-officer"] = "Oficial de Cumplimiento",
        ["human-resources"] = "Recursos Humanos",
        ["training-coordinator"] = "Coordinador de Capacitación",
        ["accountant"] = "Contador",
        ["finance-officer"] = "Oficial Financiero",
        ["billing-clerk"] = "Facturación",
        ["purchasing-officer"] = "Compras",
        ["procurement-manager"] = "Gestor de Adquisiciones",
        ["auditor"] = "Auditor",
        ["legal-advisor"] = "Asesor Legal",
        ["secretary"] = "Secretaria",
        ["administrative-assistant"] = "Asistente Administrativo",

        // =========================
        // ATENCIÓN AL PACIENTE
        // =========================
        ["receptionist"] = "Recepcionista",
        ["admissions-officer"] = "Admisiones",
        ["triage-nurse"] = "Enfermero de Triaje",
        ["patient-navigator"] = "Orientador de Pacientes",
        ["customer-service"] = "Atención al Cliente",
        ["social-worker"] = "Trabajador Social",

        // =========================
        // TECNOLOGÍA / INFORMÁTICA
        // =========================
        ["it-admin"] = "Administrador de TI",
        ["system-engineer"] = "Ingeniero de Sistemas",
        ["software-developer"] = "Desarrollador de Software",
        ["database-admin"] = "Administrador de Base de Datos",
        ["network-technician"] = "Técnico de Redes",
        ["support-technician"] = "Soporte Técnico",
        ["biomedical-engineer"] = "Ingeniero Biomédico",
        ["health-informatics"] = "Informática en Salud",
        ["security-officer"] = "Oficial de Seguridad Informática",
        ["data-analyst"] = "Analista de Datos",

        // =========================
        // INVESTIGACIÓN Y DOCENCIA
        // =========================
        ["researcher"] = "Investigador",
        ["clinical-research-coordinator"] = "Coordinador de Investigación Clínica",
        ["ethics-committee"] = "Comité de Ética",
        ["medical-instructor"] = "Docente Médico",
        ["nursing-instructor"] = "Docente de Enfermería",

        // =========================
        // LOGÍSTICA, LIMPIEZA Y APOYO
        // =========================
        ["cleaning-staff"] = "Personal de Limpieza",
        ["janitor"] = "Conserje",
        ["maintenance-technician"] = "Técnico de Mantenimiento",
        ["electrician"] = "Electricista",
        ["plumber"] = "Plomero",
        ["warehouse-clerk"] = "Encargado de Almacén",
        ["inventory-manager"] = "Gestor de Inventario",
        ["driver"] = "Conductor",
        ["security-guard"] = "Guardia de Seguridad",
        ["messenger"] = "Mensajero",
        ["laundry-staff"] = "Lavandería",
        ["kitchen-staff"] = "Cocina"
    };

    // Conceptos: Ubicaciones
    public static readonly Dictionary<string, string> TiposUbicacionConcepts = new()
    {
        // =========================
        // ESTRUCTURA GENERAL
        // =========================
        ["edificio"] = "Edificio",
        ["piso"] = "Piso",
        ["ala"] = "Ala",
        ["sector"] = "Sector",
        ["area"] = "Área",
        ["sala"] = "Sala",
        ["aula"] = "Aula",

        // =========================
        // ÁREAS CLÍNICAS
        // =========================
        ["emergencias"] = "Área de Emergencias",
        ["consulta-externa"] = "Consulta Externa",
        ["triage"] = "Área de Triaje",
        ["med-hombres"] = "Medicina de Hombres",
        ["med-mujeres"] = "Medicina de Mujeres",
        ["pediatria"] = "Pediatría",
        ["ginecologia"] = "Ginecología",
        ["cirugia"] = "Área de Cirugía",
        ["quirófano"] = "Quirófano",
        ["sala-recuperacion"] = "Sala de Recuperación",
        ["uci"] = "Unidad de Cuidados Intensivos (UCI)",
        ["hospitalizacion"] = "Hospitalización",
        ["aislamiento"] = "Área de Aislamiento",
        ["odontologia"] = "Odontología",
        ["oftalmologia"] = "Oftalmología",
        ["psicologia"] = "Psicología",
        ["rehabilitacion"] = "Rehabilitación",
        ["fisioterapia"] = "Fisioterapia",

        // =========================
        // LABORATORIOS
        // =========================
        ["laboratorios"] = "Laboratorios",
        ["laboratorio-clinico"] = "Laboratorio Clínico",
        ["laboratorio-hematologia"] = "Laboratorio de Hematología",
        ["laboratorio-bioquimica"] = "Laboratorio de Bioquímica",
        ["laboratorio-microbiologia"] = "Laboratorio de Microbiología",
        ["laboratorio-parasitologia"] = "Laboratorio de Parasitología",
        ["laboratorio-patologia"] = "Laboratorio de Patología",
        ["laboratorio-investigacion"] = "Laboratorio de Investigación",

        // =========================
        // DIAGNÓSTICO Y APOYO CLÍNICO
        // =========================
        ["banco-sangre"] = "Banco de Sangre",
        ["radiologia"] = "Radiología",
        ["imagenologia"] = "Imagenología",
        ["farmacia"] = "Farmacia",
        ["esterilizacion"] = "Central de Esterilización",
        ["anatomia-patologica"] = "Anatomía Patológica",

        // =========================
        // ADMINISTRACIÓN Y OFICINAS
        // =========================
        ["direccion"] = "Dirección",
        ["administracion"] = "Administración",
        ["oficina-ti"] = "Oficina de Tecnología (TI)",
        ["oficina-finanzas"] = "Oficina de Finanzas",
        ["oficina-contabilidad"] = "Oficina de Contabilidad",
        ["recursos-humanos"] = "Recursos Humanos",
        ["archivo-clinico"] = "Archivo Clínico",
        ["facturacion"] = "Facturación",
        ["compras"] = "Compras",
        ["auditoria"] = "Auditoría",
        ["sala-reuniones"] = "Sala de Reuniones",

        // =========================
        // INFRAESTRUCTURA / TI
        // =========================
        ["cuarto-servidores"] = "Cuarto de Servidores",
        ["data-center"] = "Centro de Datos",
        ["rack-servidores"] = "Rack de Servidores",
        ["cuarto-redes"] = "Cuarto de Redes",
        ["cuarto-telecom"] = "Cuarto de Telecomunicaciones",
        ["centro-monitoreo-ti"] = "Centro de Monitoreo de TI",

        // =========================
        // PASILLOS Y CIRCULACIÓN
        // =========================
        ["pasillo"] = "Pasillo",
        ["pasillo-principal"] = "Pasillo Principal",
        ["pasillo-secundario"] = "Pasillo Secundario",
        ["corredor-servicio"] = "Corredor de Servicio",
        ["escaleras"] = "Escaleras",
        ["ascensor"] = "Ascensor",
        ["rampa"] = "Rampa de Acceso",
        ["salida-emergencia"] = "Salida de Emergencia",

        // =========================
        // LOGÍSTICA, APOYO Y SERVICIOS
        // =========================
        ["almacen"] = "Almacén",
        ["almacen-medicamentos"] = "Almacén de Medicamentos",
        ["almacen-insumos"] = "Almacén de Insumos",
        ["lavanderia"] = "Lavandería",
        ["cocina"] = "Cocina",
        ["comedor"] = "Comedor",
        ["mantenimiento"] = "Mantenimiento",
        ["cuarto-limpieza"] = "Cuarto de Limpieza",
        ["residuos-biologicos"] = "Área de Residuos Biológicos",

        // =========================
        // ACCESO Y EXTERIORES
        // =========================
        ["recepcion"] = "Recepción",
        ["admisiones"] = "Admisiones",
        ["sala-espera"] = "Sala de Espera",
        ["entrada-principal"] = "Entrada Principal",
        ["entrada-servicio"] = "Entrada de Servicio",
        ["parqueo"] = "Parqueo",
        ["ambulancias"] = "Área de Ambulancias",
        ["jardin"] = "Jardín"
    };
}