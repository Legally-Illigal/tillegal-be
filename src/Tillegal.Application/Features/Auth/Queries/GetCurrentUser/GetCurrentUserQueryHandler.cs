using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.DTOs;

namespace Tillegal.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository) : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var user = await userRepository.FindByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException($"User with ID '{currentUserService.UserId.Value}' was not found.");
        }

        return new UserDto(
            user.Id,
            user.FullName,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.Role?.Name);
    }
}
