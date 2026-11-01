using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.AiEvaluations;
using Tillegal.Application.Features.AiEvaluations.Commands.CreateEvaluation;
using Tillegal.Application.Features.AiEvaluations.Commands.DeleteEvaluation;
using Tillegal.Application.Features.AiEvaluations.Commands.UpdateEvaluation;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluations;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluationSummary;
using Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationById;
using Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationByMessageId;

namespace Tillegal.Api.Controllers;

[Authorize]
[Route("api/ai-evaluations")]
public sealed class AiEvaluationsController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpPost]
    [ProducesResponseType(typeof(AiEvaluationApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAiEvaluationApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<CreateEvaluationCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<AiEvaluationApiResponse>(result);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedAiEvaluationsApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isHelpful = null,
        [FromQuery] int? rating = null,
        [FromQuery] Guid? evaluatorUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAiEvaluationsQuery(pageNumber, pageSize, isHelpful, rating, evaluatorUserId);
        var result = await Mediator.Send(query, cancellationToken);
        var response = Mapper.Map<PagedAiEvaluationsApiResponse>(result);

        return Ok(response);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(AiEvaluationSummaryApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAiEvaluationSummaryQuery(), cancellationToken);
        var response = Mapper.Map<AiEvaluationSummaryApiResponse>(result);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AiEvaluationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetEvaluationByIdQuery(id), cancellationToken);
        var response = Mapper.Map<AiEvaluationApiResponse>(result);

        return Ok(response);
    }

    [HttpGet("message/{messageId:guid}")]
    [ProducesResponseType(typeof(AiEvaluationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByMessageId(
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetEvaluationByMessageIdQuery(messageId), cancellationToken);
        var response = Mapper.Map<AiEvaluationApiResponse>(result);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AiEvaluationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateAiEvaluationApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEvaluationCommand(id, request.IsHelpful, request.Rating, request.FeedbackComment);
        var result = await Mediator.Send(command, cancellationToken);
        var response = Mapper.Map<AiEvaluationApiResponse>(result);

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
        await Mediator.Send(new DeleteEvaluationCommand(id), cancellationToken);
        return NoContent();
    }
}

