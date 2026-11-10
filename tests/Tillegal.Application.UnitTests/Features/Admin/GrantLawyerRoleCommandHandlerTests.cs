using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Admin.Users.Commands.GrantLawyerRole;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Admin;

public sealed class GrantLawyerRoleCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GrantLawyerRoleCommandHandler _handler;

    public GrantLawyerRoleCommandHandlerTests()
    {
        _handler = new GrantLawyerRoleCommandHandler(_currentUserService, _userRepository, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        var act = () => _handler.Handle(new GrantLawyerRoleCommand(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenNotAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("User");

        var act = () => _handler.Handle(new GrantLawyerRoleCommand(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Only administrators can access this resource.");
    }

    [Fact]
    public async Task Handle_WhenAdminAttemptsToChangeOwnRole_ShouldThrowDomainException()
    {
        var adminId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var act = () => _handler.Handle(new GrantLawyerRoleCommand(adminId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Administrators cannot change their own role.");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        var adminId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);
        _userRepository.GetByIdAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _handler.Handle(new GrantLawyerRoleCommand(targetUserId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTargetUserIsAdmin_ShouldThrowDomainException()
    {
        var adminId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var targetUser = new User
        {
            Id = targetUserId,
            FullName = "Other Admin",
            Role = new Role(Guid.CreateVersion7(), "Admin"),
            IsActive = true
        };
        _userRepository.GetByIdAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(targetUser);

        var act = () => _handler.Handle(new GrantLawyerRoleCommand(targetUserId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot change the role of an administrator.");
    }

    [Theory]
    [InlineData("Lawyer")]
    [InlineData("Lawer")]
    public async Task Handle_WhenTargetUserAlreadyLawyer_ShouldThrowDomainException(string lawyerRoleName)
    {
        var adminId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var targetUser = new User
        {
            Id = targetUserId,
            FullName = "Lawyer User",
            Role = new Role(Guid.CreateVersion7(), lawyerRoleName),
            IsActive = true
        };
        _userRepository.GetByIdAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(targetUser);

        var act = () => _handler.Handle(new GrantLawyerRoleCommand(targetUserId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("User already has the Lawyer role.");
    }

    [Fact]
    public async Task Handle_WhenTargetUserIsInactive_ShouldThrowDomainException()
    {
        var adminId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var targetUser = new User
        {
            Id = targetUserId,
            FullName = "Locked User",
            Role = new Role(Guid.CreateVersion7(), "User"),
            IsActive = false
        };
        _userRepository.GetByIdAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(targetUser);

        var act = () => _handler.Handle(new GrantLawyerRoleCommand(targetUserId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot grant lawyer role to an inactive user.");
    }

    [Fact]
    public async Task Handle_WhenLawyerRoleNotFound_ShouldThrowDomainException()
    {
        var adminId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var targetUser = new User
        {
            Id = targetUserId,
            FullName = "Normal User",
            Role = new Role(Guid.CreateVersion7(), "User"),
            IsActive = true
        };
        _userRepository.GetByIdAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(targetUser);
        _userRepository.GetRoleByNameAsync("Lawyer", Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var act = () => _handler.Handle(new GrantLawyerRoleCommand(targetUserId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Lawyer role could not be found.");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldGrantLawyerRoleAndSave()
    {
        var adminId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var lawyerRoleId = Guid.CreateVersion7();
        var lawyerRole = new Role(lawyerRoleId, "Lawyer");

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var targetUser = new User
        {
            Id = targetUserId,
            RoleId = Guid.CreateVersion7(),
            FullName = "Standard User",
            Role = new Role(Guid.CreateVersion7(), "User"),
            IsActive = true
        };
        _userRepository.GetByIdAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(targetUser);
        _userRepository.GetRoleByNameAsync("Lawyer", Arg.Any<CancellationToken>())
            .Returns(lawyerRole);

        var expectedDto = new UserDto(targetUserId, "Standard User", "user@example.com", null, "Lawyer", true, DateTimeOffset.UtcNow, null);
        _mapper.Map<UserDto>(targetUser).Returns(expectedDto);

        var command = new GrantLawyerRoleCommand(targetUserId);
        var result = await _handler.Handle(command, CancellationToken.None);

        targetUser.RoleId.Should().Be(lawyerRoleId);
        targetUser.Role.Should().Be(lawyerRole);
        await _userRepository.Received(1).UpdateAsync(targetUser, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Should().Be(expectedDto);
    }
}

