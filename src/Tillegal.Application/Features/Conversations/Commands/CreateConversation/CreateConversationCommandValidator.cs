using FluentValidation;

namespace Tillegal.Application.Features.Conversations.Commands.CreateConversation;

public sealed class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title cannot be empty or consist only of whitespace.")
            .When(x => x.Title is not null);
    }
}
