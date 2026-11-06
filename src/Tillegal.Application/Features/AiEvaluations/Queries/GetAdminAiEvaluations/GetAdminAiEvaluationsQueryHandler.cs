using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluations;

public sealed class GetAdminAiEvaluationsQueryHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService,
    IMapper mapper) : IRequestHandler<GetAdminAiEvaluationsQuery, PagedAiEvaluationsDto>
{
    public async Task<PagedAiEvaluationsDto> Handle(
        GetAdminAiEvaluationsQuery request,
        CancellationToken cancellationToken)
    {
        _ = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
        {
            throw new ForbiddenAccessException("Only administrators can view evaluation management reports.");
        }

        var (items, totalCount) = await aiEvaluationService.GetPagedEvaluationsAsync(
            request.PageNumber,
            request.PageSize,
            request.IsHelpful,
            request.Rating,
            request.EvaluatorUserId,
            request.EvaluatorRole,
            cancellationToken);

        var dtos = mapper.Map<IReadOnlyList<AiEvaluationDto>>(items);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedAiEvaluationsDto(
            dtos,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages);
    }
}

