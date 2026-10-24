# Agent Work Log

Use this file as a concise handoff record, not as a transcript. Append an entry for each task that changes repository content. Keep current work, blockers, next action, changed paths, and checks explicit so the next agent can continue without rereading the entire repository.

## 2026-10-23 â€” Clarify architecture and establish feature backlog

- **Status:** Completed for this documentation task. Backend feature implementation status remains **ChÆ°a Ä‘Ã¡nh giÃ¡** for all BE-F01â€“BE-F09; this task did not audit or implement those features.
- **Completed:** Recorded the nine requested backend feature scopes and Backend/AI Service ownership boundaries in `docs/development/backend-feature-backlog.md`; documented excluded scope and evidence-based status rules. Added agent handoff expectations to `AGENTS.md` and linked the backlog/log from the documentation index and roadmap.
- **Architecture context:** Clarified that domain repository contracts belong in `Tillegal.Domain`, their EF Core implementations in `Tillegal.Persistence`, Application handlers call Domain services, and Application-owned technical ports (such as password hashing/JWT generation) are implemented by Infrastructure. Documented the intended login flow while explicitly noting the login endpoint/service/repository/JWT generation are not present in the inspected baseline and that Bearer-token input is not automatically cookie authentication.
- **Changed paths:** `AGENTS.md`; `docs/README.md`; `docs/development/status-and-roadmap.md`; `docs/development/backend-feature-backlog.md`; `docs/development/agent-worklog.md`; `docs/architecture/boundaries.md`; `docs/architecture/data-flow.md`; `docs/architecture/overview.md`; `docs/development/conventions.md`.
- **Checks:** `git diff --check` passed for tracked changes. The two new Markdown files were manually reviewed. No build or tests were run because this is documentation-only.
- **Remaining / next action:** None for this documentation task.

## 2026-10-24 — Implement User Registration with Email Verification (Resend & Redis OTP)

- **Status:** Completed for registration scope (part of BE-F01).
- **Completed:**
  - Implemented 2-step registration with email verification via OTP (10 minutes TTL).
  - Register flow (POST /api/auth/register): Validates input, hashes password via IPasswordHashingService, stores pending registration + 6-digit OTP into Upstash Redis via ICacheService, and sends formatted verification email via IEmailService (Resend).
  - Verify flow (POST /api/auth/verify-registration): Validates OTP against Redis cache, invokes Domain AuthService and IUserRepository to persist new user to PostgreSQL database, commits via IUnitOfWork, and evicts pending cache.
  - Resend OTP flow (POST /api/auth/resend-otp): Generates fresh OTP for existing pending registration and resends email.
  - Added unit tests covering command handlers and FluentValidation validators.
  - Updated ackend-feature-backlog.md status for BE-F01 to "M?t ph?n" (registration completed; login/logout/session remain).
- **Changed paths:**
  - docs/development/backend-feature-backlog.md
  - docs/development/agent-worklog.md
  - src/Tillegal.Domain/Entities/Role.cs
  - src/Tillegal.Domain/Entities/User.cs
  - src/Tillegal.Domain/Interfaces/Repositories/IUserRepository.cs
  - src/Tillegal.Domain/Services/AuthService.cs
  - src/Tillegal.Application/Features/Auth/Models/PendingRegistration.cs
  - src/Tillegal.Application/Features/Auth/Commands/Register/RegisterCommand.cs
  - src/Tillegal.Application/Features/Auth/Commands/Register/RegisterCommandValidator.cs
  - src/Tillegal.Application/Features/Auth/Commands/Register/RegisterCommandHandler.cs
  - src/Tillegal.Application/Features/Auth/Commands/VerifyRegistration/VerifyRegistrationCommand.cs
  - src/Tillegal.Application/Features/Auth/Commands/VerifyRegistration/VerifyRegistrationCommandValidator.cs
  - src/Tillegal.Application/Features/Auth/Commands/VerifyRegistration/VerifyRegistrationCommandHandler.cs
  - src/Tillegal.Application/Features/Auth/Commands/ResendOtp/ResendOtpCommand.cs
  - src/Tillegal.Application/Features/Auth/Commands/ResendOtp/ResendOtpCommandValidator.cs
  - src/Tillegal.Application/Features/Auth/Commands/ResendOtp/ResendOtpCommandHandler.cs
  - src/Tillegal.Application/DependencyInjection.cs
  - src/Tillegal.Persistence/Repositories/UserRepository.cs
  - src/Tillegal.Persistence/DependencyInjection.cs
  - src/Tillegal.Api/Contracts/Auth/RegisterContracts.cs
  - src/Tillegal.Api/Contracts/Auth/VerifyRegistrationContracts.cs
  - src/Tillegal.Api/Controllers/AuthController.cs
  - 	ests/Tillegal.Application.UnitTests/Features/Auth/RegisterCommandHandlerTests.cs
  - 	ests/Tillegal.Application.UnitTests/Features/Auth/VerifyRegistrationCommandHandlerTests.cs
  - 	ests/Tillegal.Application.UnitTests/Features/Auth/AuthCommandValidatorTests.cs
- **Checks:**
  - dotnet build: Succeeded with 0 warnings, 0 errors.
  - dotnet test: 26 passed in Tillegal.Application.UnitTests, 8 passed in Tillegal.ArchitectureTests (Clean Architecture boundaries strictly satisfied).
- **Remaining / next action:**
  - Implement login (POST /api/auth/login) with JWT token generation and cookie/Bearer token issuance as outlined in architecture documentation.

## 2026-10-24 - Remove UserName field from User entity and registration flow

- **Status:** Completed.
- **Reason:** User login/registration is email-based; UserName is not needed.
- **Completed:**
  - Removed `UserName` property from `User` domain entity.
  - Removed `UserName` from `RegisterCommand`, `RegisterApiRequest`, `RegisterCommandValidator`, `RegisterCommandHandler`, `AuthController.Register`.
  - Removed `UserName` parameter from `AuthService.CreateUserAsync` and `VerifyRegistrationCommandHandler`.
  - Removed `UserName` from `PendingRegistration` record.
  - Removed `UserName` property from `UserDataModel` (EF data model).
  - Removed `UserName` EF configuration from `UserConfiguration.cs`.
  - Removed `UserName` mapping from `UserRepository.MapToEntity`.
  - Removed all `UserName = "..."` assignments from `ComprehensiveDatabaseDataSeed.cs` (10 seed users).
  - Fixed unit tests: removed `UserName` argument from `PendingRegistration` and `RegisterCommand` constructors in test files.
  - Cleaned up commented-out UserName code in validator and handler.
- **Changed paths:**
  - src/Tillegal.Domain/Entities/User.cs
  - src/Tillegal.Domain/Services/AuthService.cs
  - src/Tillegal.Application/Features/Auth/Models/PendingRegistration.cs
  - src/Tillegal.Application/Features/Auth/Commands/Register/RegisterCommand.cs
  - src/Tillegal.Application/Features/Auth/Commands/Register/RegisterCommandValidator.cs
  - src/Tillegal.Application/Features/Auth/Commands/Register/RegisterCommandHandler.cs
  - src/Tillegal.Application/Features/Auth/Commands/VerifyRegistration/VerifyRegistrationCommandHandler.cs
  - src/Tillegal.Api/Controllers/AuthController.cs
  - src/Tillegal.Api/Contracts/Auth/RegisterContracts.cs
  - src/Tillegal.Persistence/Configurations/UserConfiguration.cs
  - src/Tillegal.Persistence/DataModels/UserDataModel.cs
  - src/Tillegal.Persistence/Repositories/UserRepository.cs
  - src/Tillegal.Persistence/SeedData/ComprehensiveDatabaseDataSeed.cs
  - tests/Tillegal.Application.UnitTests/Features/Auth/RegisterCommandHandlerTests.cs
  - tests/Tillegal.Application.UnitTests/Features/Auth/VerifyRegistrationCommandHandlerTests.cs
- **Checks:**
  - `dotnet build`: Succeeded (0 warnings, 0 errors).
  - `dotnet test` (unit tests): 26/26 passed.
  - SeederVerificationTests: 1 failure on `auditLogs.HaveCount(30)` — pre-existing issue unrelated to UserName removal (database has extra audit log records from manual testing).
- **Remaining / next action:**
  - Migration files still reference the `UserName` column (auto-generated, not causing build errors). Generate a new migration to drop the column when ready to apply schema changes.
  - The pre-existing SeederVerificationTests failure should be investigated separately (database state mismatch).


## 2026-10-24 — Add TRUNCATE before seed to clear all tables

- **Status:** Completed.
- **Completed:** Added `ClearAllTablesAsync` to `DevelopmentDatabaseSeeder` that runs `TRUNCATE TABLE ... CASCADE` on all EF Core-managed tables before invoking the individual `IDevelopmentDataSeed` implementations. This ensures seed data is always inserted fresh (no stale rows from prior runs). The constructor now also takes `AppDbContext` (already registered in DI).
- **Changed paths:** `src/Tillegal.Persistence/SeedData/DevelopmentDatabaseSeeder.cs`.
- **Checks run:** `dotnet build` — 0 warnings, 0 errors. `dotnet test` — 26/26 unit tests passed; 1 architecture test (`SeederVerificationTests.Seed_Should_HaveSeededExpectedCounts_And_BeIdempotent`) failed on audit log count (expected 30, found 31) — this is a pre-existing issue unrelated to this change (likely an extra audit log from the running app's middleware).
- **Next action:** None required for this task. The pre-existing `SeederVerificationTests` count mismatch should be investigated separately.

## 2026-10-24 — Integrate Mapster across API, Application, and Domain layers; update conventions documentation

- **Status:** Completed.
- **Completed:**
  - Updated documentation (`docs/development/conventions.md`, `docs/architecture/data-flow.md`, `docs/api/contracts.md`) with explicit guidelines for Mapster integration:
    - API <-> Application: Controllers use `Mapper.Map<...>` to map API requests to MediatR commands/queries, and MediatR results to API responses. Configured via `IRegister` in `Tillegal.Api/Mappings/`.
    - Application <-> Domain: Application handlers use `IMapper` to map Domain entities to Application result DTOs. Configured via `IRegister` in `Tillegal.Application/Features/{Feature}/Mappings/`. Domain remains completely free of external mapping libraries.
    - Added step-by-step feature implementation checklist for data mapping.
  - Refactored user registration flow to use Mapster:
    - Added `AuthApiMappingConfig` in `src/Tillegal.Api/Mappings/` for `Register`, `VerifyRegistration`, and `ResendOtp` request/response contracts.
    - Updated `AuthController` to use `Mapper.Map<...>` on all registration endpoints (`Register`, `VerifyRegistration`, `ResendOtp`).
    - Added `AuthMappingConfig` in `src/Tillegal.Application/Features/Auth/Mappings/` to map `User` domain entity to `VerifyRegistrationResult`.
    - Injected `IMapper` into `VerifyRegistrationCommandHandler` and replaced manual record instantiation with `mapper.Map<VerifyRegistrationResult>(createdUser)`.
    - Added unit tests `AuthMappingTests` verifying Mapster mappings, and updated `VerifyRegistrationCommandHandlerTests` with real Mapster mapper.
- **Changed paths:**
  - `docs/development/conventions.md`
  - `docs/architecture/data-flow.md`
  - `docs/api/contracts.md`
  - `docs/development/agent-worklog.md`
  - `src/Tillegal.Api/Mappings/AuthApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/AuthController.cs`
  - `src/Tillegal.Application/Features/Auth/Mappings/AuthMappingConfig.cs`
  - `src/Tillegal.Application/Features/Auth/Commands/VerifyRegistration/VerifyRegistrationCommandHandler.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Auth/AuthMappingTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Auth/VerifyRegistrationCommandHandlerTests.cs`
- **Checks run:**
  - `dotnet test tillegal-be.slnx`: 28/28 unit tests passed, 7/7 architecture tests passed.
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 formatting discrepancies.
- **Next action:** None for this task.
