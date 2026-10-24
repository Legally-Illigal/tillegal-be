# Architectural Boundaries & Dependency Rules

Tillegal Backend strictly enforces the Dependency Inversion Principle and Clean Architecture boundaries. All layer relationships are validated continuously in automated tests (`Tillegal.ArchitectureTests`).

## Dependency Graph

```text
                  ┌──────────────────────┐
                  │   Tillegal.AppHost   │ (Composition Root)
                  └──────────┬───────────┘
         ┌───────────────────┼───────────────────┐
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  Tillegal.Api   │ │Tillegal.Persist.│ │Tillegal.Infrast.│
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             ▼
                  ┌──────────────────────┐
                  │ Tillegal.Application │
                  └──────────┬───────────┘
                             ▼
                  ┌──────────────────────┐
                  │   Tillegal.Domain    │
                  └──────────────────────┘
```

## Strict Boundary Rules

1. **Domain Isolation**:
   - `Domain` must not reference any other project (`Application`, `Persistence`, `Infrastructure`, `Api`, `AppHost`).
   - Domain logic must remain framework-free and database-agnostic.

2. **Application Layer Rules**:
   - `Application` depends exclusively on `Domain`.
   - Forbidden to reference `Persistence`, `Infrastructure`, or `Api`.
   - Defines ports for technical services used by application handlers (for example, password hashing, JWT generation, and `IUnitOfWork`) without knowing their implementations.
   - Does not own repository contracts for domain services or aggregates; those contracts are declared in `Domain`.

3. **Domain Service and Repository Ownership**:
   - Domain services live in `Domain` and may depend only on domain entities/value objects and repository contracts declared in `Domain`.
   - Domain repository contracts belong under `Domain/Interfaces/Repositories`; their EF Core implementations belong in `Persistence/Repositories`.
   - Application handlers orchestrate the use case by calling domain services. They must not bypass those services by calling EF Core or a concrete repository.

4. **Adapter Isolation**:
   - `Persistence` may only reference `Domain` and `Application`.
   - `Persistence` must NOT reference `Infrastructure` or `Api`.
   - `Infrastructure` may only reference `Application` (and `Domain` transitively).
   - `Infrastructure` must NOT reference `Persistence` or `Api`.
   - `Api` may only reference `Application`.
   - `Api` must NOT directly reference `Infrastructure` or `Persistence`.

5. **Single Composition Root**:
   - `AppHost` is the sole assembly authorized to reference all layers in order to register services into the DI container (`src/Tillegal.AppHost/Configurations/`).

## Verification via NetArchTest

Automated tests in `tests/Tillegal.ArchitectureTests/CleanArchitectureTests.cs` fail the build if any illegal project reference or namespace usage is introduced:
- `Domain_ShouldNot_HaveDependencyOnOtherProjects`
- `Application_ShouldNot_HaveDependencyOn_OuterLayers`
- `Infrastructure_ShouldNot_HaveDependencyOn_PersistenceOrApi`
- `Persistence_ShouldNot_HaveDependencyOn_InfrastructureOrApi`
