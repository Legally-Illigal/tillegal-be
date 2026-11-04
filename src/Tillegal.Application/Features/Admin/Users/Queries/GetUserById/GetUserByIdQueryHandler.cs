using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Admin.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
            throw new ForbiddenAccessException("Only administrators can access this resource.");

        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            throw new NotFoundException("User", request.Id);

        return mapper.Map<UserDto>(user);
    }
}
