using Mapster;
using Tillegal.Api.Contracts.Auth;
using Tillegal.Application.Features.Auth.Commands.Login;
using Tillegal.Application.Features.Auth.Commands.Register;
using Tillegal.Application.Features.Auth.Commands.ResendOtp;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;

namespace Tillegal.Api.Mappings;

public sealed class AuthApiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterApiRequest, RegisterCommand>();
        config.NewConfig<RegisterResult, RegisterApiResponse>();

        config.NewConfig<VerifyRegistrationApiRequest, VerifyRegistrationCommand>();
        config.NewConfig<VerifyRegistrationResult, VerifyRegistrationApiResponse>();

        config.NewConfig<ResendOtpApiRequest, ResendOtpCommand>();
        config.NewConfig<ResendOtpResult, ResendOtpApiResponse>();

        config.NewConfig<LoginApiRequest, LoginCommand>();
        config.NewConfig<LoginResult, LoginApiResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.Message, _ => "Login successful.");
        config.NewConfig<LoginUserDto, LoginUserApiResponse>();
    }
}
