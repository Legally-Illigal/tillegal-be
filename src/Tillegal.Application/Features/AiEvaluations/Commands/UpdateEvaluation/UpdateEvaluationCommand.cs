using MediatR;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Application.Features.AiEvaluations.Commands.UpdateEvaluation;

public sealed record UpdateEvaluationCommand(
    Guid Id,
    bool? IsHelpful,
    int? Rating,
    string? FeedbackComment) : IRequest<AiEvaluationDto>;

