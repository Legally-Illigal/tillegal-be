# Tillegal Backend Documentation Knowledge Base

This documentation hub provides architectural, technical, operational, and API specifications for the Tillegal Backend codebase (`tillegal-be`). It is structured for rapid ingestion by AI agents and developers without needing to inspect raw source code.

## Documentation Structure

- [architecture/](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/architecture/) — High-level architecture, layer responsibilities, and dependency rules.
  - [`overview.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/architecture/overview.md): Clean Architecture + DDD core concepts, technology stack, and composition root.
  - [`boundaries.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/architecture/boundaries.md): Dependency rules and NetArchTest enforcement.
  - [`data-flow.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/architecture/data-flow.md): HTTP middleware pipeline, MediatR pipeline behaviors, and transaction lifecycle.

- [decisions/](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/decisions/) — Architecture Decision Records (ADRs).
  - [`0001-clean-architecture-and-dotnet10.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/decisions/0001-clean-architecture-and-dotnet10.md): Adoption of .NET 10, Clean Architecture, MediatR, and Mapster.
  - [`0002-postgresql-efcore-unitofwork.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/decisions/0002-postgresql-efcore-unitofwork.md): PostgreSQL with EF Core, Guid v7, automatic timestamps, and global soft-delete.
  - [`0003-serverless-external-integrations.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/decisions/0003-serverless-external-integrations.md): Upstash Redis REST, Resend email, Supabase Storage, and Quartz.NET.
  - [`0004-api-security-and-error-handling.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/decisions/0004-api-security-and-error-handling.md): JWT bearer authentication, RFC 7807 ProblemDetails, rate limiting, and correlation IDs.

- [development/](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/development/) — Setup, commands, guidelines, and current milestone status.
  - [`setup-and-workflow.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/development/setup-and-workflow.md): Local environment setup, dotnet CLI commands, and database migration/seeding.
  - [`conventions.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/development/conventions.md): Code conventions, entity design patterns, CQRS rules, and testing principles.
  - [`status-and-roadmap.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/development/status-and-roadmap.md): Current implementation state and upcoming modules.
  - [`backend-feature-backlog.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/development/backend-feature-backlog.md): Backend feature inventory, Backend/AI Service responsibilities, and evidence-based feature status.
  - [`agent-worklog.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/development/agent-worklog.md): Concise task progress and handoff history for agents.

- [api/](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/api/) — API contracts and integration protocols.
  - [`overview.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/api/overview.md): Base URL, routing conventions, rate limiting, and health probes (`/healthz`, `/readyz`).
  - [`contracts.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/api/contracts.md): Controller patterns, request/response formats, and headers (`X-Correlation-ID`).
  - [`error-handling.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/api/error-handling.md): Standard error responses (RFC 7807 ProblemDetails) and status code mappings.
  - [`authentication.md`](file:///c:/Users/Asus/Desktop/Tillegal/tillegal-be/docs/api/authentication.md): JWT Bearer authentication scheme, tokens, and current user context.
