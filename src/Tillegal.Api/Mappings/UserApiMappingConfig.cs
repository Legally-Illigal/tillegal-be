using Mapster;
using Tillegal.Api.Contracts.Users;
using Tillegal.Application.Features.Users.Commands.UpdateMyProfile;
using Tillegal.Application.Features.Users.Models;

namespace Tillegal.Api.Mappings;

public sealed class UserApiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateUserProfileApiRequest, UpdateMyProfileCommand>();
        config.NewConfig<UserProfileDto, UserProfileApiResponse>();
    }
}
