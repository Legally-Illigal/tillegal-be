using FluentValidation;

namespace Tillegal.Application.Features.Attachments.Queries.GetAttachmentById;

public sealed class GetAttachmentByIdQueryValidator : AbstractValidator<GetAttachmentByIdQuery>
{
    public GetAttachmentByIdQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.AttachmentId)
            .NotEmpty()
            .WithMessage("AttachmentId cannot be empty.");
    }
}

