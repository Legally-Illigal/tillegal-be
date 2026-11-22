using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.Users;
using Tillegal.Application.Features.Users.Commands.ChangeMyPassword;
using Tillegal.Application.Features.Users.Commands.DeactivateMyAccount;
using Tillegal.Application.Features.Users.Commands.UpdateMyProfile;
using Tillegal.Application.Features.Users.Queries.GetMyProfile;

namespace Tillegal.Api.Controllers;

[Authorize]
[Route("api/users")]
public sealed class UsersController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyProfileQuery(), cancellationToken);
        var response = Mapper.Map<UserProfileApiResponse>(result);

        return Ok(response);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateUserProfileApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<UpdateMyProfileCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<UserProfileApiResponse>(result);

        return Ok(response);
    }

    [HttpPut("me/password")]
    [ProducesResponseType(typeof(ChangePasswordApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeMyPasswordCommand(request.NewPassword);
        await Mediator.Send(command, cancellationToken);

        return Ok(new ChangePasswordApiResponse());
    }

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateAccount(CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeactivateMyAccountCommand(), cancellationToken);

        Response.Cookies.Delete("access_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api"
        });

        return NoContent();
    }
}
