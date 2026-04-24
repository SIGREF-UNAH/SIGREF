
using SIGREF.API.Database;
using SIGREF.API.Utils;

namespace SIGREF.API.Services.FhirUtils;

public class HapiReadinessWaiter : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HapiReadinessWaiter> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private const string HapiBaseUrl = "http://hapifhir:8080/fhir";

    public HapiReadinessWaiter(
        IHttpClientFactory httpClientFactory,
        ILogger<HapiReadinessWaiter> logger,
        IServiceScopeFactory scopeFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            $"{AnsiColors.Blue}[HAPI-READY] Esperando a que HAPI FHIR se inicialice completamente...{AnsiColors.Reset}");

        var client = _httpClientFactory.CreateClient();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Paso 1: metadata
                var response = await client.GetAsync(
                    $"{HapiBaseUrl}/metadata",
                    stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        $"{AnsiColors.Cyan}[HAPI-READY] Metadata correcta, verificando acceso a base de datos...{AnsiColors.Reset}");

                    // Paso 2: JPA / DB
                    var test = await client.GetAsync(
                        $"{HapiBaseUrl}/Patient?_summary=count",
                        stoppingToken);

                    if (test.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            $"{AnsiColors.Green}[HAPI-READY] HAPI FHIR listo completamente (JPA y base de datos OK){AnsiColors.Reset}");

                        using var scope = _scopeFactory.CreateScope();
 
                        // El orquestador corre todos los initializers y reindexar una sola vez
                        var orchestrator =
                            scope.ServiceProvider.GetRequiredService<FhirSearchParameterOrchestrator>();
 
                        await orchestrator.EnsureAllAsync();
 
                        var seeder =
                            scope.ServiceProvider.GetRequiredService<SIGREFSeeder>();
                        await seeder.SeedAsync(stoppingToken);
 
                        _logger.LogInformation(
                            $"{AnsiColors.Green}[HAPI-READY] HAPI FHIR LISTO - Proceso de inicializacion terminado{AnsiColors.Reset}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"{AnsiColors.Yellow}[HAPI-READY] Esperando a HAPI... ({ex.Message}){AnsiColors.Reset}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
