using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationById;

public sealed record GetEvaluationByIdQuery(Guid Id) : IRequest<AiEvaluationDto>;

