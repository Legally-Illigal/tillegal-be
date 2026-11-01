using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Commands.CreateEvaluation;

public sealed record CreateEvaluationCommand(
    Guid MessageId,
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment) : IRequest<AiEvaluationDto>;

