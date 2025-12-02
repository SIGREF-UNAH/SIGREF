using SIGREF.API.Audit.Models;

namespace SIGREF.API.Audit.Services;

public interface IAuditService
{
    Task LogAsync(AuditLog auditLog);
    Task<AuditLog> GetLogByIdAsync(string id);
    Task<List<AuditLog>> GetAllLogsAsync(int page = 1, int pageSize = 50);
    Task<List<AuditLog>> GetLogsByResourceAsync(string resourceType, string resourceId);
    Task<List<AuditLog>> GetLogsByUserAsync(string userId, DateTime? from = null, DateTime? to = null);
    Task<List<AuditLog>> GetLogsByActionAsync(string action, DateTime? from = null, DateTime? to = null);
    Task<List<AuditLog>> GetLogsByStatusCodeAsync(int statusCode, DateTime? from = null, DateTime? to = null);
    Task LogLoginAsync(string userId, string userName, List<string> roles, string clientIp, bool success, string errorMessage = null);
    Task ClearAllLogsAsync();
}
