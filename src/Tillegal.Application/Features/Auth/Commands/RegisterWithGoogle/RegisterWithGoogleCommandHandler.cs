using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Mappings;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Auth.Commands.RegisterWithGoogle;

public sealed class RegisterWithGoogleCommandHandler(
    IGoogleIdentityTokenValidator googleIdentityTokenValidator,
    IAuthService authService,
    IUnitOfWork unitOfWork,
    IJwtTokenGenerator jwtTokenGenerator,
    IMapper mapper) : IRequestHandler<RegisterWithGoogleCommand, GoogleRegistrationResult>
{
    public async Task<GoogleRegistrationResult> Handle(
        RegisterWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        var identity = await googleIdentityTokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (!identity.EmailVerified)
        {
            throw new DomainException("The Google account email must be verified.");
        }

        if (string.IsNullOrWhiteSpace(identity.Subject)
            || string.IsNullOrWhiteSpace(identity.Email)
            || string.IsNullOrWhiteSpace(identity.FullName))
        {
            throw new DomainException("The Google account must provide a subject, email address, and full name.");
        }

        var email = identity.Email.Trim().ToLowerInvariant();
        var user = await authService.CreateUserAsync(
            email,
            passwordHash: null,
            fullName: identity.FullName.Trim(),
            googleSubject: identity.Subject,
            cancellationToken: cancellationToken);

        var generatedJwt = jwtTokenGenerator.Generate(user.Id, email, user.FullName);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<GoogleRegistrationResult>(new GoogleAuthResultMappingSource(
            user,
            generatedJwt,
            "Google account registration completed successfully."));
    }
}
