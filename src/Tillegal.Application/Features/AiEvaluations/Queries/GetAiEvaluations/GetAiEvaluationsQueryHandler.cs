using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluations;

public sealed class GetAiEvaluationsQueryHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService,
    IMapper mapper) : IRequestHandler<GetAiEvaluationsQuery, PagedAiEvaluationsDto>
{
    public async Task<PagedAiEvaluationsDto> Handle(
        GetAiEvaluationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        var effectiveEvaluatorId = isAdmin ? request.EvaluatorUserId : userId;

        var totalCount = await aiEvaluationService.GetCountAsync(
            request.IsHelpful,
            request.Rating,
            effectiveEvaluatorId,
            cancellationToken);

        var skip = (request.PageNumber - 1) * request.PageSize;

        var items = await aiEvaluationService.GetPagedAsync(
            skip,
            request.PageSize,
            request.IsHelpful,
            request.Rating,
            effectiveEvaluatorId,
            cancellationToken);

        var dtos = items.Select(mapper.Map<AiEvaluationDto>).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedAiEvaluationsDto(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);
    }
}
