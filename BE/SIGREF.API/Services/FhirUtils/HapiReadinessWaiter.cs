
using SIGREF.API.Database;

namespace SIGREF.API.Services.FhirUtils;

public class HapiReadinessWaiter : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HapiReadinessWaiter> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private const string HapiBaseUrl = "http://hapifhir:8080/fhir";

    // ANSI COLORS (ASCII only)
    private const string RESET  = "\u001b[0m";
    private const string GREEN  = "\u001b[32m";
    private const string YELLOW = "\u001b[33m";
    private const string BLUE   = "\u001b[34m";
    private const string RED    = "\u001b[31m";
    private const string CYAN   = "\u001b[36m";

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
            $"{BLUE}[HAPI-READY] Esperando a que HAPI FHIR se inicialice completamente...{RESET}");

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
                        $"{CYAN}[HAPI-READY] Metadata correcta, verificando acceso a base de datos...{RESET}");

                    // Paso 2: JPA / DB
                    var test = await client.GetAsync(
                        $"{HapiBaseUrl}/Patient?_summary=count",
                        stoppingToken);

                    if (test.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            $"{GREEN}[HAPI-READY] HAPI FHIR listo completamente (JPA y base de datos OK){RESET}");

                        using var scope = _scopeFactory.CreateScope();
 
                        // El orquestador corre todos los initializers y reindexar una sola vez
                        var orchestrator =
                            scope.ServiceProvider.GetRequiredService<FhirSearchParameterOrchestrator>();
 
                        await orchestrator.EnsureAllAsync();
 
                        var seeder =
                            scope.ServiceProvider.GetRequiredService<SIGREFSeeder>();
                        await seeder.SeedAsync(stoppingToken);
 
                        _logger.LogInformation(
                            $"{GREEN}[HAPI-READY] HAPI FHIR LISTO - Proceso de inicializacion terminado{RESET}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"{YELLOW}[HAPI-READY] Esperando a HAPI... ({ex.Message}){RESET}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
