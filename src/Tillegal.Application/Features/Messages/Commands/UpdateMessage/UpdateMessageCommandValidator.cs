using FluentValidation;

namespace Tillegal.Application.Features.Messages.Commands.UpdateMessage;

public sealed class UpdateMessageCommandValidator : AbstractValidator<UpdateMessageCommand>
{
    public UpdateMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId cannot be empty.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content cannot be empty.");
    }
}
