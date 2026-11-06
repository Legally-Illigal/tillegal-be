using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationById;

public sealed record GetAdminAiEvaluationByIdQuery(Guid Id) : IRequest<AiEvaluationDto>;

