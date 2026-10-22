using System.Threading.Channels;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;

namespace Tillegal.Api.Services;

public sealed class AuditLogQueue : IAuditLogService
{
    private readonly Channel<AuditLogEntry> _channel = Channel.CreateBounded<AuditLogEntry>(new BoundedChannelOptions(10_000)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = true,
        SingleWriter = false
    });

    public ChannelReader<AuditLogEntry> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(entry, cancellationToken);
    }
}
