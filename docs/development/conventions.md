# Coding Conventions & Design Rules

This document outlines implementation patterns, conventions, and design standards expected throughout `tillegal-be`.

## Code Style & Language Features

- **C# Modern Features**: Use file-scoped namespaces, primary constructors, target-typed `new`, pattern matching, and nullable reference types (`<Nullable>enable</Nullable>`).
- **Formatting**: Enforce `.editorconfig` rules. Code must pass `dotnet format --verify-no-changes`.
- **Async/Await**: Always pass `CancellationToken` to all asynchronous operations. Always append `Async` suffix to asynchronous methods.

## Domain Modeling Rules

- **Entity Construction**:
  - All domain entities must inherit from `BaseEntity`.
  - Primary key `Id` defaults to `Guid.CreateVersion7()`.
  - Properties must have `private set` or `protected set` to prevent external property bag tampering.
  - State mutations must occur through expressive domain methods (e.g. `entity.UpdateDetails(...)`, `entity.SoftDelete()`).
- **Domain Invariants**:
  - Enforce business invariants inside domain entities.
  - Throw `DomainException` when invariants are breached. Never throw generic `Exception`.

## CQRS & Application Layer Conventions

- **Use Case Organization**:
  - Group features by capability or aggregate (e.g., `Features/{AggregateName}/Commands/{CommandName}/`).
  - Structure each use case with dedicated request, response, handler, and validator:
    - `{Verb}{Aggregate}Command : IRequest<{ResultDto}>`
    - `{Verb}{Aggregate}CommandHandler : IRequestHandler<{Command}, {ResultDto}>`
    - `{Verb}{Aggregate}CommandValidator : AbstractValidator<{Command}>`
- **Validation**:
  - All commands and queries must have accompanying FluentValidation rules where inputs require validation.
  - Validation failures are automatically caught by `ValidationBehavior` and converted to `ValidationException`.
- **Data Mapping Conventions (Mapster)**:
  - **API Layer <-> Application Layer**:
    - Controllers inheriting from `BaseApiController` must utilize `Mapper` (`IMapper`) for object transformations rather than manual instantiations.
    - Incoming: Transform API Request contract to MediatR Command/Query (`Mapper.Map<SomeCommand>(request)`).
    - Outgoing: Transform MediatR Result DTO to API Response contract (`Mapper.Map<SomeApiResponse>(result)`).
    - API-level mapping configurations implementing `IRegister` belong in `Tillegal.Api/Mappings/{Feature}ApiMappingConfig.cs`.
  - **Application Layer <-> Domain Layer**:
    - Application Handlers inject `IMapper mapper` to project Domain Entities/Models to Application Result DTOs (`mapper.Map<SomeResultDto>(entity)`).
    - **Strict Boundary Rule**: `Tillegal.Domain` must **NEVER** reference Mapster or any external mapping library. Domain entities preserve their own encapsulation, business rules, and constructors.
    - All configurations mapping between Domain Entities and Application DTOs belong strictly inside `Tillegal.Application/Features/{Feature}/Mappings/{Feature}MappingConfig.cs` implementing `IRegister`.
  - **Auto-Discovery via `IRegister`**:
    - DI setup calls `configuration.Scan(...)` for assemblies (`Tillegal.Application` and `Tillegal.Api`). Any class implementing `IRegister` is automatically registered.
  - **Feature Implementation Checklist**:
    1. Define API Request & Response contracts in `Tillegal.Api/Contracts/{Feature}/`.
    2. Define CQRS Command/Query, Validator, and Result DTOs in `Tillegal.Application/Features/{Feature}/...`.
    3. Configure API <-> Application mappings via `IRegister` in `Tillegal.Api/Mappings/{Feature}ApiMappingConfig.cs`.
    4. In Controller, map incoming request to command/query and outgoing result to response using `Mapper.Map<T>()`.
    5. In Application Handler, inject `IMapper` and map Domain entities to Result DTOs; define domain-to-DTO mappings via `IRegister` in `Tillegal.Application/Features/{Feature}/Mappings/{Feature}MappingConfig.cs`.

## Domain Services & Repository Contracts

- Put domain services and domain-owned repository contracts in `Tillegal.Domain`. A domain service applies domain rules and calls a repository contract; it must not reference EF Core or an outer project.
- Declare domain repository contracts under `Domain/Interfaces/Repositories` and implement them in `Tillegal.Persistence/Repositories` using EF Core. Keep persistence mapping between database models and domain entities inside Persistence.
- Application handlers orchestrate commands/queries by calling Domain services; do not define an Application repository interface for a repository that belongs to the domain or access `AppDbContext` directly.
- Keep ports for technical capabilities used directly by Application handlers (for example, password hashing and JWT generation) in Application; implement those ports in Infrastructure.

## Error Handling & Exceptions

- Use explicit domain/application exceptions rather than returning ad-hoc error codes:
  - `NotFoundException`: Entity or resource does not exist.
  - `ForbiddenAccessException`: Current user lacks necessary authorization.
  - `ValidationException`: Input payload violates validation rules.
  - `DomainException`: Action violates core business rules.

## Testing Conventions

- Place unit tests in `tests/Tillegal.Application.UnitTests/`.
- Test names should follow: `UnitOfWork_StateUnderTest_ExpectedBehavior` (e.g., `Constructor_Should_Initialize_ValidGuidV7_And_CreatedAt`).
- Use **xUnit** assertions via **FluentAssertions**.
- Verify architectural boundaries continuously in `tests/Tillegal.ArchitectureTests/`.
