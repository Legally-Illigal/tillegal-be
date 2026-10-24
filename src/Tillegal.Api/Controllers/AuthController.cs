using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.Auth;
using Tillegal.Application.Features.Auth.Commands.Register;
using Tillegal.Application.Features.Auth.Commands.ResendOtp;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;

namespace Tillegal.Api.Controllers;

[AllowAnonymous]
public sealed class AuthController(ISender mediator, IMapper mapper) : BaseApiController(mediator, mapper)
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            Email: request.Email,
            Password: request.Password,
            FullName: request.FullName,
            PhoneNumber: request.PhoneNumber);

        var result = await Mediator.Send(command, cancellationToken);
        var response = new RegisterApiResponse(result.Message, result.ExpirationMinutes);

        return Ok(response);
    }

    [HttpPost("verify-registration")]
    [ProducesResponseType(typeof(VerifyRegistrationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyRegistration(
        [FromBody] VerifyRegistrationApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new VerifyRegistrationCommand(request.Email, request.Otp);
        var result = await Mediator.Send(command, cancellationToken);
        var response = new VerifyRegistrationApiResponse(result.UserId, result.Email, result.FullName, result.Message);

        return Ok(response);
    }

    [HttpPost("resend-otp")]
    [ProducesResponseType(typeof(ResendOtpApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendOtp(
        [FromBody] ResendOtpApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResendOtpCommand(request.Email);
        var result = await Mediator.Send(command, cancellationToken);
        var response = new ResendOtpApiResponse(result.Message, result.ExpirationMinutes);

        return Ok(response);
    }
}
