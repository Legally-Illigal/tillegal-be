namespace Tillegal.Application.Features.AiEvaluations.Models;

public sealed record PagedAiEvaluationsDto(
    IReadOnlyList<AiEvaluationDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

