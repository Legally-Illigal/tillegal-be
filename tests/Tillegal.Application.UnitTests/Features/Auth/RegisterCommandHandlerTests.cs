using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Features.Auth.Commands.Register;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;

namespace Tillegal.Application.UnitTests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHashingService _passwordHashingService = Substitute.For<IPasswordHashingService>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly AuthService _authService;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _authService = new AuthService(_userRepository);
        _dateTimeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
        _passwordHashingService.HashPassword(Arg.Any<string>()).Returns("hashed_pwd");

        _handler = new RegisterCommandHandler(
            _authService,
            _passwordHashingService,
            _cacheService,
            _emailService,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123", "Test User");
        _userRepository.ExistsByEmailAsync(command.Email.ToLowerInvariant(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already registered*");

        await _cacheService.DidNotReceiveWithAnyArgs().SetAsync(default!, default!, default, default);
        await _emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldStoreInCacheAndSendEmail()
    {
        // Arrange
        var command = new RegisterCommand("newuser@example.com", "Password123", "New User", "0123456789");
        _userRepository.ExistsByEmailAsync(command.Email.ToLowerInvariant(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ExpirationMinutes.Should().Be(10);
        result.Message.Should().Contain("newuser@example.com");

        await _cacheService.Received(1).SetAsync(
            Arg.Is<string>(key => key == "pending_reg:newuser@example.com"),
            Arg.Any<byte[]>(),
            Arg.Is<TimeSpan?>(t => t == TimeSpan.FromMinutes(10)),
            Arg.Any<CancellationToken>());

        await _emailService.Received(1).SendAsync(
            Arg.Is<string>(to => to == "newuser@example.com"),
            Arg.Is<string>(subj => subj.Contains("Tillegal")),
            Arg.Is<string>(body => body.Contains("New User")),
            Arg.Any<CancellationToken>());
    }
}
