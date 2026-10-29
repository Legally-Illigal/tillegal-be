using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.Conversations;
using Tillegal.Api.Contracts.Messages;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Application.Features.Conversations.Commands.DeleteConversation;
using Tillegal.Application.Features.Conversations.Commands.UpdateConversation;
using Tillegal.Application.Features.Conversations.Queries.GetConversationById;
using Tillegal.Application.Features.Conversations.Queries.GetConversations;
using Tillegal.Application.Features.Messages.Commands.CreateMessage;
using Tillegal.Application.Features.Messages.Commands.DeleteMessage;
using Tillegal.Application.Features.Messages.Commands.UpdateMessage;
using Tillegal.Application.Features.Messages.Queries.GetConversationMessages;
using Tillegal.Application.Features.Messages.Queries.GetMessageById;

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

    [HttpPost("{conversationId:guid}/messages")]
    [ProducesResponseType(typeof(MessageApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMessage(
        [FromRoute] Guid conversationId,
        [FromBody] CreateMessageApiRequest request,
        CancellationToken cancellationToken)
    {
        var role = string.IsNullOrWhiteSpace(request.Role) ? "user" : request.Role;
        var command = new CreateMessageCommand(
            conversationId,
            role,
            request.Content,
            request.Citations);

        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<MessageApiResponse>(result);

        return CreatedAtAction(
            nameof(GetMessageById),
            new { conversationId, messageId = response.Id },
            response);
    }

    [HttpGet("{conversationId:guid}/messages")]
    [ProducesResponseType(typeof(IReadOnlyList<MessageApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(
        [FromRoute] Guid conversationId,
        CancellationToken cancellationToken)
    {
        var query = new GetConversationMessagesQuery(conversationId);
        var result = await Mediator.Send(query, cancellationToken);
        var response = Mapper.Map<IReadOnlyList<MessageApiResponse>>(result);

        return Ok(response);
    }

    [HttpGet("{conversationId:guid}/messages/{messageId:guid}")]
    [ProducesResponseType(typeof(MessageApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessageById(
        [FromRoute] Guid conversationId,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken)
    {
        var query = new GetMessageByIdQuery(conversationId, messageId);
        var result = await Mediator.Send(query, cancellationToken);
        var response = Mapper.Map<MessageApiResponse>(result);

        return Ok(response);
    }

    [HttpPut("{conversationId:guid}/messages/{messageId:guid}")]
    [ProducesResponseType(typeof(MessageApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMessage(
        [FromRoute] Guid conversationId,
        [FromRoute] Guid messageId,
        [FromBody] UpdateMessageApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMessageCommand(
            conversationId,
            messageId,
            request.Content,
            request.Citations);

        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<MessageApiResponse>(result);

        return Ok(response);
    }

    [HttpDelete("{conversationId:guid}/messages/{messageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMessage(
        [FromRoute] Guid conversationId,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMessageCommand(conversationId, messageId);
        await Mediator.Send(command, cancellationToken);

        return NoContent();
    }
}

