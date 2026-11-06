using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Commands.CreateMessageEvaluation;

public sealed record CreateMessageEvaluationCommand(
    Guid ConversationId,
    Guid MessageId,
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment) : IRequest<AiEvaluationDto>;

