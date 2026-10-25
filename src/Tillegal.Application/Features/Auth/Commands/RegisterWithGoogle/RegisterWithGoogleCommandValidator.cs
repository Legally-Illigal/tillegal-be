using FluentValidation;

namespace Tillegal.Application.Features.Auth.Commands.RegisterWithGoogle;

public sealed class RegisterWithGoogleCommandValidator : AbstractValidator<RegisterWithGoogleCommand>
{
    public RegisterWithGoogleCommandValidator()
    {
        RuleFor(command => command.IdToken)
            .NotEmpty().WithMessage("Google ID token is required.")
            .MaximumLength(16_384).WithMessage("Google ID token is too long.");
    }
}
