using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Admin.Users.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateUserStatusCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
            throw new ForbiddenAccessException("Only administrators can access this resource.");

        if (currentUserService.UserId == request.Id && !request.IsActive)
            throw new DomainException("Administrators cannot lock or deactivate their own account.");

        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            throw new NotFoundException("User", request.Id);

        user.IsActive = request.IsActive;

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserDto>(user);
    }
}

