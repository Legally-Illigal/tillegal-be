using FluentValidation;

namespace Tillegal.Application.Features.Attachments.Commands.DeleteAttachment;

public sealed class DeleteAttachmentCommandValidator : AbstractValidator<DeleteAttachmentCommand>
{
    public DeleteAttachmentCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.AttachmentId)
            .NotEmpty()
            .WithMessage("AttachmentId cannot be empty.");
    }
}

