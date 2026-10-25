using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.Auth;
using Tillegal.Application.Features.Auth.Commands.Login;
using Tillegal.Application.Features.Auth.Commands.Register;
using Tillegal.Application.Features.Auth.Commands.LoginWithGoogle;
using Tillegal.Application.Features.Auth.Commands.RegisterWithGoogle;
using Tillegal.Application.Features.Auth.Commands.ResendOtp;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;

namespace Tillegal.Api.Controllers;

[AllowAnonymous]
public sealed class AuthController(ISender mediator, IMapper mapper) : BaseApiController(mediator, mapper)
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<LoginCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        SetAccessTokenCookie(result.AccessToken, DateTimeOffset.UtcNow.AddSeconds(result.ExpiresIn));

        var response = Mapper.Map<LoginApiResponse>(result);
        return Ok(response);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<RegisterCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<RegisterApiResponse>(result);

        return Ok(response);
    }

    [HttpPost("register/google")]
    [ProducesResponseType(typeof(GoogleRegistrationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterWithGoogle(
        [FromBody] GoogleRegistrationApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<RegisterWithGoogleCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        SetAccessTokenCookie(result.AccessToken, result.ExpiresAt);
        var response = Mapper.Map<GoogleRegistrationApiResponse>(result);

        return Ok(response);
    }

    [HttpPost("login/google")]
    [ProducesResponseType(typeof(GoogleRegistrationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginWithGoogle(
        [FromBody] GoogleRegistrationApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<LoginWithGoogleCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        SetAccessTokenCookie(result.AccessToken, result.ExpiresAt);
        var response = Mapper.Map<GoogleRegistrationApiResponse>(result);

        return Ok(response);
    }

    [HttpPost("verify-registration")]
    [ProducesResponseType(typeof(VerifyRegistrationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyRegistration(
        [FromBody] VerifyRegistrationApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<VerifyRegistrationCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<VerifyRegistrationApiResponse>(result);

        return Ok(response);
    }

    private void SetAccessTokenCookie(string accessToken, DateTimeOffset expiresAt)
    {
        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt
        });
    }

    [HttpPost("resend-otp")]
    [ProducesResponseType(typeof(ResendOtpApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendOtp(
        [FromBody] ResendOtpApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<ResendOtpCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<ResendOtpApiResponse>(result);

        return Ok(response);
    }
}
