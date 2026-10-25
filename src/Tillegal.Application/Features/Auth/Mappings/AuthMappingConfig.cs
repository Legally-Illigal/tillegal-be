using Mapster;
using Tillegal.Application.Features.Auth.Commands.Login;
using Tillegal.Application.Features.Auth.Commands.LoginWithGoogle;
using Tillegal.Application.Features.Auth.Commands.RegisterWithGoogle;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Auth.Mappings;

public sealed class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LoginResultMappingSource, LoginResult>()
            .Map(dest => dest.AccessToken, src => src.Token.Token)
            .Map(dest => dest.TokenType, _ => "Bearer")
            .Map(dest => dest.ExpiresIn, src => src.Token.ExpiresInSeconds)
            .Map(dest => dest.User, src => src.User);

        config.NewConfig<GoogleAuthResultMappingSource, GoogleRegistrationResult>()
            .Map(dest => dest.UserId, src => src.User.Id)
            .Map(dest => dest.Email, src => src.User.Email ?? string.Empty)
            .Map(dest => dest.FullName, src => src.User.FullName)
            .Map(dest => dest.Message, src => src.Message)
            .Map(dest => dest.AccessToken, src => src.Token.AccessToken)
            .Map(dest => dest.ExpiresAt, src => src.Token.ExpiresAt);

        config.NewConfig<GoogleAuthResultMappingSource, GoogleLoginResult>()
            .Map(dest => dest.UserId, src => src.User.Id)
            .Map(dest => dest.Email, src => src.User.Email ?? string.Empty)
            .Map(dest => dest.FullName, src => src.User.FullName)
            .Map(dest => dest.Message, src => src.Message)
            .Map(dest => dest.AccessToken, src => src.Token.AccessToken)
            .Map(dest => dest.ExpiresAt, src => src.Token.ExpiresAt);

        config.NewConfig<User, VerifyRegistrationResult>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Message, _ => "Account registration and verification completed successfully.");
    }
}
