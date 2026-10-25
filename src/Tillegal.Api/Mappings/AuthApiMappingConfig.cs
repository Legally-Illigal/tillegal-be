using Mapster;
using Tillegal.Api.Contracts.Auth;
using Tillegal.Application.Features.Auth.Commands.Login;
using Tillegal.Application.Features.Auth.Commands.Register;
using Tillegal.Application.Features.Auth.Commands.LoginWithGoogle;
using Tillegal.Application.Features.Auth.Commands.RegisterWithGoogle;
using Tillegal.Application.Features.Auth.Commands.ResendOtp;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;
using Tillegal.Domain.Entities;

namespace Tillegal.Api.Mappings;

public sealed class AuthApiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterApiRequest, RegisterCommand>();
        config.NewConfig<RegisterResult, RegisterApiResponse>();

        config.NewConfig<GoogleRegistrationApiRequest, RegisterWithGoogleCommand>();
        config.NewConfig<GoogleRegistrationResult, GoogleRegistrationApiResponse>();
        config.NewConfig<GoogleRegistrationApiRequest, LoginWithGoogleCommand>();
        config.NewConfig<GoogleLoginResult, GoogleRegistrationApiResponse>();

        config.NewConfig<VerifyRegistrationApiRequest, VerifyRegistrationCommand>();
        config.NewConfig<VerifyRegistrationResult, VerifyRegistrationApiResponse>();

        config.NewConfig<ResendOtpApiRequest, ResendOtpCommand>();
        config.NewConfig<ResendOtpResult, ResendOtpApiResponse>();

        config.NewConfig<LoginApiRequest, LoginCommand>();
        config.NewConfig<User, LoginUserApiResponse>()
            .Map(dest => dest.Role, src => src.Role != null ? src.Role.Name : null);
        config.NewConfig<LoginResult, LoginApiResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.Message, _ => "Login successful.");
    }
}
