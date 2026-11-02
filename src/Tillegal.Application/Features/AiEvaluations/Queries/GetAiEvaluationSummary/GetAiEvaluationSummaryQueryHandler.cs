using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluationSummary;

public sealed class GetAiEvaluationSummaryQueryHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService) : IRequestHandler<GetAiEvaluationSummaryQuery, AiEvaluationSummaryDto>
{
    public async Task<AiEvaluationSummaryDto> Handle(
        GetAiEvaluationSummaryQuery request,
        CancellationToken cancellationToken)
    {
        _ = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var (totalCount, averageRating, helpfulCount, unhelpfulCount) =
            await aiEvaluationService.GetSummaryAsync(cancellationToken);

        return new AiEvaluationSummaryDto(
            totalCount,
            averageRating,
            helpfulCount,
            unhelpfulCount);
    }
}

