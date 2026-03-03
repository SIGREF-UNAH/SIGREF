using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Services.Hangfire;

namespace SIGREF.API.Controllers.Hangfire;

/// <summary>
/// Controller de prueba para Hangfire.
/// Proporciona endpoints para enqueuer trabajos de prueba en Hangfire.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HangfireTestController : ControllerBase
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly HangfireTestService _hangfireTestService;
    private readonly ILogger<HangfireTestController> _logger;

    public HangfireTestController(
        IBackgroundJobClient backgroundJobClient,
        HangfireTestService hangfireTestService,
        ILogger<HangfireTestController> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _hangfireTestService = hangfireTestService;
        _logger = logger;
    }

    /// <summary>
    /// Encola un trabajo de prueba simple que imprimirá un mensaje en los logs.
    /// </summary>
    /// <param name="message">Mensaje a imprimir (opcional)</param>
    /// <returns>ID del trabajo encolado</returns>
    [HttpPost("echo")]
    public IActionResult EnqueueEchoJob([FromQuery] string? message = null)
    {
        var finalMessage = message ?? "Hola desde Hangfire!";
        
        try
        {
            var jobId = _backgroundJobClient.Enqueue(
                () => _hangfireTestService.PrintMessage(finalMessage));
            
            _logger.LogInformation("[HANGFIRE-TEST] Job encolado con ID: {JobId}, Mensaje: {Message}", jobId, finalMessage);
            
            return Ok(new 
            { 
                success = true, 
                jobId = jobId,
                message = finalMessage 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HANGFIRE-TEST] Error al enqueuer job");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Encola un trabajo de prueba con retraso que simula una tarea más pesada.
    /// </summary>
    /// <param name="message">Mensaje a imprimir (opcional)</param>
    /// <param name="delaySeconds">Segundos de retraso (opcional, por defecto 5)</param>
    /// <returns>ID del trabajo encolado</returns>
    [HttpPost("echo-delayed")]
    public IActionResult EnqueueDelayedEchoJob([FromQuery] string? message = null, [FromQuery] int delaySeconds = 5)
    {
        var finalMessage = message ?? "Hola desde Hangfire con retraso!";
        
        try
        {
            var jobId = _backgroundJobClient.Enqueue(
                () => _hangfireTestService.PrintMessageWithDelay(finalMessage, delaySeconds));
            
            _logger.LogInformation("[HANGFIRE-TEST] Job con retraso encolado con ID: {JobId}, Mensaje: {Message}, Retraso: {Seconds}s", 
                jobId, finalMessage, delaySeconds);
            
            return Ok(new 
            { 
                success = true, 
                jobId = jobId,
                message = finalMessage,
                delaySeconds = delaySeconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HANGFIRE-TEST] Error al enqueuer job con retraso");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint de health check para verificar que Hangfire está funcionando.
    /// </summary>
    /// <returns>Estado de Hangfire</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("[HANGFIRE-TEST] Health check solicitado");
        return Ok(new { status = "ok", message = "Hangfire está funcionando" });
    }
}
