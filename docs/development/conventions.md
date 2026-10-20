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
- **Data Mapping**:
  - Configure object transformations using **Mapster** in `MappingConfiguration.cs` or feature-specific configs. Inject `IMapper` where needed.

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
