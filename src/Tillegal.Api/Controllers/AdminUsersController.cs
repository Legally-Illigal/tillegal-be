using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.Admin;
using Tillegal.Application.Features.Admin.Users.Commands.GrantLawyerRole;
using Tillegal.Application.Features.Admin.Users.Commands.UpdateUserStatus;
using Tillegal.Application.Features.Admin.Users.Queries.GetUserById;
using Tillegal.Application.Features.Admin.Users.Queries.GetUsers;

namespace Tillegal.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public sealed class AdminUsersController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedAdminUsersApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery(pageNumber, pageSize, searchTerm);
        var result = await Mediator.Send(query, cancellationToken);
        var response = Mapper.Map<PagedAdminUsersApiResponse>(result);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        var response = Mapper.Map<AdminUserApiResponse>(result);

        return Ok(response);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(AdminUserApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateUserStatusApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserStatusCommand(id, request.IsActive);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<AdminUserApiResponse>(result);

        return Ok(response);
    }

    [HttpPut("{id:guid}/grant-lawyer")]
    [ProducesResponseType(typeof(AdminUserApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GrantLawyerRole(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new GrantLawyerRoleCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<AdminUserApiResponse>(result);

        return Ok(response);
    }
}
