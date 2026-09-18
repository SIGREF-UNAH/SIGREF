namespace SIGREF.Hangfire.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Mensaje de inicio profesional
        _logger.LogInformation("======================================================");
        _logger.LogInformation("SIGREF WORKER NODE: Iniciado y listo para Hangfire.");
        _logger.LogInformation("Host: {MachineName}", Environment.MachineName);
        _logger.LogInformation("======================================================");

        while (!stoppingToken.IsCancellationRequested)
        {
            // En lugar de cada 1 segundo, informamos cada 30 minutos 
            // que el nodo sigue saludable.
            _logger.LogInformation("SIGREF Worker Heartbeat: Nodo activo a las {time}", DateTime.UtcNow);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            // await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}