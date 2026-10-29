using Mapster;
using Tillegal.Application.Features.Conversations.Models;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Conversations.Mappings;

public sealed class ConversationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Conversation, ConversationDto>();
    }
}
