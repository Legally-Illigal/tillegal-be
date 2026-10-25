using Mapster;
using Tillegal.Application.Features.Conversations.Commands.AskQuestion;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Conversations.Mappings;

public sealed class ConversationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Conversation, CreateConversationResult>();
        config.NewConfig<ConversationMessage, ConversationMessageDto>();
    }
}
