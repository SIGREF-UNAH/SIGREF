
using SIGREF.API.Database.Seeding;
using SIGREF.API.Utils;

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
            $"{AnsiColors.Blue}[FHIR-SEED] ==================================================={AnsiColors.Reset}");
        _logger.LogInformation(
            $"{AnsiColors.Blue}   SIGREF - FHIR Inicializacion de Catalogos{AnsiColors.Reset}");
        _logger.LogInformation(
            $"{AnsiColors.Blue}>> Starting...{AnsiColors.Reset}");
        _logger.LogInformation(
            $"{AnsiColors.Blue}[FHIR-SEED] ==================================================={AnsiColors.Reset}");

        var failed = new List<string>();
        var success = 0;
        foreach (var seeder in all)
        {
            ct.ThrowIfCancellationRequested();
            _logger.LogInformation(
                "{Line}",
                $"{AnsiColors.Cyan}   [{seeder.CatalogName}] Procesando...{AnsiColors.Reset}");
            try
            {
                await seeder.SeedAsync(ct);
                

                if (seeder is IUpdatableTerminologySeeder updatable)
                {
                    _logger.LogInformation(
                        "{Line}",
                    $"{AnsiColors.Cyan}  >> [{seeder.CatalogName}] Actualizacion de Catalogo Detectado... Sincronizando...{AnsiColors.Reset}");
                    await updatable.UpdateAsync(ct);
                }
                success++;
                _logger.LogInformation(
                    "{Line}",
                    $"{AnsiColors.Green}  OK [{seeder.CatalogName}] Exito.{AnsiColors.Reset}");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                failed.Add(seeder.CatalogName);
                _logger.LogError(ex,
                    "{Line}",
                    $"{AnsiColors.Red}  XX [{seeder.CatalogName}] Fallo: {ex.Message}{AnsiColors.Reset}");
            }
        }

        _logger.LogInformation("{Separator}", $"{AnsiColors.Blue}====================================================={AnsiColors.Reset}");

        if (failed.Count > 0)
        {
            _logger.LogWarning(
                "{Summary}\n{Failed}",
                $"{AnsiColors.Yellow}  !! Inicializacion Completada con Errores.{AnsiColors.Reset}",
                $"{AnsiColors.Yellow}  !! Catalogos que Fallaron al iniciar ({failed.Count}): {string.Join(", ", failed)}{AnsiColors.Reset}");
        }
        else
        {
            _logger.LogInformation(
                "{Summary}",
                $"{AnsiColors.Green}   Todos los Catalogos fueron inicializados Correctamente ({success}/{all.Count}){AnsiColors.Reset}");
        }

        _logger.LogInformation("{Footer}", $"{AnsiColors.Blue}============================================={AnsiColors.Reset}");
    }
}