namespace Tillegal.Domain.Models;

public sealed record AiEvaluationSummary(
    int TotalEvaluations,
    int HelpfulCount,
    int UnhelpfulCount,
    double AverageRating,
    int LawyerEvaluationsCount,
    int UserEvaluationsCount,
    IReadOnlyDictionary<int, int> RatingDistribution);

