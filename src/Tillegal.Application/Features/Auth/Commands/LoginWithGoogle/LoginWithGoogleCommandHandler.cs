using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Mappings;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Application.Features.Auth.Commands.LoginWithGoogle;

public sealed class LoginWithGoogleCommandHandler(
    IGoogleIdentityTokenValidator googleIdentityTokenValidator,
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IMapper mapper) : IRequestHandler<LoginWithGoogleCommand, GoogleLoginResult>
{
    public async Task<GoogleLoginResult> Handle(
        LoginWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        var identity = await googleIdentityTokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (!identity.EmailVerified)
        {
            throw new DomainException("The Google account email must be verified.");
        }

        if (string.IsNullOrWhiteSpace(identity.Subject))
        {
            throw new DomainException("The Google account subject is missing.");
        }

        var user = await userRepository.GetByGoogleSubjectAsync(identity.Subject, cancellationToken)
            ?? throw new DomainException("No account is registered with this Google account.");

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new DomainException("The Google account has no email address.");
        }

        var generatedJwt = jwtTokenGenerator.Generate(user.Id, user.Email, user.FullName);
        return mapper.Map<GoogleLoginResult>(new GoogleAuthResultMappingSource(
            user,
            generatedJwt,
            "Google login completed successfully."));
    }
}
