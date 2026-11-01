using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Commands.DeleteEvaluation;

public sealed class DeleteEvaluationCommandHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteEvaluationCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteEvaluationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var evaluation = await aiEvaluationService.GetByIdAsync(request.Id, cancellationToken);
        if (evaluation is null)
        {
            throw new NotFoundException("AiEvaluationLog", request.Id);
        }

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (evaluation.EvaluatorUserId != userId && !isAdmin)
        {
            throw new ForbiddenAccessException("You do not have permission to delete this evaluation.");
        }

        await aiEvaluationService.DeleteEvaluationAsync(evaluation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
