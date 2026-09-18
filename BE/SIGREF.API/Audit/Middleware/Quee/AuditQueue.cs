using System.Threading.Channels;
using SIGREF.API.Audit.Dto;

namespace SIGREF.API.Audit.Middleware.Quee;

public class AuditQueue : IAuditQueue
{
    private readonly Channel<AuditLog> _channel;

    public AuditQueue(int capacity = 10000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest // Prioriza seguir operando
        };
        _channel = Channel.CreateBounded<AuditLog>(options);
    }

    public async ValueTask WriteLogAsync(AuditLog log)
    {
        await _channel.Writer.WriteAsync(log);
    }

    public IAsyncEnumerable<AuditLog> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}