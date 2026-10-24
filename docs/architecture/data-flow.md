# Request Lifecycle & Data Flow

This document details the lifecycle of an HTTP request traversing through the middleware, presentation, application, domain, and persistence layers.

## Request Pipeline Flow

```text
HTTP Client
   │
   ▼
1. Middlewares:
   ├── ForwardedHeaders & CORS
   ├── CorrelationIdMiddleware (Generates or propagates X-Correlation-ID)
   ├── ExceptionHandlingMiddleware (Catches exceptions -> RFC 7807 ProblemDetails)
   ├── RequestLoggingMiddleware (Logs method, route, duration, status)
   ├── RateLimiter (100 req/min per IP partition)
   └── Authentication & Authorization (JWT Bearer)
   │
   ▼
2. Presentation (`Tillegal.Api`):
   └── Controller inherits `BaseApiController`
       └── Maps incoming API request contract to MediatR command/query via Mapster (`IMapper`)
       └── Dispatches through `ISender.Send`
   │
   ▼
3. MediatR pipeline (`Tillegal.Application`):
   ├── `LoggingBehavior<TRequest, TResponse>` logs execution time and errors
   ├── `ValidationBehavior<TRequest, TResponse>` runs FluentValidation validators
   │     └── Throws `ValidationException` when validation fails
   └── Request handler (`IRequestHandler<TRequest, TResponse>`)
         ├── Orchestrates the application use case
         ├── Calls domain services for domain rules; it does not use EF Core directly
         ├── Uses Mapster (`IMapper`) to project Domain entities to Application Result DTOs
         └── Uses application ports for technical services and persistence coordination
   │
   ▼
4. Domain (`Tillegal.Domain`):
   └── Domain services enforce domain policies and call domain repository contracts
       └── Repository contracts are declared in `Domain`; domain code has no EF Core or Mapster dependency
   │
   ▼
5. Persistence (`Tillegal.Persistence`):
   └── Implements domain repository contracts and uses `AppDbContext` / EF Core
       ├── Maps database data models to domain entities
       ├── Applies the global soft-delete query filter (`DeletedAt == null`)
       └── Executes SQL transactions against PostgreSQL
   │
   ▼
6. Response:
   └── Handler returns result DTO -> Controller maps result DTO to API response contract via Mapster (`IMapper`) -> Returns `ActionResult` -> HTTP response
```

## Login Request Flow (Target)

The following describes the intended `/api/auth/login` flow. It is an architectural guide, not a claim that the login endpoint and all of its classes are already implemented in the current codebase.

1. `AuthController` receives `POST /api/auth/login` with `LoginRequest` (`Email`, `Password`), maps it to `LoginCommand`, and sends it through MediatR.
2. `ValidationBehavior` runs `LoginCommandValidator`. Invalid input raises `ValidationException` and follows the standard exception-handling path; valid input continues to the handler.
3. `LoginCommandHandler` calls the Domain `AuthService` to look up the user by email.
4. `AuthService` calls the `IAuthRepository` contract declared in `Tillegal.Domain`. The repository implementation belongs in `Tillegal.Persistence` and uses EF Core to load/map the user and role name. The service applies login eligibility rules (for example: user exists, is not deleted, email is verified, and account is active) and returns the user and role name or a failed lookup result.
5. The handler verifies the supplied password against the stored hash through an application-owned password-hashing port (for example, `IPasswordHashingService`), implemented by `Tillegal.Infrastructure`. A failed verification returns the login failure result.
6. On success, the handler requests JWT creation through an application-owned token-generation port; the concrete JWT implementation belongs in `Tillegal.Infrastructure`. The handler creates and returns `LoginResultDto` through MediatR.
7. `AuthController` returns `401 Unauthorized` for a failed login. On success, it writes the JWT to the `rescuehub_token` cookie, maps `LoginResultDto` to `LoginResponse`, and returns `200 OK`. The client stores the cookie according to its configured attributes.

**Placement rule:** `Application` handlers call domain services; they must not define an `IAuthRepository` for this domain use case or depend on `Tillegal.Persistence`. Put the repository contract in `Domain` and its EF Core implementation in `Persistence`. Interfaces for technical services called by Application (such as password hashing or JWT generation) remain application ports, with implementations in Infrastructure.

The current repository has the generic API, MediatR, validation, persistence, and password-hashing foundations, but does not yet contain the login controller/use case, domain authentication service/repository, or JWT-generation service. Also, the existing API authentication documentation describes Bearer-token input; writing a JWT to a cookie alone does not configure the current authentication handler to read tokens from that cookie.

## Transaction Management

- Operations mutating multiple aggregates or entities should utilize `IUnitOfWork`:
  - `BeginTransactionAsync(ct)`
  - `CommitTransactionAsync(ct)`
  - `RollbackTransactionAsync(ct)`
- `IUnitOfWork.SaveChangesAsync(ct)` automatically tracks modified entities deriving from `BaseEntity` and updates `UpdatedAt`.
