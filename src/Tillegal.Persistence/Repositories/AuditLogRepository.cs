using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Entities;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Repositories;

public sealed class AuditLogRepository(AppDbContext dbContext) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        var model = new AuditLogDataModel
        {
            Id = auditLog.Id,
            EventType = auditLog.EventType,
            ActorUserId = auditLog.ActorUserId,
            ActorRole = auditLog.ActorRole,
            IpAddress = auditLog.IpAddress,
            TargetType = auditLog.TargetType,
            TargetUserId = auditLog.TargetUserId,
            Action = auditLog.Action,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues,
            Status = auditLog.Status,
            Description = auditLog.Description,
            CreatedAt = auditLog.CreatedAt,
            UpdatedAt = auditLog.UpdatedAt,
            DeletedAt = auditLog.DeletedAt
        };

        await dbContext.AuditLogs.AddAsync(model, cancellationToken);
    }
}
