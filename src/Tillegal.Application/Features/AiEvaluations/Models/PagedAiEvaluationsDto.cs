namespace Tillegal.Application.Features.AiEvaluations.Models;

public sealed record PagedAiEvaluationsDto(
    IReadOnlyList<AiEvaluationDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

