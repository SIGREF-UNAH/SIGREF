using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Database.Seeding;
using SIGREF.API.Services;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// Seeder encargado de crear los CodeSystems y ValueSets
/// relacionados con los Tipos de Ubicación.
/// </summary>
public class TiposUbicacionSeeder : TerminologySeederBase
{
    public TiposUbicacionSeeder(FhirService fhirService, ILogger<TiposUbicacionSeeder> logger)
        : base(fhirService, logger) { }

    public Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var def = new TerminologyDefinition
        {
            CodeSystemUrl = TerminologyConstants.TiposUbicacionCodeSystemUrl,
            CodeSystemName = "TiposUbicacion_HO",
            CodeSystemTitle = "Tipos de Ubicación - Hospital de Occidente HN",
            Concepts = TerminologyConstants.TiposUbicacionConcepts,
            ValueSetUrl = TerminologyConstants.TiposUbicacionValueSetUrl,
            ValueSetName = "ValueSetTiposUbicacionHN",
            ValueSetTitle = "Tipos de Ubicación Permitidos"
        };

        return SeedTerminologyAsync(def, cancellationToken);
    }
}

/// <summary>
/// Seeder encargado de inicializar el CodeSystem y ValueSet de Roles Administrativos.
/// </summary>
public class RolesAdminSeeder : TerminologySeederBase
{
    public RolesAdminSeeder(FhirService fhirService, ILogger<RolesAdminSeeder> logger)
        : base(fhirService, logger) { }

    public Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var def = new TerminologyDefinition
        {
            CodeSystemUrl = TerminologyConstants.RolesAdminCodeSystemUrl,
            CodeSystemName = "RolesAdministrativos_HO",
            CodeSystemTitle = "Roles Administrativos - Hospital de Occidente HN",
            Concepts = TerminologyConstants.RolesAdminConcepts,
            ValueSetUrl = TerminologyConstants.RolesAdminValueSetUrl,
            ValueSetName = "ValueSetRolesAdminHN",
            ValueSetTitle = "Roles Administrativos Permitidos"
        };

        return SeedTerminologyAsync(def, cancellationToken);
    }
}