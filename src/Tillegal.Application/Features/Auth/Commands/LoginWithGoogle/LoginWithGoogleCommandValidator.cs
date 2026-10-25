using FluentValidation;

namespace Tillegal.Application.Features.Auth.Commands.LoginWithGoogle;

public sealed class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(command => command.IdToken)
            .NotEmpty().WithMessage("Google ID token is required.")
            .MaximumLength(16_384).WithMessage("Google ID token is too long.");
    }
}
