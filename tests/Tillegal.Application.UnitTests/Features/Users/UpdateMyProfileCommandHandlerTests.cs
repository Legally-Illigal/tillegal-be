using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Users.Commands.UpdateMyProfile;
using Tillegal.Application.Features.Users.Models;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Users;

public sealed class UpdateMyProfileCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly UpdateMyProfileCommandHandler _handler;

    public UpdateMyProfileCommandHandlerTests()
    {
        _handler = new UpdateMyProfileCommandHandler(_currentUserService, _userRepository, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        var act = () => _handler.Handle(new UpdateMyProfileCommand("New Name", null), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserIdMissing_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns((Guid?)null);

        var act = () => _handler.Handle(new UpdateMyProfileCommand("New Name", null), CancellationToken.None);

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

        var act = () => _handler.Handle(new UpdateMyProfileCommand("New Name", null), CancellationToken.None);

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

        var act = () => _handler.Handle(new UpdateMyProfileCommand("New Name", null), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User account has been deactivated.");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUpdateUserAndSaveChanges()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        var user = new User
        {
            Id = userId,
            FullName = "Old Name",
            Email = "user@example.com",
            PhoneNumber = "0123456789",
            Role = new Role { Id = Guid.NewGuid(), Name = "User" },
            IsActive = true
        };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var command = new UpdateMyProfileCommand("  Nguyen Van Updated  ", "  0987654321  ");
        var expectedDto = new UserProfileDto(userId, "Nguyen Van Updated", "user@example.com", "0987654321", "User", true, user.CreatedAt, null);
        _mapper.Map<UserProfileDto>(user).Returns(expectedDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        user.FullName.Should().Be("Nguyen Van Updated");
        user.PhoneNumber.Should().Be("0987654321");
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Handle_WhenPhoneNumberIsWhitespace_ShouldSetPhoneNumberToNull()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        var user = new User
        {
            Id = userId,
            FullName = "Old Name",
            PhoneNumber = "0123456789",
            IsActive = true
        };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var command = new UpdateMyProfileCommand("New Name", "   ");
        await _handler.Handle(command, CancellationToken.None);

        user.PhoneNumber.Should().BeNull();
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
