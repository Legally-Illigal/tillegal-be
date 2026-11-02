using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationById;

public sealed class GetEvaluationByIdQueryHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService,
    IMapper mapper) : IRequestHandler<GetEvaluationByIdQuery, AiEvaluationDto>
{
    public async Task<AiEvaluationDto> Handle(
        GetEvaluationByIdQuery request,
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
            throw new ForbiddenAccessException("You do not have permission to view this evaluation.");
        }

        return mapper.Map<AiEvaluationDto>(evaluation);
    }
}
