using FluentValidation;

namespace Tillegal.Application.Features.Admin.Users.Commands.GrantLawyerRole;

public sealed class GrantLawyerRoleCommandValidator : AbstractValidator<GrantLawyerRoleCommand>
{
    public GrantLawyerRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}

