using FluentValidation;

namespace Tillegal.Application.Features.Messages.Queries.GetConversationMessages;

public sealed class GetConversationMessagesQueryValidator : AbstractValidator<GetConversationMessagesQuery>
{
    public GetConversationMessagesQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50)
            .WithMessage("Limit must not exceed 50.");
    }
}
