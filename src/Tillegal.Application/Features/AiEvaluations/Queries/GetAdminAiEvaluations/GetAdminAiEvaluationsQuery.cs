using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluations;

public sealed record GetAdminAiEvaluationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsHelpful = null,
    int? Rating = null,
    Guid? EvaluatorUserId = null,
    string? EvaluatorRole = null) : IRequest<PagedAiEvaluationsDto>;

