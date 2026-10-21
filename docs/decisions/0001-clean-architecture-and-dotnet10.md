# ADR 0001: Clean Architecture and .NET 10

## Status
Accepted

## Context
Tillegal is a legal-tech backend platform requiring long-term maintainability, testability, and clear separation of business rules from external technologies and cloud infrastructure.

## Decision
1. **Target Runtime**: Adopt **.NET 10** for modern performance optimizations, latest C# features, and native support for Guid version 7.
2. **Architecture**: Implement **Clean Architecture** combined with **Domain-Driven Design (DDD)** principles:
   - `Domain`: Enterprise business rules, entity models, domain exceptions.
   - `Application`: Application business rules, CQRS with MediatR, pipeline behaviors, FluentValidation, Mapster.
   - `Persistence`: PostgreSQL data access with EF Core.
   - `Infrastructure`: External third-party integrations (Upstash Redis, Resend, Supabase, Quartz).
   - `Api`: ASP.NET Core presentation layer.
   - `AppHost`: Executable composition root assembling DI without polluting layer boundaries.
3. **Automated Architectural Enforcement**: Use `NetArchTest.Rules` in `Tillegal.ArchitectureTests` to verify layer dependencies in automated test suites.

## Consequences
- **Positive**:
  - Domain invariants remain isolated and independent of database schemas and HTTP frameworks.
  - High unit test coverage without database or external mock overhead.
  - Modularity simplifies team collaboration and future technology replacements.
- **Negative**:
  - More projects and boilerplate classes (Commands, Queries, DTOs, Mappings) compared to monolithic CRUD APIs.
