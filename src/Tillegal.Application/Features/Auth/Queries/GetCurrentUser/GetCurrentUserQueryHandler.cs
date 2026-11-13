using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User identifier is missing or invalid.");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account has been deactivated.");
        }

        return mapper.Map<CurrentUserDto>(user);
    }
}

