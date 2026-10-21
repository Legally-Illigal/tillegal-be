# Authentication & Authorization Specification

Tillegal Backend uses standard **JWT (JSON Web Token) Bearer** authentication for securing protected API endpoints.

## Authentication Configuration

JWT validation parameters are configured in `Tillegal.Api/DependencyInjection.cs`:
- **Scheme**: `Bearer` (`JwtBearerDefaults.AuthenticationScheme`).
- **Signing Key**: HMAC-SHA256 symmetric key configured via `Jwt:SigningKey` (default dev key provided for local testing).
- **Issuer / Audience**: Configured via `Jwt:Issuer` and `Jwt:Audience`. Validated if non-empty.
- **Clock Skew**: 30 seconds (`ClockSkew = TimeSpan.FromSeconds(30)`).
- **HTTPS Metadata**: Disabled in Development (`RequireHttpsMetadata = false`).

## Token Header Format

Clients must pass the access token in the `Authorization` header:

```http
Authorization: Bearer <token_string>
```

When missing or invalid, ASP.NET Core returns HTTP 401 (`Unauthorized`).

## Current User Service (`ICurrentUserService`)

The application layer resolves caller identity via `ICurrentUserService` without directly coupling to `HttpContext`:

```csharp
public interface ICurrentUserService
{
    Guid? UserId { get; }
    ClaimsPrincipal? User { get; }
}
```

- **Implementation**: `CurrentUserService` resolves `ClaimTypes.NameIdentifier` or `"sub"` from `IHttpContextAccessor.HttpContext.User`.
- Parses claim string into `Guid?`. If anonymous or malformed, returns `null`.

## Controller Authorization Attributes

- Open endpoints: decorate with `[AllowAnonymous]`.
- Protected endpoints: decorate controller or action with `[Authorize]`.
- Role/Policy-based endpoints: decorate with `[Authorize(Roles = "Admin")]` or `[Authorize(Policy = "...")]`.

## Swagger / OpenAPI Testing

In Swagger UI (`/swagger`):
1. Click the **Authorize** button.
2. Enter token format: `Bearer <your_token>`.
3. Submit and execute authorized API endpoints.
