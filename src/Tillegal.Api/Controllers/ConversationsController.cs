using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.Conversations;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Application.Features.Conversations.Commands.DeleteConversation;
using Tillegal.Application.Features.Conversations.Commands.UpdateConversation;
using Tillegal.Application.Features.Conversations.Queries.GetConversationById;
using Tillegal.Application.Features.Conversations.Queries.GetConversations;

namespace Tillegal.Api.Controllers;

[Authorize]
public sealed class ConversationsController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpPost]
    [ProducesResponseType(typeof(ConversationApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateConversationApiRequest? request,
        CancellationToken cancellationToken)
    {
        var command = request is not null
            ? Mapper.Map<CreateConversationCommand>(request)
            : new CreateConversationCommand();

        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<ConversationApiResponse>(result);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetConversationsQuery(), cancellationToken);
        var response = Mapper.Map<IReadOnlyList<ConversationApiResponse>>(result);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConversationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetConversationByIdQuery(id), cancellationToken);
        var response = Mapper.Map<ConversationApiResponse>(result);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ConversationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateConversationApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateConversationCommand(id, request.Title);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<ConversationApiResponse>(result);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteConversationCommand(id), cancellationToken);
        return NoContent();
    }
}
