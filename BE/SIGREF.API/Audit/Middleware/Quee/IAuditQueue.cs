using SIGREF.API.Audit.Dto;

namespace SIGREF.API.Audit.Middleware.Quee;

public interface IAuditQueue
{
    ValueTask WriteLogAsync(AuditLog log);
    IAsyncEnumerable<AuditLog> ReadAllAsync(CancellationToken ct);
}