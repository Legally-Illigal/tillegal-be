using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Tillegal.Application.Features.Auth.Commands.Login;
using Tillegal.Application.Features.Auth.DTOs;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.UnitTests.Features.Auth.Commands.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IAuditLogRepository _auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IPasswordHashingService _passwordHashingService = Substitute.For<IPasswordHashingService>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(new DateTimeOffset(2026, 10, 22, 12, 0, 0, TimeSpan.Zero));
        _handler = new LoginCommandHandler(
            _userRepository,
            _auditLogRepository,
            _passwordHashingService,
            _jwtTokenGenerator,
            _dateTimeProvider,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsExpectedAuthDto_AndRecordsAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var role = new Role(roleId, "Admin");
        var user = new User(
            userId,
            roleId,
            "Quản Trị Viên",
            "admin_system",
            "admin.system@tillegal.com",
            "hashed_password_123",
            "0901000001",
            DateTimeOffset.UtcNow.AddMonths(-1),
            null,
            role);

        _userRepository.FindByEmailOrUserNameAsync("admin_system", Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHashingService.VerifyPassword("hashed_password_123", "P@ssword123!")
            .Returns(true);

        _jwtTokenGenerator.GenerateToken(user)
            .Returns(new TokenResult("test.jwt.token", 3600));

        var command = new LoginCommand("admin_system", "P@ssword123!", "127.0.0.1");

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().Be("test.jwt.token");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
        response.User.Id.Should().Be(userId);
        response.User.UserName.Should().Be("admin_system");
        response.User.Email.Should().Be("admin.system@tillegal.com");
        response.User.FullName.Should().Be("Quản Trị Viên");
        response.User.Role.Should().Be("Admin");

        await _auditLogRepository.Received(1).AddAsync(
            Arg.Is<AuditLog>(a =>
                a.Action == "LOGIN_SUCCESS" &&
                a.ActorUserId == userId &&
                a.ActorRole == "Admin" &&
                a.IpAddress == "127.0.0.1"),
            Arg.Any<CancellationToken>());

        await _userRepository.Received(1).UpdateLastActiveAtAsync(userId, _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _userRepository.FindByEmailOrUserNameAsync("unknown_user", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new LoginCommand("unknown_user", "AnyPassword123!");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email/username or password.");

        await _auditLogRepository.DidNotReceive().AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(
            userId,
            Guid.NewGuid(),
            "Nguyễn Văn A",
            "nguyen_vana",
            "nguyen.vana@gmail.com",
            "hashed_password_123",
            "0912000001",
            DateTimeOffset.UtcNow,
            null);

        _userRepository.FindByEmailOrUserNameAsync("nguyen_vana", Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHashingService.VerifyPassword("hashed_password_123", "WrongPassword")
            .Returns(false);

        var command = new LoginCommand("nguyen_vana", "WrongPassword");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email/username or password.");

        await _auditLogRepository.DidNotReceive().AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserPasswordHashIsNullOrWhitespace_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(
            userId,
            Guid.NewGuid(),
            "No Password User",
            "nopassword",
            "nopassword@example.com",
            null,
            null,
            DateTimeOffset.UtcNow,
            null);

        _userRepository.FindByEmailOrUserNameAsync("nopassword", Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new LoginCommand("nopassword", "AnyPassword");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email/username or password.");

        _passwordHashingService.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
    }
}
