using FluentValidation;

namespace Tillegal.Application.Features.Messages.Commands.DeleteMessage;

public sealed class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
{
    public DeleteMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId cannot be empty.");
    }
}
