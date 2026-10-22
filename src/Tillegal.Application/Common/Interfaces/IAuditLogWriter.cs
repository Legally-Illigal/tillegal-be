using Tillegal.Application.Common.Models;

namespace Tillegal.Application.Common.Interfaces;

public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
