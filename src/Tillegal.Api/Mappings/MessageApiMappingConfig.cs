using Mapster;
using Tillegal.Api.Contracts.Messages;
using Tillegal.Application.Features.Messages.Models;

namespace Tillegal.Api.Mappings;

public sealed class MessageApiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MessageDto, MessageApiResponse>();
    }
}
