using Hl7.Fhir.Model;
using SIGREF.API.Dtos.AuditLog;

namespace SIGREF.API.Services.AuditLog
{
    public interface IAuditLogService
    {
        // Método principal que crea y envía AuditEvent a FHIR
        System.Threading.Tasks.Task LogActionAsync(string userId, string username, string action, string actionType,
            string? endpoint = null, string? httpMethod = null, int? statusCode = null,
            string? errorMessage = null, string? requestBody = null, string? responseBody = null,
            string? ipAddress = null, string? userAgent = null, string? additionalData = null);

        // Método para crear un AuditEvent FHIR completo
        System.Threading.Tasks.Task<AuditEvent> CreateFhirAuditEventAsync(string userId, string username, string action,
            string actionType, string? endpoint = null, string? httpMethod = null,
            int? statusCode = null, string? errorMessage = null, string? requestBody = null,
            string? responseBody = null, string? ipAddress = null, string? userAgent = null,
            string? additionalData = null);

        // Método para enviar AuditEvent al servidor FHIR
        System.Threading.Tasks.Task<bool> SendAuditEventToFhirAsync(AuditEvent auditEvent);

        // Métodos de consulta (desde MongoDB como respaldo)
        System.Threading.Tasks.Task<List<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter);
        System.Threading.Tasks.Task<AuditLogDto?> GetLogByIdAsync(int id);
    }
}