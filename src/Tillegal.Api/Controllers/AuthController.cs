using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tillegal.Api.Contracts.Requests;
using Tillegal.Application.Features.Auth.Commands.Login;
using Tillegal.Application.Features.Auth.Queries.GetCurrentUser;

namespace Tillegal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public sealed class AuthController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    /// <summary>
    /// Authenticates a user with email/username and password, returning a JWT Bearer access token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new LoginCommand(request.EmailOrUserName, request.Password, ipAddress);
        var response = await Mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves the profile of the currently authenticated user based on the JWT Bearer token.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(response);
    }
}
