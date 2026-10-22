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

## Authentication Endpoints

### 1. Đăng nhập hệ thống (Login)

- **Route**: `POST /api/auth/login`
- **Rate Limit**: 100 requests/minute (fixed window policy)
- **Access**: Anonymous (`[AllowAnonymous]`)
- **Request Body**:

```json
{
  "emailOrUserName": "admin_system",
  "password": "P@ssword123!"
}
```

- **Response `200 OK`**:

```json
{
  "accessToken": "eyJhbGciOi...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "20000000-0000-0000-0000-000000000001",
    "fullName": "Quản Trị Viên Hệ Thống",
    "userName": "admin_system",
    "email": "admin.system@tillegal.com",
    "phoneNumber": "0901000001",
    "role": "Admin"
  }
}
```

- **Response `401 Unauthorized`**: Trả về `ProblemDetails` theo chuẩn RFC 7807 khi sai tài khoản hoặc mật khẩu.
- **Response `400 BadRequest`**: Trả về lỗi khi thiếu thông tin đăng nhập (`ValidationException`).
- **Audit Logging**: Khi đăng nhập thành công, hệ thống tự động ghi nhận bản ghi `AuditLog` với `Action = "LOGIN_SUCCESS"`, `EventType = "AUTH_EVENT"`, và cập nhật `LastActiveAt` của người dùng.

### 2. Thông tin người dùng hiện tại (Get Current User)

- **Route**: `GET /api/auth/me`
- **Access**: Yêu cầu xác thực (`[Authorize]`)
- **Headers**: `Authorization: Bearer <access_token>`
- **Response `200 OK`**: Trả về thông tin `UserDto` của người dùng đang đăng nhập.
- **Response `401 Unauthorized`**: Khi token không hợp lệ hoặc hết hạn.

## Controller Authorization Attributes

- Open endpoints: decorate with `[AllowAnonymous]`.
- Protected endpoints: decorate controller or action with `[Authorize]`.
- Role/Policy-based endpoints: decorate with `[Authorize(Roles = "Admin")]` or `[Authorize(Policy = "...")]`.

## Swagger / OpenAPI Testing

In Swagger UI (`/swagger`):
1. Execute `POST /api/auth/login` với tài khoản seeded (ví dụ: `admin_system` / `P@ssword123!`).
2. Copy `accessToken` từ response.
3. Click the **Authorize** button ở góc trên bên phải Swagger UI.
4. Enter token format: `Bearer <your_token>`.
5. Submit và kiểm tra endpoint `GET /api/auth/me`.
