using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Common.Models;
using Tillegal.Application.Features.Auth.Commands.RegisterWithGoogle;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;

namespace Tillegal.Application.UnitTests.Features.Auth;

public sealed class RegisterWithGoogleCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IGoogleIdentityTokenValidator _tokenValidator = Substitute.For<IGoogleIdentityTokenValidator>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = new Mapper(MappingConfiguration.Create());
    private readonly RegisterWithGoogleCommandHandler _handler;

    public RegisterWithGoogleCommandHandlerTests()
    {
        _handler = new RegisterWithGoogleCommandHandler(
            _tokenValidator,
            new AuthService(_userRepository),
            _unitOfWork,
            _jwtTokenGenerator,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenGoogleEmailIsVerified_ShouldCreatePasswordlessUser()
    {
        var role = new Role(Guid.NewGuid(), "User");
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-123", " User@Example.com ", " Nguyen Van A ", true));
        var generatedJwt = new GeneratedJwt("tillegal-jwt", DateTimeOffset.UtcNow.AddHours(1));
        _jwtTokenGenerator.Generate(Arg.Any<Guid>(), "user@example.com", "Nguyen Van A")
            .Returns(generatedJwt);
        _userRepository.ExistsByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.GetDefaultRoleAsync("User", Arg.Any<CancellationToken>())
            .Returns(role);

        var result = await _handler.Handle(new RegisterWithGoogleCommand("google-id-token"), CancellationToken.None);

        result.Email.Should().Be("user@example.com");
        result.FullName.Should().Be("Nguyen Van A");
        result.Message.Should().Be("Google account registration completed successfully.");
        result.AccessToken.Should().Be(generatedJwt.AccessToken);
        result.ExpiresAt.Should().Be(generatedJwt.ExpiresAt);
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(user => user.Email == "user@example.com"
                && user.GoogleSubject == "google-sub-123"
                && user.PasswordHash == null
                && user.RoleId == role.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGoogleEmailIsNotVerified_ShouldRejectRegistration()
    {
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-123", "user@example.com", "Nguyen Van A", false));

        var act = () => _handler.Handle(new RegisterWithGoogleCommand("google-id-token"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("The Google account email must be verified.");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _jwtTokenGenerator.DidNotReceive().Generate(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }
}
