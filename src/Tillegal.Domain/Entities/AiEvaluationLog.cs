using Tillegal.Domain.Common;

namespace Tillegal.Domain.Entities;

public sealed class AiEvaluationLog : BaseEntity
{
    public Guid MessageId { get; set; }

    public Guid? EvaluatorUserId { get; set; }

    public bool? IsHelpful { get; set; }

    public int? Rating { get; set; }

    public string? FeedbackComment { get; set; }
}
