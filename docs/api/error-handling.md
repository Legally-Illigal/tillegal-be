# API Error Handling Specification

All API errors return RFC 7807 **ProblemDetails** payloads with HTTP content type `application/problem+json`, produced centrally by `ExceptionHandlingMiddleware`.

## ProblemDetails Schema

```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "detail": "Review the errors and try again.",
  "instance": "/api/documents",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
  "errors": {
    "Title": ["'Title' must not be empty."]
  }
}
```

## Exception Mappings

| Source Exception | HTTP Status | Response Title | Extensions |
| :--- | :--- | :--- | :--- |
| `ValidationException` | `400 BadRequest` | "One or more validation errors occurred." | `errors`: dictionary of property names to array of error strings. |
| `DomainException` | `400 BadRequest` | "The request violates a business rule." | `detail` contains domain invariant message. |
| `NotFoundException` | `404 NotFound` | "Resource not found." | `detail` contains message (e.g. entity name + ID). |
| `ForbiddenAccessException` | `403 Forbidden` | "Access denied." | `detail` contains access denial explanation. |
| Unhandled `Exception` | `500 InternalServerError`| "An unexpected error occurred." | `detail` included only in Development environment. |

## Client Integration Guidelines

1. **Check Content-Type**: When receiving HTTP 4xx or 5xx, inspect `Content-Type: application/problem+json`.
2. **Form Validation Feedback**: In UI forms, inspect the `errors` property on 400 responses to map field errors directly to input fields:
   ```typescript
   if (response.status === 400 && data.errors) {
     Object.entries(data.errors).forEach(([field, messages]) => {
       setFieldError(field.toLowerCase(), messages[0]);
     });
   }
   ```
3. **Trace Support**: Include `traceId` (or the `X-Correlation-ID` header) when logging errors or contacting support.
