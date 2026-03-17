using SIGREF.API.Audit.Models;
using SIGREF.API.Dtos.Audit;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Audit.Services;

public interface IAuditService
{
    Task LogAsync(AuditLog auditLog);
    Task<AuditLog> GetLogByIdAsync(string id);
    Task<ResponseDto<PagedResultDto<AuditLogDto>>> GetAuditLogsAsync(AuditLogQueryDto query);
    Task LogLoginAsync(string userId, string userName, List<string> roles, string clientIp, bool success, string errorMessage = null);
    Task ClearAllLogsAsync();
}
