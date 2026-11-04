using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Admin.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetUsersQuery, PagedUsersDto>
{
    public async Task<PagedUsersDto> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
            throw new ForbiddenAccessException("Only administrators can access this resource.");

        var skip = (request.PageNumber - 1) * request.PageSize;
        var (items, totalCount) = await userRepository.GetPagedAsync(skip, request.PageSize, request.SearchTerm, cancellationToken);

        var dtos = items.Select(mapper.Map<UserDto>).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedUsersDto(dtos, totalCount, request.PageNumber, request.PageSize, totalPages);
    }
}
