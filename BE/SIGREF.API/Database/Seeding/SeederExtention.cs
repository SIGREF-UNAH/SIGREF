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