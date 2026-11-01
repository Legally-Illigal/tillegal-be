namespace Tillegal.Api.Contracts.AiEvaluations;

public sealed record CreateAiEvaluationApiRequest(
    Guid MessageId,
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment);

public sealed record UpdateAiEvaluationApiRequest(
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment);

public sealed record AiEvaluationApiResponse(
    Guid Id,
    Guid MessageId,
    Guid? EvaluatorUserId,
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record AiEvaluationSummaryApiResponse(
    int TotalEvaluations,
    double AverageRating,
    int HelpfulCount,
    int UnhelpfulCount);

public sealed record PagedAiEvaluationsApiResponse(
    IReadOnlyList<AiEvaluationApiResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

