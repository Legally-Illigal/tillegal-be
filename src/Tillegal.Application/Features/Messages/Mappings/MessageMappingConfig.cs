using Mapster;
using Tillegal.Application.Features.Messages.Models;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Messages.Mappings;

public sealed class MessageMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ConversationMessage, MessageDto>();
    }
}
