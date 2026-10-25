using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Auth.Commands.Login;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;

namespace Tillegal.Application.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHashingService _passwordHashingService = Substitute.For<IPasswordHashingService>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IMapper _mapper = new Mapper(MappingConfiguration.Create());
    private readonly AuthService _authService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _authService = new AuthService(_userRepository);

        _handler = new LoginCommandHandler(
            _authService,
            _passwordHashingService,
            _jwtTokenGenerator,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Password123");
        _userRepository.GetByEmailAsync("nonexistent@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        _jwtTokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!, default);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoPasswordHash_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "nopassword@example.com",
            FullName = "No Password",
            PasswordHash = null
        };

        var command = new LoginCommand("nopassword@example.com", "Password123");
        _userRepository.GetByEmailAsync("nopassword@example.com", Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        _jwtTokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!, default);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FullName = "User Name",
            PasswordHash = "stored_hash"
        };

        var command = new LoginCommand("user@example.com", "WrongPassword");
        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHashingService.VerifyPassword("stored_hash", "WrongPassword")
            .Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        _jwtTokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!, default);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ShouldReturnLoginResultWithTokenAndUser()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FullName = "Nguyen Van A",
            PhoneNumber = "0123456789",
            PasswordHash = "valid_hash",
            RoleId = roleId,
            Role = new Role(roleId, "User")
        };

        var command = new LoginCommand("user@example.com", "CorrectPassword");
        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHashingService.VerifyPassword("valid_hash", "CorrectPassword")
            .Returns(true);

        _jwtTokenGenerator.GenerateToken(user, "User")
            .Returns(new JwtTokenResult("mock.jwt.token", 3600));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock.jwt.token");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);
        result.User.Should().BeEquivalentTo(user);
        result.User.Id.Should().Be(user.Id);
        result.User.Email.Should().Be("user@example.com");
        result.User.FullName.Should().Be("Nguyen Van A");
        result.User.PhoneNumber.Should().Be("0123456789");
        result.User.Role?.Name.Should().Be("User");
    }
}
