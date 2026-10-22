using System.Text.Json;

namespace Tillegal.Persistence.DataModels;

public sealed class AuditLogDataModel : BaseDataModel
{
    public string EventType { get; set; } = null!;

    public Guid? ActorUserId { get; set; }

    public string? ActorRole { get; set; }

    public string IpAddress { get; set; } = null!;

    public string? TargetType { get; set; }

    public Guid? TargetUserId { get; set; }

    public string Action { get; set; } = null!;

    public JsonDocument? OldValues { get; set; }

    public JsonDocument? NewValues { get; set; }

    public string Status { get; set; } = "SUCCESS";

    public string? Description { get; set; }

    // Navigation properties
    public UserDataModel? ActorUser { get; set; }

    public UserDataModel? TargetUser { get; set; }
}
