using MediatR;

namespace Tillegal.Application.Features.AiEvaluations.Commands.DeleteMessageEvaluation;

public sealed record DeleteMessageEvaluationCommand(
    Guid ConversationId,
    Guid MessageId) : IRequest;

