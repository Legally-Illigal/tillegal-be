using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetMessageEvaluation;

public sealed record GetMessageEvaluationQuery(
    Guid ConversationId,
    Guid MessageId) : IRequest<AiEvaluationDto>;

