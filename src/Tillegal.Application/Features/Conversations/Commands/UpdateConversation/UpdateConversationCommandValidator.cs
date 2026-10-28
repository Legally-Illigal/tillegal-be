using FluentValidation;

namespace Tillegal.Application.Features.Conversations.Commands.UpdateConversation;

public sealed class UpdateConversationCommandValidator : AbstractValidator<UpdateConversationCommand>
{
    public UpdateConversationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Conversation ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");
    }
}
