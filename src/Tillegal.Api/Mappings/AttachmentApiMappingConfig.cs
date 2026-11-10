using Mapster;
using Tillegal.Api.Contracts.Attachments;
using Tillegal.Application.Features.Attachments.Models;

namespace Tillegal.Api.Mappings;

public sealed class AttachmentApiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AttachmentDto, AttachmentApiResponse>();
    }
}

