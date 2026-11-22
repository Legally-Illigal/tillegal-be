using Mapster;
using Tillegal.Application.Features.Users.Models;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Users.Mappings;

public sealed class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserProfileDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
            .Map(dest => dest.Role, src => src.Role != null ? src.Role.Name : null)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.LastActiveAt, src => src.LastActiveAt);
    }
}
