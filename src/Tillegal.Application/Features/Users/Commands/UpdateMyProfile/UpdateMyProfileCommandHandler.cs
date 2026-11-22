using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Users.Models;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateMyProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
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

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserProfileDto>(user);
    }
}
