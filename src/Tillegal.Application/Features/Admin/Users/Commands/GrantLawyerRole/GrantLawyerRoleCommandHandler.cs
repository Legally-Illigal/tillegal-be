using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Admin.Users.Commands.GrantLawyerRole;

public sealed class GrantLawyerRoleCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<GrantLawyerRoleCommand, UserDto>
{
    public async Task<UserDto> Handle(GrantLawyerRoleCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
            throw new ForbiddenAccessException("Only administrators can access this resource.");

        if (currentUserService.UserId == request.UserId)
            throw new DomainException("Administrators cannot change their own role.");

        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException("User", request.UserId);

        if (string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Cannot change the role of an administrator.");

        if (string.Equals(user.Role?.Name, "Lawyer", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(user.Role?.Name, "Lawer", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("User already has the Lawyer role.");
        }

        if (!user.IsActive)
            throw new DomainException("Cannot grant lawyer role to an inactive user.");

        var lawyerRole = await userRepository.GetRoleByNameAsync("Lawyer", cancellationToken);
        if (lawyerRole is null)
            throw new DomainException("Lawyer role could not be found.");

        user.RoleId = lawyerRole.Id;
        user.Role = lawyerRole;

        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserDto>(user);
    }
}

