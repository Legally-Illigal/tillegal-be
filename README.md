# tillegal-be

Backend API built with .NET 10 and four-layer Clean Architecture.

## Architecture

- `tillegal.Domain`: domain model and business rules; no framework dependencies.
- `tillegal.Application`: use cases, MediatR, FluentValidation, Mapster, and application abstractions.
- `tillegal.Infrastructure`: adapters organized by concern under `Persistence/PostgreSQL`, `Caching/Redis`, `Messaging/RabbitMQ`, `Storage/Supabase`, and `Authentication`.
- `tillegal.API`: HTTP entry point, authentication, CORS, middleware, and dependency composition.

Dependencies point inward: API composes Application and Infrastructure; Infrastructure implements Application/Domain abstractions; Application depends on Domain.

## Local development

Prerequisites: .NET 10 SDK and a Neon PostgreSQL database/branch. The API runs locally and connects to Neon; database credentials must not be committed to source control.

Restore and build:

```powershell
dotnet restore .\tillegal.slnx
dotnet build .\tillegal.slnx
```

Set the development service credentials with User Secrets. Use development-only resources and copy the .NET-format connection string from Neon with SSL enabled:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<neon-host>;Database=<database>;Username=<username>;Password=<password>;SSL Mode=Require" --project .\tillegal.API
dotnet user-secrets set "Jwt:Issuer" "Tillegal" --project .\tillegal.API
dotnet user-secrets set "Jwt:Audience" "Tillegal" --project .\tillegal.API
dotnet user-secrets set "Jwt:Key" "<random-secret-at-least-32-utf8-bytes>" --project .\tillegal.API
dotnet user-secrets set "Jwt:AccessTokenMinutes" "15" --project .\tillegal.API
dotnet user-secrets set "Jwt:RefreshTokenDays" "7" --project .\tillegal.API
dotnet user-secrets set "Redis:ConnectionString" "<upstash-redis-tls-connection-string>" --project .\tillegal.API
dotnet user-secrets set "Supabase:Url" "https://<project-ref>.supabase.co" --project .\tillegal.API
dotnet user-secrets set "Supabase:ServiceRoleKey" "<development-service-role-key>" --project .\tillegal.API
dotnet user-secrets set "Supabase:BucketName" "user-files-dev" --project .\tillegal.API
dotnet user-secrets set "RabbitMQ:Uri" "amqps://<user>:<password>@<cloudamqp-host>/<vhost>" --project .\tillegal.API
dotnet user-secrets set "Cors:AllowedOrigins:0" "<frontend-origin>" --project .\tillegal.API
```

If the API is not being called from a browser frontend, the CORS origin can be omitted. Configure actual frontend origins per environment.

Run the API:

```powershell
dotnet run --project .\tillegal.API
```

Swagger is enabled in Development. Local launch profiles use `http://localhost:5228` and `https://localhost:7263`.

## Database migrations

EF Core Design is included in the API and Infrastructure projects. Restore the repository-local EF CLI tool:

```powershell
dotnet tool restore
dotnet ef --version
```

Create and apply the initial Identity schema migration with:

```powershell
dotnet ef migrations add InitialIdentitySchema --project .\tillegal.Infrastructure --startup-project .\tillegal.API --output-dir Persistence\PostgreSQL\Migrations
dotnet ef database update --project .\tillegal.Infrastructure --startup-project .\tillegal.API
```

This migration creates ASP.NET Core Identity tables. The Domain still has no business entities or seed data; add later business-schema changes through subsequent migrations. Run migrations as an explicit development/release operation rather than automatically from every API instance at startup.

## Cloud deployment

The API is intended to be deployable independently from Neon. For a cloud deployment, configure `ConnectionStrings__DefaultConnection`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__Key`, and any `Cors__AllowedOrigins__<index>` values using the hosting platform's secret/environment configuration. Use a Neon connection string with SSL enabled. Select the cloud host and configure its build, health monitoring, and one-off migration/release process when that provider is chosen; do not store production secrets in appsettings or run schema changes concurrently from multiple API instances.