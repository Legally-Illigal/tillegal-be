using Mapster;
using Tillegal.Api.Contracts.Conversations;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Application.Features.Conversations.Models;

namespace Tillegal.Api.Mappings;

public sealed class ConversationApiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateConversationApiRequest, CreateConversationCommand>();
        config.NewConfig<ConversationDto, ConversationApiResponse>();
    }
}
