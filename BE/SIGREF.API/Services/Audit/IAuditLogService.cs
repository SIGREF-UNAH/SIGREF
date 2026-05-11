using SIGREF.API.Audit.Dto;
using SIGREF.API.Dtos.Audit;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Audit;

public interface IAuditLogService
{
    Task<PagedResultDto<AuditLog>> GetAuditLogsAsync(AuditLogFilterDto filter);
    Task<AuditLog?> GetAuditLogByIdAsync(string id);
}