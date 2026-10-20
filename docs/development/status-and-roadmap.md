# Implementation Status & Roadmap

This document outlines the current technical readiness of `tillegal-be` and pending implementation milestones.

## Current Baseline Status (Completed)

The core architectural scaffold and cross-cutting infrastructure are fully implemented, verified, and test-covered.

| Component / Layer | Status | Description |
| :--- | :--- | :--- |
| **Solution & Architecture** | Complete | .NET 10 solution (`tillegal-be.slnx`) with strict Clean Architecture separation. |
| **Domain Foundation** | Complete | `BaseEntity` (Guid v7, UTC timestamps, soft-delete), `IAggregateRoot`, `DomainException`. |
| **Application Foundation** | Complete | MediatR pipeline behaviors (`LoggingBehavior`, `ValidationBehavior`), FluentValidation, Mapster mapper. |
| **Persistence Layer** | Complete | `AppDbContext` (PostgreSQL via EF Core), `IUnitOfWork`, soft-delete query filter, migration/seeder bootstrapper. |
| **Infrastructure Adapters**| Complete | REST clients for Upstash Redis, Resend email, Supabase Storage; Quartz background job scheduler; `PasswordHashingService`, `DateTimeProvider`. |
| **Presentation & API** | Complete | `BaseApiController`, `CorrelationIdMiddleware`, `ExceptionHandlingMiddleware` (RFC 7807), rate limiting (100 req/min/IP), JWT Bearer auth, Swagger UI. |
| **Health Probes** | Complete | `/healthz` (liveness) and `/readyz` (PostgreSQL readiness). |
| **Automated Testing** | Complete | Unit tests for domain entities and behaviors; NetArchTest architecture guard tests. |

## Pending Work & Roadmap

1. **Business Domain Features**:
   - Define concrete domain aggregates, entities, and value objects according to legal product requirements.
   - Implement EF Core `IEntityTypeConfiguration<T>` mappings in `Tillegal.Persistence`.
2. **CQRS Use Cases**:
   - Implement feature Commands and Queries with FluentValidation validators in `Tillegal.Application`.
   - Expose feature endpoints via controllers inheriting `BaseApiController` in `Tillegal.Api`.
3. **Integration & E2E Testing**:
   - Introduce integration tests using `WebApplicationFactory<Program>` and `Testcontainers` for PostgreSQL.
4. **CI/CD Pipeline**:
   - Establish GitHub Actions workflows for continuous build, format verification, and automated test execution.
