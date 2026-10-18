# tillegal-be

Tillegal - Phần Backend của hệ thống.

## Solution structure

```text
src/
├── Tillegal.Domain/
│   ├── Common/
│   │   ├── BaseEntity.cs
│   │   └── IAggregateRoot.cs
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Exceptions/
│   └── Repositories/
├── Tillegal.Application/
│   ├── Common/Interfaces/
│   ├── Common/Behaviors/
│   ├── Common/Mappings/
│   └── Features/
│       ├── Auth/{Commands, Dtos}/
│       ├── Conversations/{Commands, Queries, Dtos}/
│       └── Chat/{Commands, Queries, Dtos}/
├── Tillegal.Persistence/
│   ├── Context/
│   ├── Configurations/
│   ├── Repositories/
│   └── Migrations/
├── Tillegal.Infrastructure/
│   ├── ExternalServices/{AiService, Storage, Cache}/
│   ├── Logging/
│   └── SystemServices/
├── Tillegal.Api/
│   ├── Controllers/
│   ├── Contracts/{Requests, Responses}/
│   ├── Middlewares/
│   ├── Filters/
│   ├── Services/
│   └── OpenAPI/
└── Tillegal.AppHost/
    ├── Configurations/
    ├── Extensions/
    └── Options/
```

The repository is the Server Boundary; there is no separate `Tillegal.Server` project.

## Project references

- `Domain`: no project dependencies.
- `Application` → `Domain`.
- `Persistence` → `Domain`, `Application`.
- `Infrastructure` → `Application`.
- `Api` → `Application` only.
- `AppHost` → `Domain`, `Application`, `Persistence`, `Infrastructure`, `Api`.

`Api` is a class library containing HTTP presentation code. `AppHost` is the ASP.NET Core entry point and composition root. Projects target .NET 10. Domain provides a base entity with audit timestamps, an aggregate-root marker, value-object equality, and domain exceptions; feature folders remain scaffolding.
