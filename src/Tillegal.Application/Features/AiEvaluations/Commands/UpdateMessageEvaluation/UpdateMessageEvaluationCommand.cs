using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Commands.UpdateMessageEvaluation;

public sealed record UpdateMessageEvaluationCommand(
    Guid ConversationId,
    Guid MessageId,
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment) : IRequest<AiEvaluationDto>;

