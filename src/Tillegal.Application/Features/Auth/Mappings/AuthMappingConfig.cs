using Mapster;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Auth.Mappings;

public sealed class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, VerifyRegistrationResult>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Message, _ => "Account registration and verification completed successfully.");
    }
}
