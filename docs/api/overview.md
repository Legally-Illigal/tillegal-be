# API Overview & Protocol Standards

This document describes the high-level API communication protocol, gateway controls, and operational endpoints of `tillegal-be`.

## Protocol & Conventions

- **Transport**: HTTPS (TLS 1.2+ required in production).
- **Default Format**: UTF-8 encoded JSON (`application/json`).
- **Base Route Pattern**: `/api/[controller]`.
- **OpenAPI / Swagger**:
  - Swagger UI accessible in Development at `/swagger`.
  - Configured with Bearer token authentication support.

## Gateway Policies & Protections

### 1. Reverse Proxy & Forwarded Headers
- ASP.NET Core `ForwardedHeadersMiddleware` is configured to trust `X-Forwarded-For` and `X-Forwarded-Proto`.
- Upstream proxy IPs must be registered under `ReverseProxy:KnownProxies` in production.

### 2. Cross-Origin Resource Sharing (CORS)
- Policy `DefaultCorsPolicy` is applied automatically.
- Configurable origins via `Cors:AllowedOrigins` (defaults to local frontends: `http://localhost:3000`, `http://localhost:5173`, `http://localhost:4200`).
- Supports credentials (`AllowCredentials()`) when explicit origins are defined.

### 3. Rate Limiting
- Distributed fixed-window rate limiter powered by Redis (`RedisRateLimitingMiddleware` via `IRateLimitService` / `UpstashRedisRateLimitService`):
  - **Global Policy** (applied across all `/api/*` endpoints):
    - **Permit Limit**: 100 requests per 1-minute window.
    - **Partition Key**: User ID when authenticated (`global:user:{userId}`), falling back to remote IP address (`global:ip:{clientIp}`).
  - **Login Policy** (applied strictly to `/api/auth/login` and `/api/auth/login/google`):
    - **Permit Limit**: 10 requests per 1-minute window.
    - **Partition Key**: Client IP address (`login:{clientIp}`).
  - **Headers**:
    - `X-RateLimit-Limit`: Maximum requests permitted in the window.
    - `X-RateLimit-Remaining`: Remaining request quota.
    - `X-RateLimit-Reset`: Unix timestamp when the current window resets.
    - `Retry-After`: Seconds to wait before retrying (on HTTP 429).
  - **Rejection Code**: HTTP 429 (`application/problem+json` RFC 7807 `ProblemDetails`).
  - **Fail-open Resiliency**: If Redis is temporarily unreachable, rate limiter logs a warning and permits requests to avoid cascading outages.

## Operational Health Endpoints

Health checks are mapped at root level and bypass authentication:

| Endpoint | Purpose | Scope / Tag | Expected Response |
| :--- | :--- | :--- | :--- |
| `GET /healthz` | Liveness probe | Non-blocking process liveness | `{"status":"Healthy"}` (HTTP 200) |
| `GET /readyz` | Readiness probe | Validates PostgreSQL connectivity (`DatabaseHealthCheck`) | `{"status":"Healthy"}` (HTTP 200) |
