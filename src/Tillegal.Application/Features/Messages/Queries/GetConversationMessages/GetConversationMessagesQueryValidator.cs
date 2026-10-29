using FluentValidation;

namespace Tillegal.Application.Features.Messages.Queries.GetConversationMessages;

public sealed class GetConversationMessagesQueryValidator : AbstractValidator<GetConversationMessagesQuery>
{
    public GetConversationMessagesQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");
    }
}
