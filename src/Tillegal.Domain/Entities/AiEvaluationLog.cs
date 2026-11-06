using Tillegal.Domain.Common;
using Tillegal.Domain.Exceptions;

namespace Tillegal.Domain.Entities;

public sealed class AiEvaluationLog : BaseEntity
{
    private AiEvaluationLog() : base()
    {
    }

    public AiEvaluationLog(
        Guid id,
        Guid messageId,
        Guid? evaluatorUserId,
        bool? isHelpful,
        int? rating,
        string? feedbackComment,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? deletedAt,
        string? evaluatorRole = null)
        : base(id, createdAt, updatedAt, deletedAt)
    {
        MessageId = messageId;
        EvaluatorUserId = evaluatorUserId;
        IsHelpful = isHelpful;
        Rating = rating;
        FeedbackComment = feedbackComment;
        EvaluatorRole = evaluatorRole;
    }

    public static AiEvaluationLog Create(
        Guid messageId,
        Guid? evaluatorUserId,
        bool? isHelpful,
        int? rating,
        string? feedbackComment = null,
        string? evaluatorRole = null)
    {
        if (messageId == Guid.Empty)
        {
            throw new DomainException("MessageId cannot be empty.");
        }

        ValidateRating(rating);
        ValidateHasAtLeastOneAttribute(isHelpful, rating, feedbackComment);

        return new AiEvaluationLog
        {
            MessageId = messageId,
            EvaluatorUserId = evaluatorUserId,
            IsHelpful = isHelpful,
            Rating = rating,
            FeedbackComment = string.IsNullOrWhiteSpace(feedbackComment) ? null : feedbackComment.Trim(),
            EvaluatorRole = evaluatorRole
        };
    }

    public Guid MessageId { get; private set; }

    public Guid? EvaluatorUserId { get; private set; }

    public bool? IsHelpful { get; private set; }

    public int? Rating { get; private set; }

    public string? FeedbackComment { get; private set; }

    public string? EvaluatorRole { get; private set; }

    public void Update(
        bool? isHelpful,
        int? rating,
        string? feedbackComment)
    {
        ValidateRating(rating);
        ValidateHasAtLeastOneAttribute(isHelpful, rating, feedbackComment);

        IsHelpful = isHelpful;
        Rating = rating;
        FeedbackComment = string.IsNullOrWhiteSpace(feedbackComment) ? null : feedbackComment.Trim();

        MarkUpdated();
    }

    private static void ValidateRating(int? rating)
    {
        if (rating.HasValue && rating is < 1 or > 5)
        {
            throw new DomainException("Rating must be between 1 and 5.");
        }
    }

    private static void ValidateHasAtLeastOneAttribute(bool? isHelpful, int? rating, string? feedbackComment)
    {
        if (!isHelpful.HasValue && !rating.HasValue && string.IsNullOrWhiteSpace(feedbackComment))
        {
            throw new DomainException("At least one evaluation attribute (isHelpful, rating, or feedbackComment) must be provided.");
        }
    }
}

