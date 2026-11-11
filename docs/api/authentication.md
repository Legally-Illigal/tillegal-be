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

## Google registration

`POST /api/auth/register/google` accepts a Google ID token issued to the configured frontend OAuth client:

```json
{ "idToken": "<Google ID token>" }
```

The backend verifies the token signature, expiry, issuer, and audience with Google's validation library, and requires a verified email. It creates a passwordless user with the default `User` role, persists the stable Google `sub` claim in `Users.GoogleSubject`, and sets the Tillegal JWT in the `access_token` HttpOnly cookie. The JWT is not included in the JSON response, and the Google ID token itself is not stored. If the email already exists, registration is rejected; existing accounts are not implicitly linked. Invalid or expired Google tokens return HTTP 401; validation and duplicate-account errors return HTTP 400.

`POST /api/auth/login/google` accepts the same request and verifies the new Google ID token. If an account already exists with the matching Google `sub`, the user is logged in immediately. If no account has this Google `sub`, but an existing account matches the verified email without a Google subject, the account is automatically linked to the Google `sub` and logged in. If no account exists for the email, the user is automatically registered as a passwordless user with the default `User` role and logged in immediately without requiring a separate registration step. In all successful login flows, a Tillegal JWT is set in the `access_token` HttpOnly cookie. Both endpoints return user details and a message in JSON, but never return the access token directly in the response body.

Configure `GoogleAuth:ClientId` (environment variable `GoogleAuth__ClientId`) to the OAuth client ID used to obtain the ID token. The Tillegal JWT is signed using `Jwt:SigningKey`, with issuer/audience and expiration from the existing `Jwt` configuration. The database migration `AddGoogleSubjectToUsers` must be applied before using these endpoints.

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
