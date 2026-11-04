using FluentValidation;

namespace Tillegal.Application.Features.Admin.Users.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User Id is required.");
    }
}

