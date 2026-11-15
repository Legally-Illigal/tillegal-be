using System.Text.Json;

namespace Tillegal.Persistence.DataModels;

public sealed class ConversationMessageDataModel : BaseDataModel
{
    public Guid ConversationId { get; set; }

    public Guid? ParentMessageId { get; set; }

    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;

    public JsonDocument? Citations { get; set; }

    // Navigation properties
    public ConversationDataModel Conversation { get; set; } = null!;

    public ConversationMessageDataModel? ParentMessage { get; set; }

    public ICollection<ConversationMessageDataModel> ChildMessages { get; set; } = new List<ConversationMessageDataModel>();

    public ICollection<AiEvaluationLogDataModel> AiEvaluationLogs { get; set; } = new List<AiEvaluationLogDataModel>();
}
