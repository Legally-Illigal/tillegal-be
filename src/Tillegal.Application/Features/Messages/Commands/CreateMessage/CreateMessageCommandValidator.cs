using FluentValidation;

namespace Tillegal.Application.Features.Messages.Commands.CreateMessage;

public sealed class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role cannot be empty.")
            .Must(role => role is not null && role.Trim().ToLowerInvariant() is "user" or "assistant" or "system")
            .WithMessage("Role must be 'user', 'assistant', or 'system'.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content cannot be empty.");
    }
}
