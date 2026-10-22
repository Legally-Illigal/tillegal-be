using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Queries.GetCurrentUser;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.UnitTests.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _handler = new GetCurrentUserQueryHandler(_currentUserService, _userRepository);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedAndUserExists_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = new Role(Guid.NewGuid(), "Lawer");
        var user = new User(
            userId,
            role.Id,
            "Luật Sư Nguyễn Văn B",
            "lawer_b",
            "lawer.b@tillegal.com",
            "hash",
            "0900000000",
            DateTimeOffset.UtcNow,
            null,
            role);

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        _userRepository.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FullName.Should().Be("Luật Sư Nguyễn Văn B");
        result.UserName.Should().Be("lawer_b");
        result.Email.Should().Be("lawer.b@tillegal.com");
        result.Role.Should().Be("Lawer");
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(false);
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var act = async () => await _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedButUserNotFoundInDb_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);
        _userRepository.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var act = async () => await _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID '{userId}' was not found.");
    }
}
