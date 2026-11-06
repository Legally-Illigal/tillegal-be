namespace Tillegal.Application.Features.AiEvaluations.Models;

public sealed record AiEvaluationSummaryDto(
    int TotalEvaluations,
    int HelpfulCount,
    int UnhelpfulCount,
    double AverageRating,
    int LawyerEvaluationsCount,
    int UserEvaluationsCount,
    IReadOnlyDictionary<int, int> RatingDistribution);

