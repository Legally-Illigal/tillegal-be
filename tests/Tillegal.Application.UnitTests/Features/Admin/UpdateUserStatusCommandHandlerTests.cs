using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Admin.Users.Commands.UpdateUserStatus;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Admin;

public sealed class UpdateUserStatusCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly UpdateUserStatusCommandHandler _handler;

    public UpdateUserStatusCommandHandlerTests()
    {
        _handler = new UpdateUserStatusCommandHandler(_currentUserService, _userRepository, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        var act = () => _handler.Handle(new UpdateUserStatusCommand(Guid.CreateVersion7(), false), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenNotAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("User");

        var act = () => _handler.Handle(new UpdateUserStatusCommand(Guid.CreateVersion7(), false), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenAdminAttemptsToLockSelf_ShouldThrowDomainException()
    {
        var adminId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var act = () => _handler.Handle(new UpdateUserStatusCommand(adminId, false), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Administrators cannot lock or deactivate their own account.");
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

        var act = () => _handler.Handle(new UpdateUserStatusCommand(targetUserId, false), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLockingUser_ShouldSetIsActiveFalseAndSave()
    {
        var adminId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var user = new User { Id = targetUserId, FullName = "Target User", IsActive = true };
        _userRepository.GetByIdAsync(targetUserId, Arg.Any<CancellationToken>()).Returns(user);

        var expectedDto = new UserDto(targetUserId, "Target User", null, null, null, false, DateTimeOffset.UtcNow, null);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(expectedDto);

        var command = new UpdateUserStatusCommand(targetUserId, false);
        var result = await _handler.Handle(command, CancellationToken.None);

        user.IsActive.Should().BeFalse();
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Handle_WhenUnlockingUser_ShouldSetIsActiveTrueAndSave()
    {
        var adminId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _currentUserService.UserId.Returns(adminId);

        var user = new User { Id = targetUserId, FullName = "Target User", IsActive = false };
        _userRepository.GetByIdAsync(targetUserId, Arg.Any<CancellationToken>()).Returns(user);

        var expectedDto = new UserDto(targetUserId, "Target User", null, null, null, true, DateTimeOffset.UtcNow, null);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(expectedDto);

        var command = new UpdateUserStatusCommand(targetUserId, true);
        var result = await _handler.Handle(command, CancellationToken.None);

        user.IsActive.Should().BeTrue();
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Should().Be(expectedDto);
    }
}

