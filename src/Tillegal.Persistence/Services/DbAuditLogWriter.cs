using System.Text.Json;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Services;

public sealed class DbAuditLogWriter(AppDbContext dbContext) : IAuditLogWriter
{
    public async Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var dataModel = new AuditLogDataModel
        {
            Id = entry.Id,
            CreatedAt = entry.CreatedAt,
            EventType = entry.EventType,
            ActorUserId = entry.ActorUserId,
            ActorRole = entry.ActorRole,
            IpAddress = entry.IpAddress,
            TargetType = entry.TargetType,
            TargetUserId = entry.TargetUserId,
            Action = entry.Action,
            OldValues = entry.OldValuesJson is not null ? JsonDocument.Parse(entry.OldValuesJson) : null,
            NewValues = entry.NewValuesJson is not null ? JsonDocument.Parse(entry.NewValuesJson) : null,
            Status = entry.Status,
            Description = entry.Description
        };

        dbContext.AuditLogs.Add(dataModel);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
