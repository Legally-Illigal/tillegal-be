using Mapster;
using Tillegal.Application.Features.Attachments.Models;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Attachments.Mappings;

public sealed class AttachmentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserAttachment, AttachmentDto>();
    }
}

