using Tillegal.Application.Common.Models;

namespace Tillegal.Application.Common.Interfaces;

public interface IAuditLogService
{
    ValueTask EnqueueAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
