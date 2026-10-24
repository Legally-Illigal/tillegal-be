using System.Text.Json;
using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;
using Tillegal.Application.Features.Auth.Models;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;

namespace Tillegal.Application.UnitTests.Features.Auth;

public class VerifyRegistrationCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly AuthService _authService;
    private readonly VerifyRegistrationCommandHandler _handler;

    public VerifyRegistrationCommandHandlerTests()
    {
        _authService = new AuthService(_userRepository);
        _mapper = new Mapper(MappingConfiguration.Create());
        _handler = new VerifyRegistrationCommandHandler(_authService, _cacheService, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_WhenCacheMissingOrExpired_ShouldThrowDomainException()
    {
        // Arrange
        var command = new VerifyRegistrationCommand("user@example.com", "123456");
        _cacheService.GetAsync("pending_reg:user@example.com", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_WhenOtpMismatch_ShouldThrowDomainException()
    {
        // Arrange
        var pending = new PendingRegistration(
            "user@example.com",
            "hashed_pwd",
            "Full Name",
            null,
            "654321",
            DateTimeOffset.UtcNow);

        var cachedBytes = JsonSerializer.SerializeToUtf8Bytes(pending);
        _cacheService.GetAsync("pending_reg:user@example.com", Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        var command = new VerifyRegistrationCommand("user@example.com", "123456");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Invalid verification code*");
    }

    [Fact]
    public async Task Handle_WhenOtpCorrect_ShouldCreateUserSaveAndClearCache()
    {
        // Arrange
        var defaultRole = new Role(Guid.NewGuid(), "User");
        _userRepository.GetDefaultRoleAsync("User", Arg.Any<CancellationToken>())
            .Returns(defaultRole);
        _userRepository.ExistsByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(false);

        var pending = new PendingRegistration(
            "user@example.com",
            "hashed_pwd",
            "Full Name",
            null,
            "123456",
            DateTimeOffset.UtcNow);

        var cachedBytes = JsonSerializer.SerializeToUtf8Bytes(pending);
        _cacheService.GetAsync("pending_reg:user@example.com", Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        var command = new VerifyRegistrationCommand("user@example.com", "123456");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().NotBeEmpty();
        result.Email.Should().Be("user@example.com");
        result.FullName.Should().Be("Full Name");
        result.Message.Should().Be("Account registration and verification completed successfully.");

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "user@example.com" && u.FullName == "Full Name" && u.RoleId == defaultRole.Id),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _cacheService.Received(1).RemoveAsync(
            "pending_reg:user@example.com",
            Arg.Any<CancellationToken>());
    }
}
