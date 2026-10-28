using FluentValidation;

namespace Tillegal.Application.Features.Conversations.Commands.DeleteConversation;

public sealed class DeleteConversationCommandValidator : AbstractValidator<DeleteConversationCommand>
{
    public DeleteConversationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Conversation ID is required.");
    }
}
