using SIGREF.API.Constants;
using SIGREF.API.Services.Common;

namespace SIGREF.API.Database.Seeding;

 
// =============================================================================
// CATÁLOGOS BASE — Hospital de Occidente HN
// =============================================================================
// Para agregar un nuevo catálogo:
//   1. Hereda de TerminologySeederBase (+ IUpdatableTerminologySeeder si puede crecer).
//   2. Regístrala en SeederServiceCollectionExtensions con AddTerminologySeeder<T>().
//   El orquestador SIGREFSeeder la ejecutará automáticamente — no hay que tocarlo.
// =============================================================================
 
/// <summary>
/// Catálogo base: Roles Administrativos.
/// Datos fijos — no se esperan cambios frecuentes.
/// </summary>
public class RolesAdminSeeder : TerminologySeederBase
{
    public override string CatalogName => "RolesAdmin";
 
    public RolesAdminSeeder(FhirService fhirService, ILogger<RolesAdminSeeder> logger)
        : base(fhirService, logger) { }
 
    public override Task SeedAsync(CancellationToken cancellationToken = default)
        => SeedTerminologyAsync(BuildDefinition(), cancellationToken);
 
    private static TerminologyDefinition BuildDefinition() => new()
    {
        CodeSystemUrl   = TerminologyConstants.RolesAdminCodeSystemUrl,
        CodeSystemName  = "RolesAdministrativos_HO",
        CodeSystemTitle = "Roles Administrativos - Hospital de Occidente HN",
        Concepts        = TerminologyConstants.RolesAdminConcepts,
        ValueSetUrl     = TerminologyConstants.RolesAdminValueSetUrl,
        ValueSetName    = "ValueSetRolesAdminHN",
        ValueSetTitle   = "Roles Administrativos Permitidos"
    };
}
 
/// <summary>
/// Catálogo base: Tipos de Ubicación.
/// Implementa <see cref="IUpdatableTerminologySeeder"/> porque puede
/// recibir nuevas ubicaciones sin necesidad de recrear el catálogo.
/// </summary>
public class TiposUbicacionSeeder : TerminologySeederBase
{
    public override string CatalogName => "TiposUbicacion";
 
    public TiposUbicacionSeeder(FhirService fhirService, ILogger<TiposUbicacionSeeder> logger)
        : base(fhirService, logger) { }
 
    public override Task SeedAsync(CancellationToken cancellationToken = default)
        => SeedTerminologyAsync(BuildDefinition(), cancellationToken);
 
    /// <summary>
    /// Agrega al catálogo existente los tipos de ubicación que aún no estén registrados.
    /// No elimina ni modifica los existentes.
    /// </summary>
    //public Task UpdateAsync(CancellationToken cancellationToken = default)
    //    => UpdateTerminologyAsync(BuildDefinition(), cancellationToken);
 
    private static TerminologyDefinition BuildDefinition() => new()
    {
        CodeSystemUrl   = TerminologyConstants.TiposUbicacionCodeSystemUrl,
        CodeSystemName  = "TiposUbicacion_HO",
        CodeSystemTitle = "Tipos de Ubicación - Hospital de Occidente HN",
        Concepts        = TerminologyConstants.TiposUbicacionConcepts,
        ValueSetUrl     = TerminologyConstants.TiposUbicacionValueSetUrl,
        ValueSetName    = "ValueSetTiposUbicacionHN",
        ValueSetTitle   = "Tipos de Ubicación Permitidos"
    };
}
 
// =============================================================================
// EJEMPLO: catálogo nuevo — datos fijos
// =============================================================================
//
// public class EspecialidadesMedicasSeeder : TerminologySeederBase
// {
//     public override string CatalogName => "EspecialidadesMedicas";
//
//     public EspecialidadesMedicasSeeder(FhirService fhirService, ILogger<EspecialidadesMedicasSeeder> logger)
//         : base(fhirService, logger) { }
//
//     public override Task SeedAsync(CancellationToken cancellationToken = default)
//         => SeedTerminologyAsync(new TerminologyDefinition
//         {
//             CodeSystemUrl   = TerminologyConstants.EspecialidadesCodeSystemUrl,
//             CodeSystemName  = "EspecialidadesMedicas_HO",
//             CodeSystemTitle = "Especialidades Médicas - Hospital de Occidente HN",
//             Concepts        = TerminologyConstants.EspecialidadesConcepts,
//             ValueSetUrl     = TerminologyConstants.EspecialidadesValueSetUrl,
//             ValueSetName    = "ValueSetEspecialidadesHN",
//             ValueSetTitle   = "Especialidades Médicas Permitidas"
//         }, cancellationToken);
// }
//
// → Registrar en SeederServiceCollectionExtensions:
//   services.AddTerminologySeeder<EspecialidadesMedicasSeeder>();
// =============================================================================