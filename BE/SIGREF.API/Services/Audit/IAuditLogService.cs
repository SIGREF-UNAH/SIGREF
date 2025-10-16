using SIGREF.API.Dtos.AuditLog;

namespace SIGREF.API.Services.AuditLog
{
    public interface IAuditLogService
    {
        Task LogActionAsync(string userId, string username, string action, string actionType, 
            string? endpoint = null, string? httpMethod = null, int? statusCode = null, 
            string? errorMessage = null, string? requestBody = null, string? responseBody = null,
            string? ipAddress = null, string? userAgent = null, string? additionalData = null);

        Task<List<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter);
        Task<AuditLogDto?> GetLogByIdAsync(int id);
    }
}