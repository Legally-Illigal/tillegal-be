using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Users.Commands.DeactivateMyAccount;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Users;

public sealed class DeactivateMyAccountCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeactivateMyAccountCommandHandler _handler;

    public DeactivateMyAccountCommandHandlerTests()
    {
        _handler = new DeactivateMyAccountCommandHandler(_currentUserService, _userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        var act = () => _handler.Handle(new DeactivateMyAccountCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserIdMissing_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns((Guid?)null);

        var act = () => _handler.Handle(new DeactivateMyAccountCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User identifier is missing or invalid.");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(new DeactivateMyAccountCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsDeactivated_ShouldThrowUnauthorizedException()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        var deactivatedUser = new User
        {
            Id = userId,
            FullName = "Deactivated User",
            Email = "inactive@example.com",
            IsActive = false
        };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(deactivatedUser);

        var act = () => _handler.Handle(new DeactivateMyAccountCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User account has been deactivated.");
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldThrowDomainException()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("Admin");

        var adminUser = new User
        {
            Id = userId,
            FullName = "Admin User",
            Email = "admin@example.com",
            Role = new Role { Id = Guid.NewGuid(), Name = "Admin" },
            IsActive = true
        };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(adminUser);

        var act = () => _handler.Handle(new DeactivateMyAccountCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Administrators cannot lock or deactivate their own account.");
    }

    [Fact]
    public async Task Handle_WhenUserIsRegularUser_ShouldDeactivateAccount()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");

        var user = new User
        {
            Id = userId,
            FullName = "Regular User",
            Email = "user@example.com",
            Role = new Role { Id = Guid.NewGuid(), Name = "User" },
            IsActive = true
        };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(new DeactivateMyAccountCommand(), CancellationToken.None);

        user.IsActive.Should().BeFalse();
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
