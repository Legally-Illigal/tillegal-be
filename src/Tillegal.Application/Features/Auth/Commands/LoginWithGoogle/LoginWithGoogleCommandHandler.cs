using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Mappings;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Auth.Commands.LoginWithGoogle;

public sealed class LoginWithGoogleCommandHandler(
    IGoogleIdentityTokenValidator googleIdentityTokenValidator,
    IUserRepository userRepository,
    IAuthService authService,
    IUnitOfWork unitOfWork,
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

        if (string.IsNullOrWhiteSpace(identity.Email))
        {
            throw new DomainException("The Google account email is missing.");
        }

        var email = identity.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByGoogleSubjectAsync(identity.Subject, cancellationToken);

        if (user is null)
        {
            user = await userRepository.GetByEmailAsync(email, cancellationToken);

            if (user is not null)
            {
                if (!string.IsNullOrWhiteSpace(user.GoogleSubject) && user.GoogleSubject != identity.Subject)
                {
                    throw new DomainException("This email is already associated with another Google account.");
                }

                if (string.IsNullOrWhiteSpace(user.GoogleSubject))
                {
                    user.GoogleSubject = identity.Subject;
                    await userRepository.UpdateAsync(user, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                var fullName = !string.IsNullOrWhiteSpace(identity.FullName)
                    ? identity.FullName.Trim()
                    : email.Split('@')[0];

                user = await authService.CreateUserAsync(
                    email,
                    passwordHash: null,
                    fullName: fullName,
                    googleSubject: identity.Subject,
                    cancellationToken: cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        var userEmail = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : email;
        var generatedJwt = jwtTokenGenerator.Generate(user.Id, userEmail, user.FullName);

        return mapper.Map<GoogleLoginResult>(new GoogleAuthResultMappingSource(
            user,
            generatedJwt,
            "Google login completed successfully."));
    }
}
