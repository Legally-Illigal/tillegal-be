using Mapster;
using Tillegal.Api.Contracts.Admin;
using Tillegal.Application.Features.Admin.Users.Models;

namespace Tillegal.Api.Mappings;

public sealed class AdminUserApiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserDto, AdminUserApiResponse>();
        config.NewConfig<PagedUsersDto, PagedAdminUsersApiResponse>();
    }
}
