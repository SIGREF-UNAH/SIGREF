namespace SIGREF.API.Services.Hangfire;

/// <summary>
/// Servicio de prueba para Hangfire.
/// Proporciona métodos que pueden ser encolados como trabajos en segundo plano.
/// </summary>
public class HangfireTestService
{
    private readonly ILogger<HangfireTestService> _logger;

    public HangfireTestService(ILogger<HangfireTestService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Método de prueba que imprime un mensaje en los logs.
    /// Este método está diseñado para ser encolado como un trabajo de Hangfire.
    /// </summary>
    /// <param name="message">Mensaje a imprimir</param>
    public void PrintMessage(string message)
    {
        _logger.LogInformation("[HANGFIRE-TEST] Mensaje recibido: {Message}", message);
    }

    /// <summary>
    /// Método de prueba que simula un trabajo más pesado con retrasos.
    /// </summary>
    /// <param name="message">Mensaje inicial</param>
    /// <param name="delaySeconds">Segundos de retraso antes de completar</param>
    public async Task PrintMessageWithDelay(string message, int delaySeconds = 5)
    {
        _logger.LogInformation("[HANGFIRE-TEST] Iniciando trabajo con mensaje: {Message}", message);
        
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        
        _logger.LogInformation("[HANGFIRE-TEST] Trabajo completado después de {Seconds}s: {Message}", delaySeconds, message);
    }
}
