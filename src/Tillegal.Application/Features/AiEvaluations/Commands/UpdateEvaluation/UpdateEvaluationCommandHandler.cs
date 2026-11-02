using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Commands.UpdateEvaluation;

public sealed class UpdateEvaluationCommandHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateEvaluationCommand, AiEvaluationDto>
{
    public async Task<AiEvaluationDto> Handle(
        UpdateEvaluationCommand request,
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
            throw new ForbiddenAccessException("You do not have permission to update this evaluation.");
        }

        var updated = await aiEvaluationService.UpdateEvaluationAsync(
            evaluation,
            request.IsHelpful,
            request.Rating,
            request.FeedbackComment,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<AiEvaluationDto>(updated);
    }
}
