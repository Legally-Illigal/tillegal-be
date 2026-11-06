using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationSummary;

public sealed record GetAdminAiEvaluationSummaryQuery : IRequest<AiEvaluationSummaryDto>;

