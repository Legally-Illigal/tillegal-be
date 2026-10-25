using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.Conversations;
using Tillegal.Application.Features.Conversations.Commands.AskQuestion;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Application.Features.Conversations.Queries.GetConversationMessages;

namespace Tillegal.Api.Controllers;

[Authorize]
public sealed class ConversationsController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateConversationApiResponse), StatusCodes.Status201Created)]
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
        var response = Mapper.Map<CreateConversationApiResponse>(result);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(AskQuestionApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ask(
        [FromRoute] Guid id,
        [FromBody] AskQuestionApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AskQuestionCommand(id, request.Content);

        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<AskQuestionApiResponse>(result);

        return Ok(response);
    }

    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationMessageApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetConversationMessagesQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        var response = Mapper.Map<IReadOnlyList<ConversationMessageApiResponse>>(result);

        return Ok(response);
    }
}
