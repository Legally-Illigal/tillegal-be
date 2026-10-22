namespace Tillegal.Persistence.DataModels;

public sealed class AiEvaluationLogDataModel : BaseDataModel
{
    public Guid MessageId { get; set; }

    public Guid? EvaluatorUserId { get; set; }

    public bool? IsHelpful { get; set; }

    public int? Rating { get; set; }

    public string? FeedbackComment { get; set; }

    // Navigation properties
    public ConversationMessageDataModel Message { get; set; } = null!;

    public UserDataModel? EvaluatorUser { get; set; }
}
