using FluentValidation;

namespace Tillegal.Application.Features.Messages.Queries.GetMessageById;

public sealed class GetMessageByIdQueryValidator : AbstractValidator<GetMessageByIdQuery>
{
    public GetMessageByIdQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId cannot be empty.");
    }
}
