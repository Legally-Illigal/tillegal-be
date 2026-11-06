using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.AiEvaluations;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationById;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluations;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationSummary;

namespace Tillegal.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin/ai-evaluations")]
public sealed class AdminAiEvaluationsController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedAiEvaluationsApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isHelpful = null,
        [FromQuery] int? rating = null,
        [FromQuery] Guid? evaluatorUserId = null,
        [FromQuery] string? evaluatorRole = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminAiEvaluationsQuery(
            pageNumber,
            pageSize,
            isHelpful,
            rating,
            evaluatorUserId,
            evaluatorRole);

        var result = await Mediator.Send(query, cancellationToken);
        var response = Mapper.Map<PagedAiEvaluationsApiResponse>(result);

        return Ok(response);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(AiEvaluationSummaryApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
    {
        var query = new GetAdminAiEvaluationSummaryQuery();
        var result = await Mediator.Send(query, cancellationToken);
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
        var query = new GetAdminAiEvaluationByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        var response = Mapper.Map<AiEvaluationApiResponse>(result);

        return Ok(response);
    }
}

