namespace Tillegal.Api.Contracts.AiEvaluations;

public sealed record CreateAiEvaluationApiRequest(
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
    string? EvaluatorRole,
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record AiEvaluationSummaryApiResponse(
    int TotalEvaluations,
    int HelpfulCount,
    int UnhelpfulCount,
    double AverageRating,
    int LawyerEvaluationsCount,
    int UserEvaluationsCount,
    IReadOnlyDictionary<int, int> RatingDistribution);

public sealed record PagedAiEvaluationsApiResponse(
    IReadOnlyList<AiEvaluationApiResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

