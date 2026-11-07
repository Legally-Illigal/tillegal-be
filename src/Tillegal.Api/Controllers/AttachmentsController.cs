using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tillegal.Api.Contracts.Attachments;
using Tillegal.Application.Features.Attachments.Queries.GetUserAttachments;

namespace Tillegal.Api.Controllers;

[Authorize]
[Route("api/attachments")]
public sealed class AttachmentsController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AttachmentApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUserAttachments(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserAttachmentsQuery(), cancellationToken);
        var response = Mapper.Map<IReadOnlyList<AttachmentApiResponse>>(result);

        return Ok(response);
    }
}

