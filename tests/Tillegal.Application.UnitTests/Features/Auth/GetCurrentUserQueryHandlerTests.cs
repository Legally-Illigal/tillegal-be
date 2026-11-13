using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Queries.GetCurrentUser;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Auth;

public sealed class GetCurrentUserQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _handler = new GetCurrentUserQueryHandler(_currentUserService, _userRepository, _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        var act = () => _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserIdMissing_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns((Guid?)null);

        var act = () => _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

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

        var act = () => _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

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

        var act = () => _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User account has been deactivated.");
    }

    [Fact]
    public async Task Handle_WhenUserValid_ShouldReturnCurrentUserDto()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        var user = new User
        {
            Id = userId,
            FullName = "Nguyen Van A",
            Email = "a@example.com",
            PhoneNumber = "0123456789",
            Role = new Role { Id = Guid.NewGuid(), Name = "User" },
            IsActive = true
        };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var expectedDto = new CurrentUserDto(userId, "Nguyen Van A", "a@example.com", "0123456789", "User", true);
        _mapper.Map<CurrentUserDto>(user).Returns(expectedDto);

        var result = await _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().Be(expectedDto);
    }
}

