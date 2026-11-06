namespace Tillegal.Application.Features.AiEvaluations.Models;

public sealed record AiEvaluationDto(
    Guid Id,
    Guid MessageId,
    Guid? EvaluatorUserId,
    string? EvaluatorRole,
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

