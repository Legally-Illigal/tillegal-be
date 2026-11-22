using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Users.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IPasswordHashingService passwordHashingService,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeMyPasswordCommand>
{
    public async Task Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
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

        user.PasswordHash = passwordHashingService.HashPassword(request.NewPassword);

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
