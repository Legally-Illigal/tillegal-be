# Architecture Overview

Tillegal Backend is built on **.NET 10** using **Clean Architecture** and **Domain-Driven Design (DDD)** principles.

## Core Architectural Layers

1. **`Tillegal.Domain` (Enterprise Business Rules)**
   - Innermost core, completely independent of frameworks and outer layers.
   - Contains domain entities deriving from `BaseEntity` (utilizing Guid v7), `IAggregateRoot` markers, and domain exceptions (`DomainException`).
   - Pure business invariants and state mutation methods.

2. **`Tillegal.Application` (Application Business Rules)**
   - Orchestrates use cases via CQRS with **MediatR**.
   - Contains Commands, Queries, Handlers, FluentValidation validators, and Mapster mapping configurations.
   - Cross-cutting pipeline behaviors: `LoggingBehavior<TRequest, TResponse>`, `ValidationBehavior<TRequest, TResponse>`.
   - Defines interfaces for outer ports: `IUnitOfWork`, `ICacheService`, `IEmailService`, `IStorageService`, `ICurrentUserService`, `IDateTimeProvider`, `IPasswordHashingService`.

3. **`Tillegal.Persistence` (Data Access Adapter)**
   - Implements data persistence using **Entity Framework Core** with **PostgreSQL**.
   - `AppDbContext` implements `IUnitOfWork` with transaction management (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`).
   - Features automatic `UpdatedAt` timestamp management and global soft-delete query filters on `BaseEntity`.

4. **`Tillegal.Infrastructure` (External Integration Adapters)**
   - External third-party integrations:
     - **Upstash Redis**: Cache implementation over HTTP REST API (`ICacheService`).
     - **Resend**: Transactional email dispatch via HTTP REST API (`IEmailService`).
     - **Supabase Storage**: Object storage management via HTTP REST API (`IStorageService`).
     - **Quartz.NET**: Cron-based background jobs (`DailyUpdateJob`).
   - System services: `DateTimeProvider` (`IDateTimeProvider`), `PasswordHashingService` (`IPasswordHashingService`).

5. **`Tillegal.Api` (HTTP Presentation Library)**
   - ASP.NET Core presentation library containing controllers inheriting `BaseApiController`.
   - Middlewares: `CorrelationIdMiddleware`, `ExceptionHandlingMiddleware`, `RequestLoggingMiddleware`.
   - Swagger / OpenAPI documentation configuration with JWT bearer support.

6. **`Tillegal.AppHost` (Executable Host & Composition Root)**
   - Entry point executable (`Program.cs`) hosting ASP.NET Core runtime.
   - Wires up Dependency Injection for all layers via configuration extension methods.
   - Executes development database migration and seeding on startup.
