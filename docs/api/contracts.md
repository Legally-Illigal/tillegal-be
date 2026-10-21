# API Integration Contracts & Conventions

This document defines conventions for controller implementation, header handling, HTTP status codes, and payload formatting.

## Controller Implementation Pattern

All controllers in `Tillegal.Api` must inherit from `BaseApiController`:

```csharp
namespace Tillegal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DocumentsController(ISender mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDocumentByIdQuery(id), ct);
        return Ok(Mapper.Map<DocumentResponse>(result));
    }
}
```

## Standard Request & Response Headers

| Header | Type | Direction | Description |
| :--- | :--- | :--- | :--- |
| `X-Correlation-ID` | string | Inbound / Outbound | Distributed trace identifier (up to 128 chars, alphanumeric and `-_.`). Echoed in response. |
| `Authorization` | string | Inbound | Bearer token format: `Bearer <jwt_token>`. |
| `Content-Type` | string | Both | `application/json` for requests/success responses; `application/problem+json` for errors. |

## Standard HTTP Status Codes

- **`200 OK`**: Query succeeded or update returned payload.
- **`201 Created`**: Resource successfully created. Returns created resource representation.
- **`204 NoContent`**: Command completed successfully without payload response.
- **`400 BadRequest`**: Request failed input validation or violated a domain invariant.
- **`401 Unauthorized`**: Authentication missing, invalid, or expired.
- **`403 Forbidden`**: Authenticated caller lacks permissions (`ForbiddenAccessException`).
- **`404 NotFound`**: Target resource does not exist (`NotFoundException`).
- **`429 TooManyRequests`**: Client exceeded rate limit (100 req/min).
- **`500 InternalServerError`**: Unhandled system exception.
