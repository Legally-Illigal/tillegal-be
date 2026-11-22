using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Users.Commands.ChangeMyPassword;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Users;

public sealed class ChangeMyPasswordCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHashingService _passwordHashingService = Substitute.For<IPasswordHashingService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeMyPasswordCommandHandler _handler;

    public ChangeMyPasswordCommandHandlerTests()
    {
        _handler = new ChangeMyPasswordCommandHandler(
            _currentUserService,
            _userRepository,
            _passwordHashingService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        var act = () => _handler.Handle(new ChangeMyPasswordCommand("NewPass123"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserIdMissing_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns((Guid?)null);

        var act = () => _handler.Handle(new ChangeMyPasswordCommand("NewPass123"), CancellationToken.None);

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

        var act = () => _handler.Handle(new ChangeMyPasswordCommand("NewPass123"), CancellationToken.None);

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

        var act = () => _handler.Handle(new ChangeMyPasswordCommand("NewPass123"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User account has been deactivated.");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldHashPasswordAndUpdateUser()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        var user = new User
        {
            Id = userId,
            FullName = "Active User",
            Email = "user@example.com",
            PasswordHash = "old_hashed_password",
            IsActive = true
        };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHashingService.HashPassword("NewPass123").Returns("new_hashed_password");

        await _handler.Handle(new ChangeMyPasswordCommand("NewPass123"), CancellationToken.None);

        user.PasswordHash.Should().Be("new_hashed_password");
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
