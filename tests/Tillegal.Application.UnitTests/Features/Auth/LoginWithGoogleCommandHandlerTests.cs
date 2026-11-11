using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Common.Models;
using Tillegal.Application.Features.Auth.Commands.LoginWithGoogle;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;

namespace Tillegal.Application.UnitTests.Features.Auth;

public sealed class LoginWithGoogleCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IGoogleIdentityTokenValidator _tokenValidator = Substitute.For<IGoogleIdentityTokenValidator>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = new Mapper(MappingConfiguration.Create());
    private readonly LoginWithGoogleCommandHandler _handler;

    public LoginWithGoogleCommandHandlerTests()
    {
        _handler = new LoginWithGoogleCommandHandler(
            _tokenValidator,
            _userRepository,
            new AuthService(_userRepository),
            _unitOfWork,
            _jwtTokenGenerator,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenGoogleSubjectExists_ShouldReturnTillegalJwt()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FullName = "Nguyen Van A",
            GoogleSubject = "google-sub-123"
        };
        var generatedJwt = new GeneratedJwt("tillegal-jwt", DateTimeOffset.UtcNow.AddHours(1));
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-123", "user@example.com", "Nguyen Van A", true));
        _userRepository.GetByGoogleSubjectAsync("google-sub-123", Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenGenerator.Generate(user.Id, "user@example.com", "Nguyen Van A")
            .Returns(generatedJwt);

        var result = await _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("user@example.com");
        result.FullName.Should().Be("Nguyen Van A");
        result.Message.Should().Be("Google login completed successfully.");
        result.AccessToken.Should().Be("tillegal-jwt");
        result.ExpiresAt.Should().Be(generatedJwt.ExpiresAt);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldAutoRegisterAndReturnTillegalJwt()
    {
        var role = new Role(Guid.NewGuid(), "User");
        var generatedJwt = new GeneratedJwt("tillegal-jwt", DateTimeOffset.UtcNow.AddHours(1));
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-new", "newuser@example.com", "Nguyen Van B", true));
        _userRepository.GetByGoogleSubjectAsync("google-sub-new", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync("newuser@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.ExistsByEmailAsync("newuser@example.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.GetDefaultRoleAsync("User", Arg.Any<CancellationToken>())
            .Returns(role);
        _jwtTokenGenerator.Generate(Arg.Any<Guid>(), "newuser@example.com", "Nguyen Van B")
            .Returns(generatedJwt);

        var result = await _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        result.Email.Should().Be("newuser@example.com");
        result.FullName.Should().Be("Nguyen Van B");
        result.Message.Should().Be("Google login completed successfully.");
        result.AccessToken.Should().Be("tillegal-jwt");
        result.ExpiresAt.Should().Be(generatedJwt.ExpiresAt);

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "newuser@example.com"
                && u.GoogleSubject == "google-sub-new"
                && u.FullName == "Nguyen Van B"
                && u.PasswordHash == null
                && u.RoleId == role.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserExistsByEmailWithoutGoogleSubject_ShouldLinkGoogleSubjectAndReturnTillegalJwt()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FullName = "Nguyen Van C",
            GoogleSubject = null
        };
        var generatedJwt = new GeneratedJwt("tillegal-jwt", DateTimeOffset.UtcNow.AddHours(1));
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-123", "user@example.com", "Nguyen Van C", true));
        _userRepository.GetByGoogleSubjectAsync("google-sub-123", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _jwtTokenGenerator.Generate(existingUser.Id, "user@example.com", "Nguyen Van C")
            .Returns(generatedJwt);

        var result = await _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        result.UserId.Should().Be(existingUser.Id);
        result.AccessToken.Should().Be("tillegal-jwt");
        result.Message.Should().Be("Google login completed successfully.");
        existingUser.GoogleSubject.Should().Be("google-sub-123");

        await _userRepository.Received(1).UpdateAsync(existingUser, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserExistsByEmailWithDifferentGoogleSubject_ShouldRejectLogin()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FullName = "Nguyen Van C",
            GoogleSubject = "other-google-sub"
        };
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-123", "user@example.com", "Nguyen Van C", true));
        _userRepository.GetByGoogleSubjectAsync("google-sub-123", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var act = () => _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("This email is already associated with another Google account.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _jwtTokenGenerator.DidNotReceive().Generate(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenGoogleEmailIsNotVerified_ShouldRejectLogin()
    {
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-123", "user@example.com", "Nguyen Van A", false));

        var act = () => _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("The Google account email must be verified.");
        _jwtTokenGenerator.DidNotReceive().Generate(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenGoogleSubjectIsMissing_ShouldRejectLogin()
    {
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity(string.Empty, "user@example.com", "Nguyen Van A", true));

        var act = () => _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("The Google account subject is missing.");
    }

    [Fact]
    public async Task Handle_WhenGoogleEmailIsMissing_ShouldRejectLogin()
    {
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-123", string.Empty, "Nguyen Van A", true));

        var act = () => _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("The Google account email is missing.");
    }

    [Fact]
    public async Task Handle_WhenNewUserHasBlankFullName_ShouldFallbackToEmailPrefix()
    {
        var role = new Role(Guid.NewGuid(), "User");
        var generatedJwt = new GeneratedJwt("tillegal-jwt", DateTimeOffset.UtcNow.AddHours(1));
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-new", "fallbackuser@example.com", "   ", true));
        _userRepository.GetByGoogleSubjectAsync("google-sub-new", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync("fallbackuser@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.ExistsByEmailAsync("fallbackuser@example.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.GetDefaultRoleAsync("User", Arg.Any<CancellationToken>())
            .Returns(role);
        _jwtTokenGenerator.Generate(Arg.Any<Guid>(), "fallbackuser@example.com", "fallbackuser")
            .Returns(generatedJwt);

        var result = await _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        result.FullName.Should().Be("fallbackuser");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.FullName == "fallbackuser"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
