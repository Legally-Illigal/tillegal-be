# Development Setup & Workflow

This guide details local environment setup, developer workflow commands, and runtime verification.

## Prerequisites

- **.NET 10 SDK** (e.g. `10.0.100` preview or higher)
- **PostgreSQL 15+** database instance
- Optional accounts for full external integrations: Upstash Redis, Resend, Supabase

## Configuration

Copy the template configuration file to `appsettings.Development.json`:

```bash
cp src/Tillegal.AppHost/appsettings.Example.json src/Tillegal.AppHost/appsettings.Development.json
```

Key configuration sections in `appsettings.Development.json`:
- `ConnectionStrings:Postgres`: Connection string for PostgreSQL database.
- `Jwt`: `Issuer`, `Audience`, `SigningKey` (minimum 32 characters in production).
- `Upstash`: `RedisRestUrl` (HTTPS) and `RedisRestToken`.
- `Resend`: `ApiKey` and `FromEmail`.
- `Supabase`: `Url` (HTTPS), `Bucket`, and `ServiceKey`.
- `BackgroundJobs`: `TimeZone` (default `"Asia/Ho_Chi_Minh"`).

## Essential CLI Commands

Execute all commands from the repository root:

```bash
# 1. Restore dependencies
dotnet restore tillegal-be.slnx

# 2. Compile solution
dotnet build tillegal-be.slnx

# 3. Execute unit and architecture tests
dotnet test tillegal-be.slnx

# 4. Validate source code formatting according to .editorconfig
dotnet format tillegal-be.slnx --verify-no-changes

# 5. Format and fix formatting issues automatically
dotnet format tillegal-be.slnx

# 6. Run Web API host
dotnet run --project src/Tillegal.AppHost
```

## Database Migrations & Seeding

- When running in the `Development` environment, `Program.cs` automatically invokes `ApplyDevelopmentDatabaseChangesAsync()`:
  1. Applies pending EF Core migrations (`dbContext.Database.MigrateAsync()`).
  2. Executes initial development seeders (`DevelopmentDatabaseSeeder.SeedAsync()`).
- In non-development environments, run migrations via standard CI/CD deployment jobs.

## Local Endpoints

- **Swagger UI**: `http://localhost:5063/swagger` (or `https://localhost:7001/swagger`)
- **Liveness Probe**: `http://localhost:5063/healthz` (returns `{"status":"Healthy"}`)
- **Readiness Probe**: `http://localhost:5063/readyz` (verifies PostgreSQL connection)
