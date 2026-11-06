using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationSummary;

public sealed class GetAdminAiEvaluationSummaryQueryHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService,
    IMapper mapper) : IRequestHandler<GetAdminAiEvaluationSummaryQuery, AiEvaluationSummaryDto>
{
    public async Task<AiEvaluationSummaryDto> Handle(
        GetAdminAiEvaluationSummaryQuery request,
        CancellationToken cancellationToken)
    {
        _ = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
        {
            throw new ForbiddenAccessException("Only administrators can view evaluation management reports.");
        }

        var summary = await aiEvaluationService.GetSummaryAsync(cancellationToken);
        return mapper.Map<AiEvaluationSummaryDto>(summary);
    }
}

