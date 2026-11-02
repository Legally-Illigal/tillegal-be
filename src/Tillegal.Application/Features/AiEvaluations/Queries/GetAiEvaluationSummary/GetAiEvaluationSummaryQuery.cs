using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluationSummary;

public sealed record GetAiEvaluationSummaryQuery : IRequest<AiEvaluationSummaryDto>;

