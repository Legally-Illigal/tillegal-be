using MediatR;

namespace Tillegal.Application.Features.AiEvaluations.Commands.DeleteEvaluation;

public sealed record DeleteEvaluationCommand(Guid Id) : IRequest<Unit>;

