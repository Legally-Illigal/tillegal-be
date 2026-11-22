using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Users.Models;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Users.Queries.GetMyProfile;

public sealed class GetMyProfileQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetMyProfileQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
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

        return mapper.Map<UserProfileDto>(user);
    }
}
