using System.Text.Json;
using Tillegal.Domain.Common;

namespace Tillegal.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public required string EventType { get; set; }

    public Guid? ActorUserId { get; set; }

    public string? ActorRole { get; set; }

    public required string IpAddress { get; set; }

    public string? TargetType { get; set; }

    public Guid? TargetUserId { get; set; }

    public required string Action { get; set; }

    public JsonDocument? OldValues { get; set; }

    public JsonDocument? NewValues { get; set; }

    public string Status { get; set; } = "SUCCESS";

    public string? Description { get; set; }
}
