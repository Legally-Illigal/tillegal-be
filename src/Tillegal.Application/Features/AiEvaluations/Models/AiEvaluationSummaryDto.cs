namespace Tillegal.Application.Features.AiEvaluations.Models;

public sealed record AiEvaluationSummaryDto(
    int TotalEvaluations,
    double AverageRating,
    int HelpfulCount,
    int UnhelpfulCount);

