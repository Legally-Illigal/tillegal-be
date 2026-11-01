using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluations;

public sealed record GetAiEvaluationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsHelpful = null,
    int? Rating = null,
    Guid? EvaluatorUserId = null) : IRequest<PagedAiEvaluationsDto>;

