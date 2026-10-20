# ADR 0004: API Security, Observability, and Error Handling

## Status
Accepted

## Context
A production-ready Web API requires consistent error responses, robust security controls (rate limiting, reverse proxy handling, JWT authentication), and end-to-end distributed tracing.

## Decision
1. **Error Contract (RFC 7807 ProblemDetails)**:
   - `ExceptionHandlingMiddleware` intercepts all unhandled exceptions:
     - `ValidationException` -> `400 Bad Request` with structured `errors` dictionary grouped by property name.
     - `DomainException` -> `400 Bad Request` with business violation detail.
     - `NotFoundException` -> `404 Not Found`.
     - `ForbiddenAccessException` -> `403 Forbidden`.
     - Generic `Exception` -> `500 Internal Server Error` (redacting internal messages in non-Development environments).
   - Responses set content type `application/problem+json` and attach `traceId` extension and `X-Correlation-ID` header.
2. **Correlation ID & Tracing**:
   - `CorrelationIdMiddleware` extracts or generates `X-Correlation-ID` (validated up to 128 chars, alphanumeric plus `-_.`).
   - Starts or augments a W3C `Activity`, sets `context.TraceIdentifier`, and returns `X-Correlation-ID` in HTTP response headers.
3. **Authentication & Authorization**:
   - Standard ASP.NET Core JWT Bearer authentication (`JwtBearerDefaults.AuthenticationScheme`).
   - Validates symmetric signing key, lifetime, issuer, audience, with 30s clock skew.
   - `ICurrentUserService` / `CurrentUserService` resolves `UserId` and `ClaimsPrincipal` from `IHttpContextAccessor`.
4. **Rate Limiting & Network Security**:
   - ASP.NET Core built-in Rate Limiter using a fixed-window partition (100 requests / 1 minute per remote IP; rejects with HTTP 429).
   - `ForwardedHeadersOptions` processes `X-Forwarded-For` and `X-Forwarded-Proto` with a configured trusted proxy list (`ReverseProxy:KnownProxies`).
   - CORS policy (`DefaultCorsPolicy`) allows configured origins or defaults to local developer frontends (`:3000`, `:5173`, `:4200`).

## Consequences
- **Positive**:
  - Consistent API client consumption experience with standardized error schemas.
  - Full request traceability across frontend, API layer, and outbound HTTP calls.
  - Built-in brute-force / DoS protection at the presentation gateway.
- **Negative**:
  - All external callers must follow RFC 7807 error parsing contracts.
