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

namespace Tillegal.Application.UnitTests.Features.Auth;

public sealed class LoginWithGoogleCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IGoogleIdentityTokenValidator _tokenValidator = Substitute.For<IGoogleIdentityTokenValidator>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IMapper _mapper = new Mapper(MappingConfiguration.Create());
    private readonly LoginWithGoogleCommandHandler _handler;

    public LoginWithGoogleCommandHandlerTests()
    {
        _handler = new LoginWithGoogleCommandHandler(_tokenValidator, _userRepository, _jwtTokenGenerator, _mapper);
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
        result.AccessToken.Should().Be("tillegal-jwt");
        result.ExpiresAt.Should().Be(generatedJwt.ExpiresAt);
    }

    [Fact]
    public async Task Handle_WhenGoogleSubjectIsNotRegistered_ShouldRejectLogin()
    {
        _tokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(new GoogleIdentity("google-sub-unknown", "user@example.com", "Nguyen Van A", true));
        _userRepository.GetByGoogleSubjectAsync("google-sub-unknown", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _handler.Handle(new LoginWithGoogleCommand("google-id-token"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("No account is registered with this Google account.");
        _jwtTokenGenerator.DidNotReceive().Generate(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }
}
