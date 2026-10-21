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
2. Presentation (Tillegal.Api):
   └── Controller inherits BaseApiController
       └── Maps incoming DTO to MediatR Command/Query via Mapster IMapper
       └── Dispatches via ISender (Mediator.Send)
   │
   ▼
3. MediatR Pipeline (Tillegal.Application):
   ├── LoggingBehavior<TRequest, TResponse> (Logs execution time and errors)
   ├── ValidationBehavior<TRequest, TResponse> (Executes FluentValidation validators)
   │     └── Throws ValidationException if validation fails
   └── Request Handler (IRequestHandler<TRequest, TResponse>)
         ├── Evaluates domain rules / calls entity methods
         ├── Mutates state via repositories or AppDbContext
         └── Persists state via IUnitOfWork.SaveChangesAsync
   │
   ▼
4. Persistence (Tillegal.Persistence):
   └── AppDbContext:
       ├── Intercepts ChangeTracker to set UpdatedAt = DateTimeOffset.UtcNow
       ├── Enforces global soft-delete query filter (DeletedAt == null)
       └── Executes SQL transaction against PostgreSQL
   │
   ▼
5. Response:
   └── Handler returns result DTO -> Controller returns ActionResult -> HTTP 200/201/204
```

## Transaction Management

- Operations mutating multiple aggregates or entities should utilize `IUnitOfWork`:
  - `BeginTransactionAsync(ct)`
  - `CommitTransactionAsync(ct)`
  - `RollbackTransactionAsync(ct)`
- `IUnitOfWork.SaveChangesAsync(ct)` automatically tracks modified entities deriving from `BaseEntity` and updates `UpdatedAt`.
