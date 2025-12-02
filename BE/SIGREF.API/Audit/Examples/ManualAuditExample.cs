using SIGREF.API.Audit.Models;
using SIGREF.API.Audit.Services;

namespace SIGREF.API.Audit.Examples;

/// <summary>
/// Ejemplo de cómo usar el servicio de auditoría manualmente
/// si necesitas registrar algo específico que no sea capturado automáticamente
/// </summary>
public class ManualAuditExample
{
    private readonly IAuditService _auditService;

    public ManualAuditExample(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task EjemploRegistroManual()
    {
        // Crear un log personalizado
        var auditLog = new AuditLog
        {
            Action = "custom-action",
            ResourceType = "Patient",
            ResourceId = "123",
            UserId = "user-id",
            UserName = "John Doe",
            UserRoles = new List<string> { "admin" },
            Endpoint = "/api/custom",
            HttpMethod = "POST",
            StatusCode = 200,
            ClientIp = "192.168.1.1",
            Success = true,
            DataAfter = "{ \"custom\": \"data\" }",
            AdditionalInfo = new Dictionary<string, string>
            {
                { "customField", "customValue" }
            }
        };

        // Guardar el log
        await _auditService.LogAsync(auditLog);
    }

    public async Task EjemploConsultarLogs()
    {
        // Obtener un log específico por ID
        var log = await _auditService.GetLogByIdAsync("507f1f77bcf86cd799439011");

        // Obtener todos los logs (paginados)
        var allLogs = await _auditService.GetAllLogsAsync(page: 1, pageSize: 50);

        // Obtener logs de un recurso específico
        var logs = await _auditService.GetLogsByResourceAsync("Patient", "123");

        // Obtener logs de un usuario
        var userLogs = await _auditService.GetLogsByUserAsync("user-id", 
            from: DateTime.UtcNow.AddDays(-7), 
            to: DateTime.UtcNow);

        // Obtener logs por acción
        var createLogs = await _auditService.GetLogsByActionAsync("create", 
            from: DateTime.UtcNow.AddDays(-30));
    }
}
