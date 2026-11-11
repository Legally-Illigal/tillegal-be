using Mapster;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Admin.Users.Mappings;

public sealed class AdminUserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.RoleName, src => src.Role != null
                ? (string.Equals(src.Role.Name, "Lawer", StringComparison.OrdinalIgnoreCase) ? "Lawyer" : src.Role.Name)
                : null)
            .Map(dest => dest.IsActive, src => src.IsActive);
    }
}
