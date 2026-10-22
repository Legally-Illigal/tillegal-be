namespace Tillegal.Persistence.DataModels;

public sealed class UserDataModel : BaseDataModel
{
    public Guid RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? PhoneNumber { get; set; }

    // Navigation properties
    public RoleDataModel Role { get; set; } = null!;

    public ICollection<ConversationDataModel> Conversations { get; set; } = new List<ConversationDataModel>();

    public ICollection<UserAttachmentDataModel> UserAttachments { get; set; } = new List<UserAttachmentDataModel>();

    public ICollection<AuditLogDataModel> ActorAuditLogs { get; set; } = new List<AuditLogDataModel>();

    public ICollection<AuditLogDataModel> TargetAuditLogs { get; set; } = new List<AuditLogDataModel>();

    public ICollection<AiEvaluationLogDataModel> AiEvaluationLogs { get; set; } = new List<AiEvaluationLogDataModel>();
}
