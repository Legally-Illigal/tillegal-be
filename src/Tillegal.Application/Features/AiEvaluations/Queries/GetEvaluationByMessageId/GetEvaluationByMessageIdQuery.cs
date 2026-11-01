using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationByMessageId;

public sealed record GetEvaluationByMessageIdQuery(Guid MessageId) : IRequest<AiEvaluationDto>;

