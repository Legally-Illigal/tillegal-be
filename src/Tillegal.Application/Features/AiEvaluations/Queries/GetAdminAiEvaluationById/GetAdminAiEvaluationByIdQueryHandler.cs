using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationById;

public sealed class GetAdminAiEvaluationByIdQueryHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService,
    IMapper mapper) : IRequestHandler<GetAdminAiEvaluationByIdQuery, AiEvaluationDto>
{
    public async Task<AiEvaluationDto> Handle(
        GetAdminAiEvaluationByIdQuery request,
        CancellationToken cancellationToken)
    {
        _ = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
        {
            throw new ForbiddenAccessException("Only administrators can view evaluation management reports.");
        }

        var evaluation = await aiEvaluationService.GetEvaluationByIdAsync(request.Id, cancellationToken);
        if (evaluation is null)
        {
            throw new NotFoundException("AiEvaluationLog", request.Id);
        }

        return mapper.Map<AiEvaluationDto>(evaluation);
    }
}

