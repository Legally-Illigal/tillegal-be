namespace Tillegal.Application.Common.Models;

public sealed class AuditLogEntry
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string EventType { get; init; }

    public Guid? ActorUserId { get; init; }

    public string? ActorRole { get; init; }

    public required string IpAddress { get; init; }

    public string? TargetType { get; init; }

    public Guid? TargetUserId { get; init; }

    public required string Action { get; init; }

    public string? OldValuesJson { get; init; }

    public string? NewValuesJson { get; init; }

    public string Status { get; init; } = "SUCCESS";

    public string? Description { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
