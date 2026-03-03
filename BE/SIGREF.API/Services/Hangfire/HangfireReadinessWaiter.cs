using Hangfire.PostgreSql;
using Npgsql;

namespace SIGREF.API.Services.Hangfire;

/// <summary>
/// BackgroundService que espera hasta que la base de datos de Hangfire sea accesible y
/// el esquema requerido (tablas) exista. Si falta el esquema, activará su creación
/// instanciando un <see cref="PostgreSqlStorage"/> con 
/// <c>PrepareSchemaIfNecessary = true</c>.
///
/// Esto es un reflejo de <see cref="Services.FhirUtils.HikariReadinessWaiter" /> 
/// utilizado para HAPI.
/// </summary>
public class HangfireReadinessWaiter : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HangfireReadinessWaiter> _logger;

    // ANSI COLORS (ASCII only)
    private const string RESET  = "\u001b[0m";
    private const string GREEN  = "\u001b[32m";
    private const string YELLOW = "\u001b[33m";
    private const string BLUE   = "\u001b[34m";
    private const string RED    = "\u001b[31m";
    private const string CYAN   = "\u001b[36m";

    // SQL que verifica si la tabla Job de Hangfire existe en public schema.
    // EXISTS devuelve un boolean que Npgsql puede leer correctamente.
    private const string CheckTableSql = 
        "SELECT EXISTS(SELECT 1 FROM information_schema.tables " +
        "WHERE table_schema='public' AND table_name='Job')";

    public HangfireReadinessWaiter(
        IConfiguration configuration,
        ILogger<HangfireReadinessWaiter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{BLUE}[HANGFIRE-READY] Esperando a que Hangfire se inicialice completamente...{RESET}");

        var connStr = _configuration.GetConnectionString("hangfire");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            _logger.LogWarning($"{YELLOW}[HANGFIRE-READY] No hay cadena de conexión 'hangfire' definida; obviando comprobación.{RESET}");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(stoppingToken);

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = CheckTableSql;
                var tableExists = (bool?)await cmd.ExecuteScalarAsync(stoppingToken) ?? false;

                if (tableExists)
                {
                    _logger.LogInformation($"{GREEN}[HANGFIRE-READY] tablas detectadas, Hangfire listo.{RESET}");
                    break;
                }

                _logger.LogInformation($"{CYAN}[HANGFIRE-READY] Esquema no encontrado; generando tablas...{RESET}");

                // crear esquema usando las opciones configuradas en el arranque de la
                // aplicación. Suele ser el mismo `PrepareSchemaIfNecessary = true` que
                // ya proporcionamos en Startup.
                var options = new PostgreSqlStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                };

                // la construcción de un PostgreSqlStorage y la llamada a un método de
                // monitorización realiza en segundo plano la creación de las tablas.
                var storage = new PostgreSqlStorage(conn, options);
                // el método Servers() realiza una consulta simple y dispara la
                // inicialización de esquema si es necesario.
                storage.GetMonitoringApi().Servers();

                _logger.LogInformation($"{GREEN}[HANGFIRE-READY] Esquema creado, Hangfire listo.{RESET}");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"{YELLOW}[HANGFIRE-READY] Esperando a Hangfire... ({ex.Message}){RESET}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
