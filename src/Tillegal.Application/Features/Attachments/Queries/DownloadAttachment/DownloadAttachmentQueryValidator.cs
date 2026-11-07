using FluentValidation;

namespace Tillegal.Application.Features.Attachments.Queries.DownloadAttachment;

public sealed class DownloadAttachmentQueryValidator : AbstractValidator<DownloadAttachmentQuery>
{
    public DownloadAttachmentQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.AttachmentId)
            .NotEmpty()
            .WithMessage("AttachmentId cannot be empty.");
    }
}

