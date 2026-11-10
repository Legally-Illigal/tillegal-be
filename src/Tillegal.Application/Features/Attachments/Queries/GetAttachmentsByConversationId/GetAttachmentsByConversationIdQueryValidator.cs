using FluentValidation;

namespace Tillegal.Application.Features.Attachments.Queries.GetAttachmentsByConversationId;

public sealed class GetAttachmentsByConversationIdQueryValidator : AbstractValidator<GetAttachmentsByConversationIdQuery>
{
    public GetAttachmentsByConversationIdQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");
    }
}

