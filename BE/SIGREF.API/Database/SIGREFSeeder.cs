
using SIGREF.API.Database.Seeding;
using SIGREF.API.Services.FhirUtils;

namespace  SIGREF.API.Database;
/// <summary>
/// Orquestador de inicialización de catálogos base de terminología FHIR.
/// Ejecuta automáticamente todos los <see cref="ITerminologySeeder"/> registrados en DI.
/// Para agregar un nuevo catálogo, solo registra su seeder — este archivo no se toca.
/// </summary>
public class SIGREFSeeder
{
    private readonly IEnumerable<ITerminologySeeder> _seeders;
    private readonly ILogger<SIGREFSeeder> _logger;

    public SIGREFSeeder(
        IEnumerable<ITerminologySeeder> seeders,
        ILogger<SIGREFSeeder> logger)
    {
        _seeders = seeders;
        _logger  = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var all = _seeders.ToList();
        
        
        _logger.LogInformation(
            $"{FhirAnsiColors.Blue}[FHIR-SEED] ==================================================={FhirAnsiColors.Reset}");
        _logger.LogInformation(
            $"{FhirAnsiColors.Blue}   SIGREF - FHIR Inicializacion de Catalogos{FhirAnsiColors.Reset}");
        _logger.LogInformation(
            $"{FhirAnsiColors.Blue}>> Starting...{FhirAnsiColors.Reset}");
        _logger.LogInformation(
            $"{FhirAnsiColors.Blue}[FHIR-SEED] ==================================================={FhirAnsiColors.Reset}");

        var failed = new List<string>();
        var success = 0;
        foreach (var seeder in all)
        {
            ct.ThrowIfCancellationRequested();
            _logger.LogInformation(
                "{Line}",
                $"{FhirAnsiColors.Cyan}   [{seeder.CatalogName}] Procesando...{FhirAnsiColors.Reset}");
            try
            {
                await seeder.SeedAsync(ct);
                

                if (seeder is IUpdatableTerminologySeeder updatable)
                {
                    _logger.LogInformation(
                        "{Line}",
                    $"{FhirAnsiColors.Cyan}  >> [{seeder.CatalogName}] Actualizacion de Catalogo Detectado... Sincronizando...{FhirAnsiColors.Reset}");
                    await updatable.UpdateAsync(ct);
                }
                success++;
                _logger.LogInformation(
                    "{Line}",
                    $"{FhirAnsiColors.Green}  OK [{seeder.CatalogName}] Exito.{FhirAnsiColors.Reset}");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                failed.Add(seeder.CatalogName);
                _logger.LogError(ex,
                    "{Line}",
                    $"{FhirAnsiColors.Red}  XX [{seeder.CatalogName}] Fallo: {ex.Message}{FhirAnsiColors.Reset}");
            }
        }

        _logger.LogInformation("{Separator}", $"{FhirAnsiColors.Blue}====================================================={FhirAnsiColors.Reset}");

        if (failed.Count > 0)
        {
            _logger.LogWarning(
                "{Summary}\n{Failed}",
                $"{FhirAnsiColors.Yellow}  !! Inicializacion Completada con Errores.{FhirAnsiColors.Reset}",
                $"{FhirAnsiColors.Yellow}  !! Catalogos que Fallaron al iniciar ({failed.Count}): {string.Join(", ", failed)}{FhirAnsiColors.Reset}");
        }
        else
        {
            _logger.LogInformation(
                "{Summary}",
                $"{FhirAnsiColors.Green}   Todos los Catalogos fueron inicializados Correctamente ({success}/{all.Count}){FhirAnsiColors.Reset}");
        }

        _logger.LogInformation("{Footer}", $"{FhirAnsiColors.Blue}============================================={FhirAnsiColors.Reset}");
    }
}