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

## 2026-10-25 — Implement User Login flow with JWT token generation and cookie issuance

- **Status:** Completed for login scope (part of BE-F01).
- **Completed:**
  - Implemented Clean Architecture login flow (`POST /api/auth/login`).
  - Added `Role` property to `User` domain entity and updated `UserRepository` to `.Include(u => u.Role)` in `GetByEmailAsync` and `GetByIdAsync`.
  - Added `GetUserForLoginAsync` to `AuthService` in Domain layer.
  - Created `IJwtTokenGenerator` port in `Tillegal.Application/Common/Interfaces/` and implemented concrete `JwtTokenGenerator` in `Tillegal.Infrastructure/SystemServices/` using `System.IdentityModel.Tokens.Jwt` (JWT claims: `sub`, `nameidentifier`, `email`, `name`, `role`, `jti`).
  - Configured `JwtOptions` in `Tillegal.Infrastructure/Options/` and bound/validated in `AppHost/Configurations/InfrastructureExtensions.cs`.
  - Implemented CQRS login flow (`LoginCommand`, `LoginCommandValidator`, `LoginCommandHandler`, `LoginResult`, `LoginUserDto`) in `Tillegal.Application/Features/Auth/Commands/Login/`.
  - Added `UnauthorizedException` in `Tillegal.Application/Common/Exceptions/` and configured HTTP 401 ProblemDetails mapping in `ExceptionHandlingMiddleware`.
  - Added Mapster mapping configurations:
    - Domain <-> Application: `User` -> `LoginUserDto` with Role name in `AuthMappingConfig.cs`.
    - Api <-> Application: `LoginApiRequest` -> `LoginCommand`, `LoginResult` -> `LoginApiResponse`, `LoginUserDto` -> `LoginUserApiResponse` in `AuthApiMappingConfig.cs`.
  - Added `POST /api/auth/login` endpoint in `AuthController`, issuing JWT exclusively via HttpOnly secure cookie (`access_token`) with `Secure = Request.IsHttps`, returning only user information in response body.
  - Configured `AddJwtBearer` with `JwtBearerEvents.OnMessageReceived` in `Tillegal.Api/DependencyInjection.cs` to authenticate requests from the `access_token` cookie when no Authorization header is present.
  - Added comprehensive unit tests in `LoginCommandHandlerTests`, `AuthCommandValidatorTests`, and `AuthMappingTests`.
- **Changed paths:**
  - `docs/development/agent-worklog.md`
  - `src/Tillegal.Domain/Entities/User.cs`
  - `src/Tillegal.Domain/Services/AuthService.cs`
  - `src/Tillegal.Persistence/Repositories/UserRepository.cs`
  - `src/Tillegal.Application/Common/Exceptions/UnauthorizedException.cs`
  - `src/Tillegal.Application/Common/Interfaces/IJwtTokenGenerator.cs`
  - `src/Tillegal.Application/Features/Auth/Commands/Login/LoginCommand.cs`
  - `src/Tillegal.Application/Features/Auth/Commands/Login/LoginCommandValidator.cs`
  - `src/Tillegal.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
  - `src/Tillegal.Application/Features/Auth/Mappings/AuthMappingConfig.cs`
  - `src/Tillegal.Infrastructure/Tillegal.Infrastructure.csproj`
  - `src/Tillegal.Infrastructure/Options/JwtOptions.cs`
  - `src/Tillegal.Infrastructure/SystemServices/JwtTokenGenerator.cs`
  - `src/Tillegal.Infrastructure/DependencyInjection.cs`
  - `src/Tillegal.AppHost/Configurations/InfrastructureExtensions.cs`
  - `src/Tillegal.Api/Contracts/Auth/LoginContracts.cs`
  - `src/Tillegal.Api/Mappings/AuthApiMappingConfig.cs`
  - `src/Tillegal.Api/Middlewares/ExceptionHandlingMiddleware.cs`
  - `src/Tillegal.Api/Controllers/AuthController.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Auth/LoginCommandHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Auth/AuthCommandValidatorTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Auth/AuthMappingTests.cs`
- **Checks run:**
  - `dotnet build tillegal-be.slnx`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tillegal-be.slnx`: 37/37 unit tests passed, 7/7 architecture tests passed.
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 discrepancies.
- **Next action:**
  - Implement logout (`POST /api/auth/logout`) and refresh token flow when required.


## 2026-10-25 — Add Google account registration

- **Status:** Implemented and verified for account registration. Google registration does not issue a Tillegal JWT; login/session issuance remains unimplemented.
- **Completed:** Added `POST /api/auth/register/google` to accept a Google ID token, validate its signature/expiry/issuer/audience with Google.Apis.Auth against configured `GoogleAuth:ClientId`, require a verified email, and create a passwordless user with the default `User` role. Existing email addresses are rejected rather than implicitly linked. Added request validation, HTTP 401 handling for invalid Google tokens, DI/configuration, API documentation, and handler/validator tests.
- **Changed paths:** `docs/api/authentication.md`; `docs/development/backend-feature-backlog.md`; `docs/development/agent-worklog.md`; `src/Tillegal.Api/Contracts/Auth/GoogleRegistrationContracts.cs`; `src/Tillegal.Api/Controllers/AuthController.cs`; `src/Tillegal.Api/Mappings/AuthApiMappingConfig.cs`; `src/Tillegal.Api/Middlewares/ExceptionHandlingMiddleware.cs`; `src/Tillegal.AppHost/Configurations/InfrastructureExtensions.cs`; `src/Tillegal.AppHost/appsettings.Example.json`; `src/Tillegal.Application/Common/Exceptions/GoogleIdentityTokenException.cs`; `src/Tillegal.Application/Common/Interfaces/IGoogleIdentityTokenValidator.cs`; `src/Tillegal.Application/Common/Models/GoogleIdentity.cs`; `src/Tillegal.Application/Features/Auth/Commands/RegisterWithGoogle/RegisterWithGoogleCommand.cs`; `src/Tillegal.Application/Features/Auth/Commands/RegisterWithGoogle/RegisterWithGoogleCommandHandler.cs`; `src/Tillegal.Application/Features/Auth/Commands/RegisterWithGoogle/RegisterWithGoogleCommandValidator.cs`; `src/Tillegal.Application/Features/Auth/Mappings/AuthMappingConfig.cs`; `src/Tillegal.Domain/Services/AuthService.cs`; `src/Tillegal.Infrastructure/DependencyInjection.cs`; `src/Tillegal.Infrastructure/ExternalServices/Auth/GoogleIdentityTokenValidator.cs`; `src/Tillegal.Infrastructure/Options/GoogleAuthOptions.cs`; `src/Tillegal.Infrastructure/Tillegal.Infrastructure.csproj`; `tests/Tillegal.Application.UnitTests/Features/Auth/AuthCommandValidatorTests.cs`; `tests/Tillegal.Application.UnitTests/Features/Auth/RegisterWithGoogleCommandHandlerTests.cs`.
- **Checks:** `dotnet build src/Tillegal.AppHost/Tillegal.AppHost.csproj` passed (0 warnings, 0 errors); Application unit tests passed (33/33); clean architecture tests passed (4/4); `dotnet format tillegal-be.slnx --verify-no-changes` passed; `git diff --check` passed.
- **Remaining / next action:** Configure `GoogleAuth__ClientId` with the frontend's OAuth client ID before deployment. JWT issuance and Google login were subsequently implemented in the 2026-10-25 follow-up.

## 2026-10-25 — Persist Google subject and issue Tillegal JWTs

- **Status:** Implemented and verified for Google registration/login.
- **Completed:** Persisted Google's stable `sub` claim in `Users.GoogleSubject` with a unique index; added `POST /api/auth/login/google` to authenticate by verified Google subject; both Google registration and login issue Tillegal JWTs signed with the existing `Jwt` settings and return `accessToken`/`expiresAt`. The Google ID token itself is not persisted.
- **Changed paths:** `docs/api/authentication.md`; `docs/development/backend-feature-backlog.md`; `docs/development/agent-worklog.md`; `src/Tillegal.Api/Contracts/Auth/GoogleRegistrationContracts.cs`; `src/Tillegal.Api/Controllers/AuthController.cs`; `src/Tillegal.Api/Mappings/AuthApiMappingConfig.cs`; `src/Tillegal.Application/Common/Interfaces/IJwtTokenGenerator.cs`; `src/Tillegal.Application/Common/Models/GeneratedJwt.cs`; `src/Tillegal.Application/Common/Models/GoogleIdentity.cs`; `src/Tillegal.Application/Features/Auth/Commands/LoginWithGoogle/LoginWithGoogleCommand.cs`; `src/Tillegal.Application/Features/Auth/Commands/LoginWithGoogle/LoginWithGoogleCommandHandler.cs`; `src/Tillegal.Application/Features/Auth/Commands/LoginWithGoogle/LoginWithGoogleCommandValidator.cs`; `src/Tillegal.Application/Features/Auth/Commands/RegisterWithGoogle/RegisterWithGoogleCommand.cs`; `src/Tillegal.Application/Features/Auth/Commands/RegisterWithGoogle/RegisterWithGoogleCommandHandler.cs`; `src/Tillegal.Application/Features/Auth/Mappings/AuthMappingConfig.cs`; `src/Tillegal.Domain/Entities/User.cs`; `src/Tillegal.Domain/Interfaces/Repositories/IUserRepository.cs`; `src/Tillegal.Domain/Services/AuthService.cs`; `src/Tillegal.Infrastructure/DependencyInjection.cs`; `src/Tillegal.Infrastructure/ExternalServices/Auth/GoogleIdentityTokenValidator.cs`; `src/Tillegal.Infrastructure/SystemServices/JwtTokenGenerator.cs`; `src/Tillegal.Infrastructure/Tillegal.Infrastructure.csproj`; `src/Tillegal.Persistence/Configurations/UserConfiguration.cs`; `src/Tillegal.Persistence/DataModels/UserDataModel.cs`; `src/Tillegal.Persistence/Migrations/20261025051549_AddGoogleSubjectToUsers.cs`; `src/Tillegal.Persistence/Migrations/20261025051549_AddGoogleSubjectToUsers.Designer.cs`; `src/Tillegal.Persistence/Migrations/AppDbContextModelSnapshot.cs`; `src/Tillegal.Persistence/Repositories/UserRepository.cs`; `tests/Tillegal.Application.UnitTests/Features/Auth/AuthCommandValidatorTests.cs`; `tests/Tillegal.Application.UnitTests/Features/Auth/RegisterWithGoogleCommandHandlerTests.cs`; `tests/Tillegal.Application.UnitTests/Features/Auth/LoginWithGoogleCommandHandlerTests.cs`.
- **Checks:** `dotnet build src/Tillegal.AppHost/Tillegal.AppHost.csproj` passed (0 warnings, 0 errors); Application unit tests passed (36/36); `dotnet format tillegal-be.slnx --verify-no-changes` passed after formatting the generated migration; migration generation completed with `dotnet ef migrations add`; `git diff --check` passed.
- **Remaining / next action:** Configure `GoogleAuth__ClientId` before deployment. Development/Staging applies EF migrations automatically; apply `AddGoogleSubjectToUsers` via the production database deployment process before releasing these endpoints. Accounts created with Google before this migration need an explicit account-linking flow; they are not automatically linked by email.

## 2026-10-25 — Resolve infrastructure configuration conflict

- **Completed:** Retained JWT signing-key validation and Google OAuth client-ID validation as separate startup options registrations in `InfrastructureExtensions`.
- **Changed paths:** `src/Tillegal.AppHost/Configurations/InfrastructureExtensions.cs`; `docs/development/agent-worklog.md`.
- **Checks:** `git diff --check -- src/Tillegal.AppHost/Configurations/InfrastructureExtensions.cs` passed; the edited file contains no conflict markers. Repository-wide `git diff --check` reports conflict markers in other unmerged files. `dotnet build src/Tillegal.AppHost/Tillegal.AppHost.csproj --no-restore` was blocked by markers in `IJwtTokenGenerator.cs` and `AuthMappingConfig.cs`; the repository-wide build tool also failed with MSBuild target errors.
- **Remaining / next action:** Other unmerged paths remain unresolved; complete them in Git Changes and rerun the build.

## 2026-10-25 — Recheck and merge remaining auth conflicts

- **Completed:** Removed remaining conflict markers from the Git working-copy resolutions. Kept both JWT generator APIs in one implementation backed by the validated `JwtOptions`, retained mappings for password login and Google registration, and registered the Google token validator.
- **Changed paths:** `src/Tillegal.Application/Common/Interfaces/IJwtTokenGenerator.cs`; `src/Tillegal.Application/Features/Auth/Mappings/AuthMappingConfig.cs`; `src/Tillegal.Infrastructure/DependencyInjection.cs`; `src/Tillegal.Infrastructure/SystemServices/JwtTokenGenerator.cs`; `docs/development/agent-worklog.md`.
- **Checks:** `dotnet build src/Tillegal.AppHost/Tillegal.AppHost.csproj --no-restore` passed (0 warnings, 0 errors); Application unit tests passed (45/45); `dotnet format tillegal-be.slnx --verify-no-changes --no-restore` passed; conflict-marker scan and scoped `git diff --check` passed.
- **Remaining / next action:** All five resolved conflict paths are staged and Git reports no unmerged paths. Run `git rebase --continue` from Git Changes or the terminal to finish replaying the commit.

## 2026-10-25 — Store Google auth JWT in cookie

- **Completed:** Google registration and login now set the Tillegal JWT in the same `access_token` HttpOnly cookie used by password login. Removed the access token and expiry from the Google API JSON response contract; documented the updated behavior.
- **Changed paths:** `src/Tillegal.Api/Controllers/AuthController.cs`; `src/Tillegal.Api/Contracts/Auth/GoogleRegistrationContracts.cs`; `docs/api/authentication.md`; `docs/development/agent-worklog.md`.
- **Checks:** AppHost build passed (0 warnings, 0 errors); Application unit tests passed (45/45); `dotnet format tillegal-be.slnx --verify-no-changes --no-restore` passed.
- **Remaining / next action:** Git rebase still awaits `git rebase --continue`.

## 2026-10-25 — Add Domain auth service interface

- **Completed:** Added `IAuthService` under Domain interfaces, implemented it in `AuthService`, registered the abstraction in Application DI, and updated authentication handlers to depend on the interface.
- **Changed paths:** `src/Tillegal.Domain/Interfaces/Services/IAuthService.cs`; `src/Tillegal.Domain/Services/AuthService.cs`; `src/Tillegal.Application/DependencyInjection.cs`; auth command handlers under `src/Tillegal.Application/Features/Auth/Commands/`; `docs/development/agent-worklog.md`.
- **Checks:** AppHost build passed (0 warnings, 0 errors); Application unit tests passed (45/45); `dotnet format tillegal-be.slnx --verify-no-changes --no-restore` passed. One test run concurrent with the build hit a temporary compiler file lock (`CS2012`); rerunning tests after the build completed passed.
- **Remaining / next action:** Git rebase still awaits `git rebase --continue`.

## 2026-10-25 — Align Google registration result construction

- **Completed:** Constructed `GoogleRegistrationResult` explicitly, matching the Google login handler, removed the unused mapper dependency and placeholder-token Mapster mapping, and updated the handler test setup.
- **Changed paths:** `src/Tillegal.Application/Features/Auth/Commands/RegisterWithGoogle/RegisterWithGoogleCommandHandler.cs`; `src/Tillegal.Application/Features/Auth/Mappings/AuthMappingConfig.cs`; `tests/Tillegal.Application.UnitTests/Features/Auth/RegisterWithGoogleCommandHandlerTests.cs`; `docs/development/agent-worklog.md`.
- **Checks:** AppHost build passed (0 warnings, 0 errors); Application unit tests passed (45/45); `dotnet format tillegal-be.slnx --verify-no-changes --no-restore` passed.
- **Remaining / next action:** Git rebase still awaits `git rebase --continue`.

## 2026-10-25 — Remove redundant login DTO and map API response

- **Completed:** Removed application-layer `LoginUserDto`; `LoginResult` now carries the authenticated `User`, and the API Mapster config maps that user to `LoginUserApiResponse`, including role-name flattening. Removed the obsolete Application mapping and adjusted login tests.
- **Changed paths:** `src/Tillegal.Application/Features/Auth/Commands/Login/LoginCommand.cs`; `src/Tillegal.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`; `src/Tillegal.Application/Features/Auth/Mappings/AuthMappingConfig.cs`; `src/Tillegal.Api/Mappings/AuthApiMappingConfig.cs`; `tests/Tillegal.Application.UnitTests/Features/Auth/LoginCommandHandlerTests.cs`; `tests/Tillegal.Application.UnitTests/Features/Auth/AuthMappingTests.cs`; `docs/development/agent-worklog.md`.
- **Checks:** AppHost build passed (0 warnings, 0 errors); Application unit tests passed (44/44); `dotnet format tillegal-be.slnx --verify-no-changes --no-restore` and `git diff --check` passed.
- **Remaining / next action:** Git rebase still awaits `git rebase --continue`.

## 2026-10-25 — Use Mapster for auth result composition

- **Completed:** Added explicit Mapster mappings for login and Google login/registration result construction from user/JWT source records; injected `IMapper` into those handlers. Kept Register and ResendOtp result construction direct because those results are calculated scalar messages/expiry values rather than object mappings.
- **Changed paths:** `src/Tillegal.Application/Features/Auth/Mappings/AuthMappingConfig.cs`; `src/Tillegal.Application/Features/Auth/Mappings/AuthResultMappingSources.cs`; `src/Tillegal.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`; `src/Tillegal.Application/Features/Auth/Commands/LoginWithGoogle/LoginWithGoogleCommandHandler.cs`; `src/Tillegal.Application/Features/Auth/Commands/RegisterWithGoogle/RegisterWithGoogleCommandHandler.cs`; auth handler tests; `docs/development/agent-worklog.md`.
- **Checks:** AppHost build passed (0 warnings, 0 errors); Application unit tests passed (44/44); `dotnet format tillegal-be.slnx --verify-no-changes --no-restore` passed.
- **Remaining / next action:** Git rebase still awaits `git rebase --continue`.

## 2026-10-28 — Implement Message CRUD (BE-F04 scope)

- **Status:** Completed for Message CRUD scope (part of BE-F04).
- **Completed:**
  - Enhanced `ConversationMessage` domain entity with property encapsulation, invariant validation, static factories (`Create`, `CreateUserMessage`, `CreateAssistantMessage`), and `UpdateContent` domain mutation method.
  - Declared `IConversationMessageRepository` repository interface in `Tillegal.Domain` and implemented it in `Tillegal.Persistence/Repositories/ConversationMessageRepository.cs` with EF Core query filters for soft-delete.
  - Declared `IConversationMessageService` domain service interface and implemented `ConversationMessageService` in `Tillegal.Domain/Services/`.
  - Registered `IConversationMessageRepository` in Persistence DI and `IConversationMessageService` in Application DI.
  - Implemented CQRS Message operations under `Tillegal.Application/Features/Messages/`:
    - `CreateMessageCommand`, `CreateMessageCommandValidator`, `CreateMessageCommandHandler`
    - `UpdateMessageCommand`, `UpdateMessageCommandValidator`, `UpdateMessageCommandHandler`
    - `DeleteMessageCommand`, `DeleteMessageCommandValidator`, `DeleteMessageCommandHandler`
    - `GetConversationMessagesQuery`, `GetConversationMessagesQueryValidator`, `GetConversationMessagesQueryHandler`
    - `GetMessageByIdQuery`, `GetMessageByIdQueryValidator`, `GetMessageByIdQueryHandler`
    - Enforced authentication via `ICurrentUserService`, verified conversation ownership (`NotFoundException` / `ForbiddenAccessException`), and verified message-to-conversation association.
  - Configured Mapster mappings:
    - Domain <-> Application: `ConversationMessage` -> `MessageDto` in `MessageMappingConfig.cs`.
    - Api <-> Application: `MessageDto` -> `MessageApiResponse` in `MessageApiMappingConfig.cs`.
  - Implemented RESTful endpoints on `ConversationsController` under route `api/conversations/{conversationId:guid}/messages`:
    - `POST /api/conversations/{conversationId}/messages` (201 Created)
    - `GET /api/conversations/{conversationId}/messages` (200 OK)
    - `GET /api/conversations/{conversationId}/messages/{messageId}` (200 OK)
    - `PUT /api/conversations/{conversationId}/messages/{messageId}` (200 OK)
    - `DELETE /api/conversations/{conversationId}/messages/{messageId}` (204 NoContent)
  - Added unit test suite covering:
    - Domain invariants & factory methods (`ConversationMessageTests.cs` - 10 tests)
    - FluentValidation rules (`MessageCommandValidatorTests.cs` - 15 tests)
    - Mapster mappings (`MessageMappingTests.cs` - 1 test)
    - CQRS Handlers with authentication, authorization, not found, and happy paths (Create, GetMessages, GetById, Update, Delete - 21 tests)
  - Updated `docs/development/backend-feature-backlog.md` status for BE-F04 to "Một phần".
- **Changed paths:**
  - `docs/development/agent-worklog.md`
  - `docs/development/backend-feature-backlog.md`
  - `src/Tillegal.Domain/Entities/ConversationMessage.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IConversationMessageRepository.cs`
  - `src/Tillegal.Domain/Interfaces/Services/IConversationMessageService.cs`
  - `src/Tillegal.Domain/Services/ConversationMessageService.cs`
  - `src/Tillegal.Persistence/Repositories/ConversationMessageRepository.cs`
  - `src/Tillegal.Persistence/DependencyInjection.cs`
  - `src/Tillegal.Application/DependencyInjection.cs`
  - `src/Tillegal.Application/Features/Messages/Models/MessageDto.cs`
  - `src/Tillegal.Application/Features/Messages/Mappings/MessageMappingConfig.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/CreateMessage/CreateMessageCommand.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/CreateMessage/CreateMessageCommandValidator.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/CreateMessage/CreateMessageCommandHandler.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/UpdateMessage/UpdateMessageCommand.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/UpdateMessage/UpdateMessageCommandValidator.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/UpdateMessage/UpdateMessageCommandHandler.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/DeleteMessage/DeleteMessageCommand.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/DeleteMessage/DeleteMessageCommandValidator.cs`
  - `src/Tillegal.Application/Features/Messages/Commands/DeleteMessage/DeleteMessageCommandHandler.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetConversationMessages/GetConversationMessagesQuery.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetConversationMessages/GetConversationMessagesQueryValidator.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetConversationMessages/GetConversationMessagesQueryHandler.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetMessageById/GetMessageByIdQuery.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetMessageById/GetMessageByIdQueryValidator.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetMessageById/GetMessageByIdQueryHandler.cs`
  - `src/Tillegal.Api/Contracts/Messages/MessageContracts.cs`
  - `src/Tillegal.Api/Mappings/MessageApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/ConversationMessageTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/MessageCommandValidatorTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/MessageMappingTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/CreateMessageCommandHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/GetConversationMessagesQueryHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/GetMessageByIdQueryHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/UpdateMessageCommandHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/DeleteMessageCommandHandlerTests.cs`
- **Checks run:**
  - `dotnet build tillegal-be.slnx`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tillegal-be.slnx`: 161/161 tests passed (154 unit tests, 7 architecture tests).
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 discrepancies.
- **Remaining / next action:**
  - Implement AI orchestration and context retrieval for legal queries (BE-F05 & BE-F06).


## 2026-10-30 — Implement Cursor-based Pagination for Messages API

- **Status:** Completed for cursor-based message pagination.
- **Completed:**
  - Added `CursorPagedResult<T>` domain record in `Tillegal.Domain.Common`.
  - Added `GetMessagesByConversationIdAsync(Guid conversationId, Guid? cursor, int limit, CancellationToken cancellationToken)` in `IConversationMessageRepository`, `ConversationMessageRepository`, `IConversationMessageService`, and `ConversationMessageService`.
  - Implemented SQL query pattern in EF Core: filters messages by `CreatedAt < cursorMessage.CreatedAt || (CreatedAt == cursorMessage.CreatedAt && Id < cursorMessage.Id)`, orders by `CreatedAt DESC, Id DESC`, fetches `limit + 1` records to determine `has_more`, takes `limit` items and reverses the list to return in chronological order (oldest to newest), with `next_cursor` pointing to the oldest message in the batch (`data[0].Id`).
  - Added `CursorPagedMessagesDto` in `Tillegal.Application/Features/Messages/Models/`.
  - Updated `GetConversationMessagesQuery` to accept `Cursor` and `Limit` (defaults to 5, max 50), and updated its validator and handler.
  - Added `CursorPagedMessagesApiResponse` with `[JsonPropertyName("data")]`, `[JsonPropertyName("has_more")]`, `[JsonPropertyName("next_cursor")]` to match the exact frontend JSON contract.
  - Updated `ConversationsController.GetMessages` to accept `[FromQuery] Guid? cursor`, `[FromQuery] int? limit`, `[FromQuery] int? pageSize` (defaulting to 5), returning `CursorPagedMessagesApiResponse`.
  - Updated unit tests in `GetConversationMessagesQueryHandlerTests` and `MessageCommandValidatorTests`.
- **Changed paths:**
  - `src/Tillegal.Domain/Common/CursorPagedResult.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IConversationMessageRepository.cs`
  - `src/Tillegal.Domain/Interfaces/Services/IConversationMessageService.cs`
  - `src/Tillegal.Domain/Services/ConversationMessageService.cs`
  - `src/Tillegal.Persistence/Repositories/ConversationMessageRepository.cs`
  - `src/Tillegal.Application/Features/Messages/Models/CursorPagedMessagesDto.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetConversationMessages/GetConversationMessagesQuery.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetConversationMessages/GetConversationMessagesQueryValidator.cs`
  - `src/Tillegal.Application/Features/Messages/Queries/GetConversationMessages/GetConversationMessagesQueryHandler.cs`
  - `src/Tillegal.Api/Contracts/Messages/MessageContracts.cs`
  - `src/Tillegal.Api/Mappings/MessageApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/GetConversationMessagesQueryHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/MessageCommandValidatorTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build tillegal-be.slnx`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tillegal-be.slnx`: 164/164 tests passed (157 unit tests, 7 architecture tests).
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 discrepancies.
- **Remaining / next action:**
  - Frontend integration for infinite scroll chat loading using `next_cursor` and `has_more`.


## 2026-10-31 — Implement AI Evaluation Management (User and Admin)

- **Status:** Completed for AI Evaluation Management.
- **Completed:**
  - Enhanced `AiEvaluationLog` domain entity with encapsulation, invariant validation (MessageId not empty, rating between 1 and 5, at least one attribute provided), static factory `Create`, and expressive `Update` and `SoftDelete` mutation methods.
  - Declared `IAiEvaluationRepository` in `Tillegal.Domain/Interfaces/Repositories/` and implemented it in `Tillegal.Persistence/Repositories/AiEvaluationRepository.cs` with query filtering (isHelpful, rating, evaluatorUserId), paging, count, summary metrics, and soft-delete support. Registered in Persistence DI.
  - Declared `IAiEvaluationService` in `Tillegal.Domain/Interfaces/Services/` and implemented `AiEvaluationService` in `Tillegal.Domain/Services/`. Registered in Application DI.
  - Added `Role` property to `ICurrentUserService` and `CurrentUserService` to enable role-based authorization in CQRS handlers.
  - Implemented CQRS operations under `Tillegal.Application/Features/AiEvaluations/`:
    - `CreateEvaluationCommand`, `CreateEvaluationCommandValidator`, `CreateEvaluationCommandHandler`: Validates authentication, message existence, verifies role is 'assistant', verifies conversation ownership (or Admin role), checks single evaluation per message invariant, commits via `IUnitOfWork`.
    - `UpdateEvaluationCommand`, `UpdateEvaluationCommandValidator`, `UpdateEvaluationCommandHandler`: Enforces evaluation ownership or Admin role.
    - `DeleteEvaluationCommand`, `DeleteEvaluationCommandValidator`, `DeleteEvaluationCommandHandler`: Soft-deletes evaluation for owner or Admin.
    - `GetEvaluationByIdQuery`, `GetEvaluationByIdQueryValidator`, `GetEvaluationByIdQueryHandler`
    - `GetEvaluationByMessageIdQuery`, `GetEvaluationByMessageIdQueryValidator`, `GetEvaluationByMessageIdQueryHandler`
    - `GetAiEvaluationsQuery`, `GetAiEvaluationsQueryValidator`, `GetAiEvaluationsQueryHandler`: Paged list with filters (isHelpful, rating, evaluatorUserId); non-admin users are scoped to their own evaluations, Admin can query all.
    - `GetAiEvaluationSummaryQuery`, `GetAiEvaluationSummaryQueryHandler`: Returns total evaluations, average rating, helpful and unhelpful counts.
  - Configured Mapster mappings:
    - Application: `AiEvaluationMappingConfig` (`AiEvaluationLog` -> `AiEvaluationDto`).
    - API: `AiEvaluationApiMappingConfig` (`CreateAiEvaluationApiRequest` -> `CreateEvaluationCommand`, DTOs -> API responses).
  - Implemented `AiEvaluationsController` under route `api/ai-evaluations`:
    - `POST /api/ai-evaluations` (201 Created)
    - `GET /api/ai-evaluations` (200 OK)
    - `GET /api/ai-evaluations/summary` (200 OK)
    - `GET /api/ai-evaluations/{id:guid}` (200 OK)
    - `GET /api/ai-evaluations/message/{messageId:guid}` (200 OK)
    - `PUT /api/ai-evaluations/{id:guid}` (200 OK)
    - `DELETE /api/ai-evaluations/{id:guid}` (204 NoContent)
  - Added comprehensive test suite:
    - Domain tests: `AiEvaluationLogTests.cs` (8 tests).
    - Validator tests: `AiEvaluationCommandValidatorTests.cs` (15 tests).
    - Mapping tests: `AiEvaluationMappingTests.cs` (1 test).
    - Handler unit tests: `CreateEvaluationCommandHandlerTests.cs` (8 tests), `UpdateEvaluationCommandHandlerTests.cs` (5 tests), `DeleteEvaluationCommandHandlerTests.cs` (5 tests), `GetEvaluationQueryHandlerTests.cs` (6 tests).
- **Changed paths:**
  - `src/Tillegal.Domain/Entities/AiEvaluationLog.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IAiEvaluationRepository.cs`
  - `src/Tillegal.Domain/Interfaces/Services/IAiEvaluationService.cs`
  - `src/Tillegal.Domain/Services/AiEvaluationService.cs`
  - `src/Tillegal.Persistence/Repositories/AiEvaluationRepository.cs`
  - `src/Tillegal.Persistence/DependencyInjection.cs`
  - `src/Tillegal.Application/Common/Interfaces/ICurrentUserService.cs`
  - `src/Tillegal.Application/DependencyInjection.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Models/AiEvaluationDto.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Models/AiEvaluationSummaryDto.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Models/PagedAiEvaluationsDto.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Mappings/AiEvaluationMappingConfig.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Commands/CreateEvaluation/`
  - `src/Tillegal.Application/Features/AiEvaluations/Commands/UpdateEvaluation/`
  - `src/Tillegal.Application/Features/AiEvaluations/Commands/DeleteEvaluation/`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetAiEvaluations/`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetAiEvaluationSummary/`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetEvaluationById/`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetEvaluationByMessageId/`
  - `src/Tillegal.Api/Contracts/AiEvaluations/AiEvaluationContracts.cs`
  - `src/Tillegal.Api/Mappings/AiEvaluationApiMappingConfig.cs`
  - `src/Tillegal.Api/Services/CurrentUserService.cs`
  - `src/Tillegal.Api/Controllers/AiEvaluationsController.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/AiEvaluationLogTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/AiEvaluations/`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build src/Tillegal.AppHost/Tillegal.AppHost.csproj`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 213/213 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed (strict Clean Architecture boundaries).
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 formatting discrepancies.
- **Next action:**
  - Frontend integration for message evaluation buttons (thumbs up/down, star ratings, and feedback form) and Admin AI performance dashboard.


## 2026-11-02 — Implement Admin User CRUD Management

- **Status:** Completed for user management scope (part of BE-F08).
- **Completed:**
  - Domain layer: Extended `IUserRepository` with `GetPagedAsync`, `UpdateAsync`, and `DeleteAsync`.
  - Persistence layer: Implemented `GetPagedAsync` (with search and pagination), `UpdateAsync`, and soft-delete via `DeleteAsync` (setting `DeletedAt`) in `UserRepository`.
  - Application layer: Implemented CQRS features under `Tillegal.Application.Features.Admin.Users`:
    - Commands: `CreateUser` (with password hashing and role assignment), `UpdateUser`, `DeleteUser`.
    - Queries: `GetUsers` (paged list with search term), `GetUserById`.
    - Validators: FluentValidation validators for all commands and queries.
    - Models: `UserDto`, `PagedUsersDto`.
    - Mappings: `AdminUserMappingConfig` for mapping `User` to `UserDto`.
    - Authorization: Enforced `Admin` role checks at application handler level and controller level.
  - API layer:
    - Added `AdminUsersController` under route `/api/admin/users` secured with `[Authorize(Roles = "Admin")]`.
    - Endpoints:
      - `POST /api/admin/users` (201 Created)
      - `GET /api/admin/users` (200 OK)
      - `GET /api/admin/users/{id:guid}` (200 OK)
      - `PUT /api/admin/users/{id:guid}` (200 OK)
      - `DELETE /api/admin/users/{id:guid}` (204 NoContent)
    - Contracts: `CreateAdminUserApiRequest`, `UpdateAdminUserApiRequest`, `AdminUserApiResponse`, `PagedAdminUsersApiResponse`.
    - Mapping: `AdminUserApiMappingConfig`.
  - Tests:
    - Added unit test classes under `tests/Tillegal.Application.UnitTests/Features/Admin/`:
      - `CreateUserCommandHandlerTests.cs` (4 tests)
      - `GetUsersQueryHandlerTests.cs` (3 tests)
      - `GetUserByIdQueryHandlerTests.cs` (4 tests)
      - `UpdateUserCommandHandlerTests.cs` (4 tests)
      - `DeleteUserCommandHandlerTests.cs` (4 tests)
  - Updated `backend-feature-backlog.md` status for BE-F08 to "Một phần".
- **Changed paths:**
  - `src/Tillegal.Domain/Interfaces/Repositories/IUserRepository.cs`
  - `src/Tillegal.Persistence/Repositories/UserRepository.cs`
  - `src/Tillegal.Application/Features/Admin/Users/`
  - `src/Tillegal.Api/Contracts/Admin/AdminUserContracts.cs`
  - `src/Tillegal.Api/Mappings/AdminUserApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/AdminUsersController.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Admin/`
  - `docs/development/backend-feature-backlog.md`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build` (Domain, Application, Persistence, Api, Tests): All succeeded with 0 errors.
  - `dotnet test tests/Tillegal.Application.UnitTests`: 232/232 passed (including 19 new Admin tests).
  - `dotnet test tests/Tillegal.ArchitectureTests`: 7/7 passed.
- **Next action:**
  - Frontend integration for Admin User Management interface.

## 2026-11-04 — Remove Duplicate Route on Admin UpdateStatus Endpoint

- **Status:** Completed.
- **Context & Motivation:** Swagger UI displayed two duplicate endpoints (`PUT /api/admin/users/{id}` and `PUT /api/admin/users/{id}/status`) because both route attributes were previously placed on the single `UpdateStatus` action method.
- **Completed:**
  - Removed `[HttpPut("{id:guid}")]` from `AdminUsersController`, keeping only `[HttpPut("{id:guid}/status")]` to maintain a single, explicit RESTful contract.
- **Changed paths:**
  - `src/Tillegal.Api/Controllers/AdminUsersController.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build src/Tillegal.Api/Tillegal.Api.csproj`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests`: 226/226 passed.
  - `dotnet format src/Tillegal.Api/Tillegal.Api.csproj --verify-no-changes`: Passed.

## 2026-11-03 — Remove Admin Create User Endpoint for Security Hardening

- **Status:** Completed.
- **Context & Motivation:** In accordance with security-by-design principles to reduce attack surface and avoid user enumeration/direct privilege injection bypass, the `POST /api/admin/users` endpoint was removed. User creation is strictly restricted to the standard registration + OTP email verification flow.
- **Completed:**
  - Removed `[HttpPost] Create` action in `AdminUsersController`.
  - Removed `CreateAdminUserApiRequest` in `AdminUserContracts.cs` and mapping in `AdminUserApiMappingConfig.cs`.
  - Deleted `CreateUserCommand`, `CreateUserCommandHandler`, and `CreateUserCommandValidator` from `src/Tillegal.Application/Features/Admin/Users/Commands/CreateUser/`.
  - Deleted obsolete `CreateUserCommandHandlerTests.cs`.
- **Changed paths:**
  - `src/Tillegal.Api/Controllers/AdminUsersController.cs`
  - `src/Tillegal.Api/Contracts/Admin/AdminUserContracts.cs`
  - `src/Tillegal.Api/Mappings/AdminUserApiMappingConfig.cs`
  - `src/Tillegal.Application/Features/Admin/Users/Commands/CreateUser/` (deleted)
  - `tests/Tillegal.Application.UnitTests/Features/Admin/CreateUserCommandHandlerTests.cs` (deleted)
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build tillegal-be.slnx`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tillegal-be.slnx`: 228/228 unit tests passed; 7/7 architecture tests passed.
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 formatting discrepancies.
- **Next action:**
  - Proceed with any further Admin capabilities or frontend integration.

## 2026-11-03 — Restrict Admin User Management: Disallow Deletion and Limit Updates to Account Status

- **Status:** Completed.
- **Context & Motivation:** Implemented strict admin boundaries as requested:
  1. Admin is not permitted to delete users or delete themselves (removed DELETE user endpoint).
  2. Admin is not permitted to modify users' personal details (FullName, PhoneNumber, etc.).
  3. Admin can only update management status (open / lock account via `IsActive`).
  4. Admin is strictly prevented from locking/deactivating their own account.
- **Completed:**
  - Domain: Added `bool IsActive` property to `User` entity; removed `DeleteAsync` from `IUserRepository`.
  - Persistence: In `UserRepository`, mapped `IsActive = dataModel.DeletedAt == null`; in `UpdateAsync`, strictly updated `DeletedAt` without mutating personal profile data; added `.IgnoreQueryFilters()` to `GetByIdAsync` and `GetPagedAsync` so admins can view and manage both active and locked users; removed `DeleteAsync`.
  - Application:
    - Removed `DeleteUser` and `UpdateUser` commands and their validators/handlers.
    - Added `UpdateUserStatusCommand`, `UpdateUserStatusCommandValidator`, and `UpdateUserStatusCommandHandler` with self-lockout prevention rule (`currentUserService.UserId == request.Id && !request.IsActive` throws `DomainException`).
    - Added `bool IsActive` to `UserDto` and updated Mapster config.
  - API:
    - Removed `[HttpDelete("{id:guid}")]` action in `AdminUsersController`.
    - Replaced `Update` action with `[HttpPut("{id:guid}/status")]` (and alias `[HttpPut("{id:guid}")]`) receiving `UpdateUserStatusApiRequest(bool IsActive)`.
    - Added `bool IsActive` to `AdminUserApiResponse`.
  - Tests:
    - Deleted obsolete `DeleteUserCommandHandlerTests` and `UpdateUserCommandHandlerTests`.
    - Added `UpdateUserStatusCommandHandlerTests` covering unauthorized, forbidden, self-lock rejection, user not found, successful lock, and successful unlock.
    - Updated `GetUserByIdQueryHandlerTests` and `GetUsersQueryHandlerTests` for `UserDto.IsActive`.
- **Changed paths:**
  - `src/Tillegal.Domain/Entities/User.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IUserRepository.cs`
  - `src/Tillegal.Persistence/Repositories/UserRepository.cs`
  - `src/Tillegal.Application/Features/Admin/Users/Models/UserDto.cs`
  - `src/Tillegal.Application/Features/Admin/Users/Mappings/AdminUserMappingConfig.cs`
  - `src/Tillegal.Application/Features/Admin/Users/Commands/DeleteUser/` (deleted)
  - `src/Tillegal.Application/Features/Admin/Users/Commands/UpdateUser/` (deleted)
  - `src/Tillegal.Application/Features/Admin/Users/Commands/UpdateUserStatus/`
  - `src/Tillegal.Api/Contracts/Admin/AdminUserContracts.cs`
  - `src/Tillegal.Api/Controllers/AdminUsersController.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Admin/DeleteUserCommandHandlerTests.cs` (deleted)
  - `tests/Tillegal.Application.UnitTests/Features/Admin/UpdateUserCommandHandlerTests.cs` (deleted)
  - `tests/Tillegal.Application.UnitTests/Features/Admin/UpdateUserStatusCommandHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Admin/GetUserByIdQueryHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Admin/GetUsersQueryHandlerTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 226/226 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 formatting discrepancies.
- **Next action:**
  - Frontend integration for Admin User Management interface.

## 2026-11-04 — Remove Duplicate Route on Admin UpdateStatus Endpoint

- **Status:** Completed.
- **Context & Motivation:** Swagger UI displayed two duplicate endpoints (`PUT /api/admin/users/{id}` and `PUT /api/admin/users/{id}/status`) because both route attributes were previously placed on the single `UpdateStatus` action method.
- **Completed:**
  - Removed `[HttpPut("{id:guid}")]` from `AdminUsersController`, keeping only `[HttpPut("{id:guid}/status")]` to maintain a single, explicit RESTful contract.
- **Changed paths:**
  - `src/Tillegal.Api/Controllers/AdminUsersController.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build src/Tillegal.Api/Tillegal.Api.csproj`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests`: 226/226 passed.
  - `dotnet format src/Tillegal.Api/Tillegal.Api.csproj --verify-no-changes`: Passed.


## 2026-11-04 — Refactor AI Evaluation to Conversation Branch and Restrict Admin Permissions

- **Status:** Completed.
- **Context & Motivation:**
  - Security enhancement: Eliminated message-level evaluation (`POST /api/ai-evaluations` requiring `messageId`) to prevent exposing internal message IDs.
  - Architectural alignment: Associated evaluations with conversations (`ConversationId`) as a sub-resource branch (`/api/conversations/{conversationId}/evaluations`), allowing multiple evaluations per conversation.
  - Role-based access control: Admin is restricted to view-only management rights (cannot chat, create conversations, post messages, create evaluations, or modify/delete user evaluations). Users can view only their own evaluations, whereas Admin can view all evaluations classified by evaluator role (Lawyer/User) with filtering and aggregated summary metrics.
- **Completed:**
  - **Domain:**
    - Modified `AiEvaluationLog` entity: replaced `MessageId` with `ConversationId`, added `EvaluatorRole` property, updated factory `Create` and constructors.
    - Updated `IAiEvaluationRepository` and `IAiEvaluationService`: replaced message queries with `GetByConversationIdAsync`, added `evaluatorRole` filtering to paged queries/counts, and enriched summary tuple with lawyer and user counts.
    - Updated `AiEvaluationService` implementation.
  - **Persistence:**
    - Replaced `MessageId` and message navigation in `AiEvaluationLogDataModel` with `ConversationId` and `Conversation` navigation; configured foreign key to `Conversations` table with cascade delete and indexing.
    - Added `AiEvaluationLogs` collection navigation to `ConversationDataModel` and removed it from `ConversationMessageDataModel`.
    - In `AiEvaluationRepository`: included `EvaluatorUser.Role` for evaluator role resolution ("Lawyer" / "User"); added `evaluatorRole` query filter; added `LawyerEvaluationsCount` and `UserEvaluationsCount` to `GetSummaryAsync`.
    - Updated seed generator in `ComprehensiveDatabaseDataSeed.cs` to link evaluations to `ConversationId`.
  - **Application:**
    - Refactored `AiEvaluationDto` and `AiEvaluationSummaryDto` to carry `ConversationId`, `EvaluatorRole`, and role metrics.
    - Updated `CreateEvaluationCommand` and validator to reference `ConversationId`; handler enforces that Admin cannot create evaluations, verifies conversation ownership, and permits multiple evaluations per conversation.
    - Enforced that Admin cannot edit or delete user evaluations in `UpdateEvaluationCommandHandler` and `DeleteEvaluationCommandHandler`.
    - Enforced that Admin cannot chat (creating conversations, posting messages, updating messages, deleting messages in `CreateConversationCommandHandler`, `CreateMessageCommandHandler`, `UpdateMessageCommandHandler`, `DeleteMessageCommandHandler`).
    - Added `GetEvaluationsByConversationIdQuery` (validator & handler) replacing obsolete `GetEvaluationByMessageId`.
    - Updated `GetAiEvaluationsQueryHandler` so non-admin users only view their own evaluations while Admins can view all with role-based filtering.
  - **API:**
    - Removed `POST /api/ai-evaluations` and `GET /api/ai-evaluations/message/{messageId}` from `AiEvaluationsController`.
    - Added `POST /api/conversations/{conversationId}/evaluations` and `GET /api/conversations/{conversationId}/evaluations` to `ConversationsController`.
    - Added `evaluatorRole` query parameter to `GET /api/ai-evaluations`.
    - Updated contracts `CreateAiEvaluationApiRequest`, `AiEvaluationApiResponse`, `AiEvaluationSummaryApiResponse`.
  - **Tests:**
    - Updated domain tests (`AiEvaluationLogTests.cs`), validator tests (`AiEvaluationCommandValidatorTests.cs`), mapping tests (`AiEvaluationMappingTests.cs`), and query/command handler test suites.
    - Added test cases verifying Admin chat and evaluation restrictions.
- **Changed paths:**
  - `src/Tillegal.Domain/Entities/AiEvaluationLog.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IAiEvaluationRepository.cs`
  - `src/Tillegal.Domain/Interfaces/Services/IAiEvaluationService.cs`
  - `src/Tillegal.Domain/Services/AiEvaluationService.cs`
  - `src/Tillegal.Persistence/DataModels/AiEvaluationLogDataModel.cs`
  - `src/Tillegal.Persistence/DataModels/ConversationDataModel.cs`
  - `src/Tillegal.Persistence/DataModels/ConversationMessageDataModel.cs`
  - `src/Tillegal.Persistence/Configurations/AiEvaluationLogConfiguration.cs`
  - `src/Tillegal.Persistence/Repositories/AiEvaluationRepository.cs`
  - `src/Tillegal.Persistence/SeedData/ComprehensiveDatabaseDataSeed.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Models/AiEvaluationDto.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Models/AiEvaluationSummaryDto.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Commands/CreateEvaluation/`
  - `src/Tillegal.Application/Features/AiEvaluations/Commands/UpdateEvaluation/`
  - `src/Tillegal.Application/Features/AiEvaluations/Commands/DeleteEvaluation/`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetAiEvaluations/`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetAiEvaluationSummary/`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetEvaluationsByConversationId/`
  - `src/Tillegal.Application/Features/Conversations/Commands/CreateConversation/`
  - `src/Tillegal.Application/Features/Conversations/Commands/UpdateConversation/`
  - `src/Tillegal.Application/Features/Conversations/Commands/DeleteConversation/`
  - `src/Tillegal.Application/Features/Messages/Commands/CreateMessage/`
  - `src/Tillegal.Application/Features/Messages/Commands/UpdateMessage/`
  - `src/Tillegal.Application/Features/Messages/Commands/DeleteMessage/`
  - `src/Tillegal.Api/Contracts/AiEvaluations/AiEvaluationContracts.cs`
  - `src/Tillegal.Api/Mappings/AiEvaluationApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/AiEvaluationsController.cs`
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/AiEvaluationLogTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/AiEvaluations/`
  - `tests/Tillegal.Application.UnitTests/Features/Conversations/`
  - `tests/Tillegal.Application.UnitTests/Features/Messages/`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build src/Tillegal.Api/Tillegal.Api.csproj`: Succeeded (0 warnings, 0 errors).
  - `dotnet build src/Tillegal.Persistence/Tillegal.Persistence.csproj`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 233/233 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 discrepancies.
- **Next action:**
  - Update frontend chat interface to call `POST /api/conversations/{conversationId}/evaluations` and display evaluations by conversation.

## 2026-11-04 — Generate EF Core Migration for AiEvaluationLog Conversation Association

- **Context / Problem:**
  - Running `Tillegal.AppHost` threw `Microsoft.EntityFrameworkCore.Migrations.PendingModelChangesWarning` during `dbContext.Database.MigrateAsync()` because `AiEvaluationLog` was refactored to relate to `Conversations` instead of `ConversationMessages`, but no migration had been generated.
- **Completed:**
  - Generated EF Core migration `20261104161031_ChangeAiEvaluationToConversation` which drops `FK_AiEvaluationLogs_ConversationMessages_MessageId`, renames `MessageId` to `ConversationId`, renames index `IX_AiEvaluationLogs_MessageId` to `IX_AiEvaluationLogs_ConversationId`, and adds `FK_AiEvaluationLogs_Conversations_ConversationId` referencing `Conversations(Id)`.
  - Added data backfill SQL in migration `Up` to update `ConversationId` values from `ConversationMessages.ConversationId` and cleanup orphaned records before applying the new FK constraint (preventing foreign key violation error `23503`).
  - Executed migration successfully against database via `dotnet ef database update`.
- **Changed paths:**
  - `src/Tillegal.Persistence/Migrations/20261104161031_ChangeAiEvaluationToConversation.cs`
  - `src/Tillegal.Persistence/Migrations/20261104161031_ChangeAiEvaluationToConversation.Designer.cs`
  - `src/Tillegal.Persistence/Migrations/AppDbContextModelSnapshot.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build --no-restore`: Succeeded (0 warnings, 0 errors).
  - `dotnet test --no-restore`: All 233 unit tests and 7 architecture tests passed.
  - `dotnet ef database update`: Succeeded and recorded in `__EFMigrationsHistory`.
- **Next action:**
  - Run `Tillegal.AppHost` without database migration errors.

## 2026-11-05 — Remove Legacy AI Evaluation Feature for Clean Redesign

- **Status:** Completed.
- **Context & Motivation:**
  - The previous AI evaluation implementation was reported as incorrect / needing a complete reimplementation.
  - Requested action: Completely remove all components and references of the old AI evaluation implementation across API, Application, Domain, Persistence, Seed, and Tests layers so the feature can be re-architected and coded cleanly.
- **Completed:**
  - **API:**
    - Removed `AiEvaluationsController.cs`, `AiEvaluationContracts.cs`, and `AiEvaluationApiMappingConfig.cs`.
    - Removed AI evaluation endpoints (`POST /api/conversations/{conversationId}/evaluations` and `GET /api/conversations/{conversationId}/evaluations`) and related using directives from `ConversationsController.cs`.
  - **Application:**
    - Removed entire `Features/AiEvaluations` folder including commands (`CreateEvaluation`, `UpdateEvaluation`, `DeleteEvaluation`), queries (`GetAiEvaluations`, `GetAiEvaluationSummary`, `GetEvaluationById`, `GetEvaluationByMessageId`), DTOs, and mappings.
    - Removed `IAiEvaluationService` registration from `DependencyInjection.cs`.
  - **Domain:**
    - Removed `AiEvaluationLog.cs` entity, `IAiEvaluationRepository.cs`, `IAiEvaluationService.cs`, and `AiEvaluationService.cs`.
  - **Persistence:**
    - Removed `AiEvaluationLogDataModel.cs`, `AiEvaluationLogConfiguration.cs`, and `AiEvaluationRepository.cs`.
    - Removed `DbSet<AiEvaluationLogDataModel> AiEvaluationLogs` from `AppDbContext.cs`.
    - Removed `AiEvaluationLogs` navigation collection from `ConversationDataModel.cs`, `ConversationMessageDataModel.cs`, and `UserDataModel.cs`.
    - Removed `IAiEvaluationRepository` registration from `DependencyInjection.cs`.
    - Removed `SeedAiEvaluationLogsAsync` and evaluation audit logs from `ComprehensiveDatabaseDataSeed.cs`.
    - Cleaned up untracked temporary migration files `20261104161031_ChangeAiEvaluationToConversation.*` and restored `AppDbContextModelSnapshot.cs` to HEAD.
  - **Tests:**
    - Removed `AiEvaluationLogTests.cs` domain tests and entire `tests/Tillegal.Application.UnitTests/Features/AiEvaluations/` test folder.
- **Changed paths:**
  - `src/Tillegal.Api/Contracts/AiEvaluations/` (deleted)
  - `src/Tillegal.Api/Controllers/AiEvaluationsController.cs` (deleted)
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
  - `src/Tillegal.Api/Mappings/AiEvaluationApiMappingConfig.cs` (deleted)
  - `src/Tillegal.Application/DependencyInjection.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/` (deleted)
  - `src/Tillegal.Domain/Entities/AiEvaluationLog.cs` (deleted)
  - `src/Tillegal.Domain/Interfaces/Repositories/IAiEvaluationRepository.cs` (deleted)
  - `src/Tillegal.Domain/Interfaces/Services/IAiEvaluationService.cs` (deleted)
  - `src/Tillegal.Domain/Services/AiEvaluationService.cs` (deleted)
  - `src/Tillegal.Persistence/Configurations/AiEvaluationLogConfiguration.cs` (deleted)
  - `src/Tillegal.Persistence/Context/AppDbContext.cs`
  - `src/Tillegal.Persistence/DataModels/AiEvaluationLogDataModel.cs` (deleted)
  - `src/Tillegal.Persistence/DataModels/ConversationDataModel.cs`
  - `src/Tillegal.Persistence/DataModels/ConversationMessageDataModel.cs`
  - `src/Tillegal.Persistence/DataModels/UserDataModel.cs`
  - `src/Tillegal.Persistence/DependencyInjection.cs`
  - `src/Tillegal.Persistence/Repositories/AiEvaluationRepository.cs` (deleted)
  - `src/Tillegal.Persistence/SeedData/ComprehensiveDatabaseDataSeed.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/AiEvaluationLogTests.cs` (deleted)
  - `tests/Tillegal.Application.UnitTests/Features/AiEvaluations/` (deleted)
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build src/Tillegal.Domain/Tillegal.Domain.csproj`: Succeeded (0 warnings, 0 errors).
  - `dotnet build src/Tillegal.Application/Tillegal.Application.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Infrastructure/Tillegal.Infrastructure.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Persistence/Tillegal.Persistence.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Api/Tillegal.Api.csproj`: Succeeded (0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 176/176 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
- **Next action:**
  - Ready for new requirements and re-implementation of AI evaluation.

## 2026-11-06 — Implement 2-Tier AI Evaluation Feature (Message Branch for Users & Dedicated Admin Reporting)

- **Status:** Completed.
- **Context & Motivation:**
  - Implemented the redesigned AI evaluation system according to updated requirements:
    1. **Tier 1 (User / Lawyer):** Evaluation is a sub-resource branch of messages (`/api/conversations/{conversationId}/messages/{messageId}/evaluation`), evaluating individual assistant messages (`role = 'assistant'`). Supports POST (create), GET (view own evaluation), PUT (update), and DELETE (soft delete). Users can only view and manage evaluations for their own conversations. Admin is strictly forbidden from evaluating or chatting.
    2. **Tier 2 (Admin):** Dedicated administrative management and summary reporting under `/api/admin/ai-evaluations`. Admin can view all evaluations (with pagination and filtering by helpfulness, rating, evaluator, and evaluator role) and view aggregated statistical summary metrics. Admin has view-only rights (no PUT/DELETE/POST).
- **Completed:**
  - **Domain:**
    - Created entity `AiEvaluationLog` linked to `MessageId` with validations (1 <= Rating <= 5, requires at least one evaluation attribute) and soft delete.
    - Defined `AiEvaluationSummary` model.
    - Defined `IAiEvaluationRepository` and `IAiEvaluationService` contracts.
    - Implemented `AiEvaluationService`.
  - **Persistence:**
    - Created `AiEvaluationLogDataModel` and configuration `AiEvaluationLogConfiguration`.
    - Restored `DbSet<AiEvaluationLogDataModel> AiEvaluationLogs` in `AppDbContext` and navigation properties on `ConversationMessageDataModel` and `UserDataModel`.
    - Implemented `AiEvaluationRepository` with message-level retrieval, admin paged querying with role resolution (Lawyer/User), and summary calculation with rating distribution.
    - Registered `IAiEvaluationRepository` in `DependencyInjection.cs`.
    - Added evaluation seeding for assistant messages in `ComprehensiveDatabaseDataSeed.cs`.
  - **Application:**
    - Models: `AiEvaluationDto`, `AiEvaluationSummaryDto`, `PagedAiEvaluationsDto`, and Mapster mapping `AiEvaluationMappingConfig`.
    - Commands:
      - `CreateMessageEvaluationCommand`: Enforces non-admin, conversation ownership, message exists in conversation, message is assistant, unique evaluation check, and persists evaluator role.
      - `UpdateMessageEvaluationCommand`: Enforces non-admin, conversation and evaluation ownership, validates input, and updates attributes.
      - `DeleteMessageEvaluationCommand`: Enforces non-admin, conversation and evaluation ownership, and marks evaluation as soft deleted.
    - Queries:
      - `GetMessageEvaluationQuery`: Returns evaluation for conversation owner only; blocks Admin and non-owners.
      - `GetAdminAiEvaluationsQuery`: Restricted to Admin; returns paged evaluations with filters.
      - `GetAdminAiEvaluationSummaryQuery`: Restricted to Admin; returns aggregated metrics and distribution.
    - Registered `IAiEvaluationService` in `DependencyInjection.cs`.
  - **API:**
    - Contracts: `CreateAiEvaluationApiRequest`, `UpdateAiEvaluationApiRequest`, `AiEvaluationApiResponse`, `AiEvaluationSummaryApiResponse`, `PagedAiEvaluationsApiResponse`.
    - Mappings: `AiEvaluationApiMappingConfig`.
    - `ConversationsController`: Added message evaluation endpoints (`POST`, `GET`, `PUT`, `DELETE` at `{conversationId}/messages/{messageId}/evaluation`).
    - `AdminAiEvaluationsController`: Added view-only endpoints (`GET /api/admin/ai-evaluations`, `GET /api/admin/ai-evaluations/summary`) protected with `[Authorize(Roles = "Admin")]`.
  - **Tests:**
    - Added domain unit tests (`AiEvaluationLogTests.cs`).
    - Added command handler tests (`CreateMessageEvaluationCommandHandlerTests.cs`, `UpdateMessageEvaluationCommandHandlerTests.cs`, `DeleteMessageEvaluationCommandHandlerTests.cs`).
    - Added query handler tests (`GetMessageEvaluationQueryHandlerTests.cs`, `GetAdminAiEvaluationsQueryHandlerTests.cs`, `GetAdminAiEvaluationSummaryQueryHandlerTests.cs`).
    - Added validator tests (`MessageEvaluationCommandValidatorTests.cs`).
    - Added mapping tests (`AiEvaluationMappingTests.cs`).
- **Changed paths:**
  - `src/Tillegal.Domain/Entities/AiEvaluationLog.cs`
  - `src/Tillegal.Domain/Models/AiEvaluationSummary.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IAiEvaluationRepository.cs`
  - `src/Tillegal.Domain/Interfaces/Services/IAiEvaluationService.cs`
  - `src/Tillegal.Domain/Services/AiEvaluationService.cs`
  - `src/Tillegal.Persistence/DataModels/AiEvaluationLogDataModel.cs`
  - `src/Tillegal.Persistence/Configurations/AiEvaluationLogConfiguration.cs`
  - `src/Tillegal.Persistence/Context/AppDbContext.cs`
  - `src/Tillegal.Persistence/DataModels/ConversationMessageDataModel.cs`
  - `src/Tillegal.Persistence/DataModels/UserDataModel.cs`
  - `src/Tillegal.Persistence/Repositories/AiEvaluationRepository.cs`
  - `src/Tillegal.Persistence/DependencyInjection.cs`
  - `src/Tillegal.Persistence/SeedData/ComprehensiveDatabaseDataSeed.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/`
  - `src/Tillegal.Application/DependencyInjection.cs`
  - `src/Tillegal.Api/Contracts/AiEvaluations/AiEvaluationContracts.cs`
  - `src/Tillegal.Api/Mappings/AiEvaluationApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
  - `src/Tillegal.Api/Controllers/AdminAiEvaluationsController.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/AiEvaluationLogTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/AiEvaluations/`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build src/Tillegal.Domain/Tillegal.Domain.csproj`: Succeeded (0 warnings, 0 errors).
  - `dotnet build src/Tillegal.Application/Tillegal.Application.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Persistence/Tillegal.Persistence.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Api/Tillegal.Api.csproj`: Succeeded (0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 212/212 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Passed (0 formatting errors).
- **Next action:**
  - None required. Feature implementation is complete and verified.

## 2026-11-06 — Apply EF Core Migration to Restore MessageId Column in PostgreSQL

- **Context / Problem:**
  - Running `dotnet run` / `Tillegal.AppHost` threw PostgreSQL error `column "MessageId" of relation "AiEvaluationLogs" does not exist`.
  - Cause: In a previous uncommitted change on 2026-11-04, migration `20261104161031_ChangeAiEvaluationToConversation` had been executed on the local PostgreSQL database, which renamed `MessageId` to `ConversationId` and recorded that migration in `__EFMigrationsHistory`. When that migration file was deleted, EF Core did not run any step to revert the column back to `MessageId`.
- **Completed:**
  - Restored `20261104161031_ChangeAiEvaluationToConversation` migration to preserve migration history continuity.
  - Added new migration `20261106110000_RevertAiEvaluationToMessage` that safely checks if `ConversationId` exists, drops the old conversation FK, renames `ConversationId` to `MessageId`, re-links evaluations to assistant messages, and adds foreign key `FK_AiEvaluationLogs_ConversationMessages_MessageId`.
  - Executed `dotnet ef database update --project src/Tillegal.Persistence/Tillegal.Persistence.csproj --startup-project src/Tillegal.AppHost/Tillegal.AppHost.csproj` which successfully applied the migration and updated `__EFMigrationsHistory`.
- **Changed paths:**
  - `src/Tillegal.Persistence/Migrations/20261104161031_ChangeAiEvaluationToConversation.cs`
  - `src/Tillegal.Persistence/Migrations/20261104161031_ChangeAiEvaluationToConversation.Designer.cs`
  - `src/Tillegal.Persistence/Migrations/20261106110000_RevertAiEvaluationToMessage.cs`
  - `src/Tillegal.Persistence/Migrations/20261106110000_RevertAiEvaluationToMessage.Designer.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet ef database update`: Applied `20261106110000_RevertAiEvaluationToMessage` successfully.
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 212/212 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
- **Next action:**
  - Run `Tillegal.AppHost` cleanly without database schema errors.

## 2026-11-06 — Implement Admin Get AI Evaluation by ID

- **Status:** Completed and verified.
- **Completed:**
  - Implemented `GetAdminAiEvaluationByIdQuery`, `GetAdminAiEvaluationByIdQueryValidator`, and `GetAdminAiEvaluationByIdQueryHandler` in Application layer.
  - Restricted query to authenticated users with `Admin` role; throws `ForbiddenAccessException` if non-admin or unauthenticated.
  - Calls `IAiEvaluationService.GetEvaluationByIdAsync(id)`, throwing `NotFoundException` if not found, and maps to `AiEvaluationDto`.
  - Added `[HttpGet("{id:guid}")]` endpoint to `AdminAiEvaluationsController` returning `AiEvaluationApiResponse` (HTTP 200, 401, 403, 404).
  - Added unit tests in `GetAdminAiEvaluationByIdQueryHandlerTests.cs` and validation tests in `MessageEvaluationCommandValidatorTests.cs`.
- **Changed paths:**
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetAdminAiEvaluationById/GetAdminAiEvaluationByIdQuery.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetAdminAiEvaluationById/GetAdminAiEvaluationByIdQueryValidator.cs`
  - `src/Tillegal.Application/Features/AiEvaluations/Queries/GetAdminAiEvaluationById/GetAdminAiEvaluationByIdQueryHandler.cs`
  - `src/Tillegal.Api/Controllers/AdminAiEvaluationsController.cs`
  - `tests/Tillegal.Application.UnitTests/Features/AiEvaluations/GetAdminAiEvaluationByIdQueryHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/AiEvaluations/MessageEvaluationCommandValidatorTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 217/217 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format --no-restore --include ... --verify-no-changes`: Passed with 0 formatting changes.
- **Next action:**
  - None required. Admin evaluation by ID query endpoint is fully operational.

## 2026-11-07 — Implement User File Upload and Isolated Management

- **Status:** Completed and verified.
- **Context & Constraints:**
  - Implemented file upload, retrieval, download, and deletion functionality for users.
  - Strict privacy/security requirement: Admins must NOT be able to view, access, download, or manage user files.
- **Completed:**
  - **Domain:**
    - Refactored `UserAttachment` into encapsulated DDD entity with `Create(...)` factory method, validation invariants, `UpdateProcessingStatus`, and `SoftDelete()`.
    - Added `IUserAttachmentRepository` interface.
    - Added `IUserAttachmentService` interface and `UserAttachmentService` implementation.
  - **Persistence:**
    - Implemented `UserAttachmentRepository` with mapping to/from `UserAttachmentDataModel`, filtering soft-deleted records, and registering in `DependencyInjection.cs`.
  - **Application:**
    - Models: `AttachmentDto`, `AttachmentDownloadDto`.
    - Mapping: `AttachmentMappingConfig`.
    - Commands:
      - `UploadAttachmentCommand`: Uploads to `IStorageService` (Supabase Storage), creates `UserAttachment`, validates ownership and blocks `Admin` with `ForbiddenAccessException`.
      - `DeleteAttachmentCommand`: Deletes from `IStorageService`, soft-deletes `UserAttachment`, validates ownership and blocks `Admin` with `ForbiddenAccessException`.
    - Queries:
      - `GetAttachmentsByConversationIdQuery`: Retrieves attachments in a conversation for the owner; blocks `Admin` with `ForbiddenAccessException`.
      - `GetAttachmentByIdQuery`: Retrieves attachment metadata by id for the owner; blocks `Admin` with `ForbiddenAccessException`.
      - `DownloadAttachmentQuery`: Streams downloaded bytes/content-type for the owner; blocks `Admin` with `ForbiddenAccessException`.
      - `GetUserAttachmentsQuery`: Lists all attachments belonging to the authenticated user; blocks `Admin` with `ForbiddenAccessException`.
    - Registered `IUserAttachmentService` in `DependencyInjection.cs`.
  - **API:**
    - Contracts: `AttachmentApiResponse`.
    - Mapping: `AttachmentApiMappingConfig`.
    - Endpoints in `ConversationsController`:
      - `POST /api/conversations/{conversationId}/attachments` (multipart/form-data upload)
      - `GET /api/conversations/{conversationId}/attachments` (list attachments in conversation)
      - `GET /api/conversations/{conversationId}/attachments/{attachmentId}` (get metadata)
      - `GET /api/conversations/{conversationId}/attachments/{attachmentId}/download` (download file)
      - `DELETE /api/conversations/{conversationId}/attachments/{attachmentId}` (delete attachment)
    - Controller `AttachmentsController`:
      - `GET /api/attachments` (list all attachments of current user across conversations)
    - Admin controllers (`AdminUsersController`, `AdminAiEvaluationsController`) expose zero user attachments or file operations.
  - **Tests:**
    - Added domain unit tests: `UserAttachmentTests.cs` (11 tests).
    - Added command handler tests: `UploadAttachmentCommandHandlerTests.cs` (5 tests), `DeleteAttachmentCommandHandlerTests.cs` (7 tests).
    - Added query handler tests: `GetAttachmentQueriesHandlerTests.cs` (10 tests).
    - Added validator tests: `AttachmentCommandValidatorTests.cs` (12 tests).
    - Added mapping tests: `AttachmentMappingTests.cs` (1 test).
- **Changed paths:**
  - `src/Tillegal.Domain/Entities/UserAttachment.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IUserAttachmentRepository.cs`
  - `src/Tillegal.Domain/Interfaces/Services/IUserAttachmentService.cs`
  - `src/Tillegal.Domain/Services/UserAttachmentService.cs`
  - `src/Tillegal.Persistence/Repositories/UserAttachmentRepository.cs`
  - `src/Tillegal.Persistence/DependencyInjection.cs`
  - `src/Tillegal.Application/Features/Attachments/`
  - `src/Tillegal.Application/DependencyInjection.cs`
  - `src/Tillegal.Api/Contracts/Attachments/AttachmentContracts.cs`
  - `src/Tillegal.Api/Mappings/AttachmentApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
  - `src/Tillegal.Api/Controllers/AttachmentsController.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/UserAttachmentTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Attachments/`
  - `docs/development/backend-feature-backlog.md`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build src/Tillegal.Domain/Tillegal.Domain.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Persistence/Tillegal.Persistence.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Application/Tillegal.Application.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Api/Tillegal.Api.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.AppHost/Tillegal.AppHost.csproj`: Succeeded (0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 270/270 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Passed (0 formatting changes).
- **Next action:**
  - None required. File upload and user-isolated file management feature is complete and verified.

## 2026-11-07 — Enhance File Deletion with Hard Delete for Pending Attachments

- **Status:** Completed and verified.
- **Context & Requirement:**
  - When users accidentally upload a wrong file and delete it before submitting the question (`ProcessingStatus == "Pending"`), the file should be permanently purged (Hard Delete) from both storage and database to prevent orphan/junk records.
  - If the attachment was already submitted/processed (`ProcessingStatus != "Pending"`), soft-delete in database is preserved while cloud storage file is deleted.
- **Completed:**
  - **Domain:**
    - Updated `IUserAttachmentRepository` to declare `Task HardDeleteAsync(UserAttachment attachment, CancellationToken cancellationToken = default)`.
    - Enhanced `UserAttachmentService.DeleteAttachmentAsync`: checks `attachment.ProcessingStatus`. If `"Pending"`, calls `HardDeleteAsync`; otherwise marks `SoftDelete()` and calls `DeleteAsync`.
  - **Persistence:**
    - Implemented `HardDeleteAsync` in `UserAttachmentRepository` removing the `UserAttachmentDataModel` entity from `AppDbContext.UserAttachments`.
  - **Tests:**
    - Added `UserAttachmentServiceTests.cs` (6 unit tests) verifying conditional hard vs soft delete behavior and delegation logic.
- **Changed paths:**
  - `src/Tillegal.Domain/Interfaces/Repositories/IUserAttachmentRepository.cs`
  - `src/Tillegal.Domain/Services/UserAttachmentService.cs`
  - `src/Tillegal.Persistence/Repositories/UserAttachmentRepository.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/UserAttachmentServiceTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet build src/Tillegal.Domain/Tillegal.Domain.csproj`: Succeeded (0 errors).
  - `dotnet build src/Tillegal.Persistence/Tillegal.Persistence.csproj`: Succeeded (0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 276/276 passed (+6 tests).
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Passed (0 formatting changes).
- **Next action:**
  - None required. Hard delete for pending attachments is fully operational.

## 2026-11-07 — Enforce Maximum 5 Attachments Limit Per Conversation

- **Status:** Completed and verified.
- **Requirement:**
  - Limit the number of attachments per conversation to a maximum of 5 files.
- **Completed:**
  - **Application:**
    - Updated `UploadAttachmentCommandHandler`: Checks existing attachments via `IUserAttachmentService.GetAttachmentsByConversationIdAsync(conversationId)`.
    - If count is >= 5, throws `DomainException("A conversation can have at most 5 attachments.")` before attempting storage upload, mapping directly to `400 Bad Request`.
  - **Tests:**
    - Added unit test `Handle_WhenConversationAlreadyHas5Attachments_ShouldThrowDomainException` in `UploadAttachmentCommandHandlerTests.cs`.
- **Changed paths:**
  - `src/Tillegal.Application/Features/Attachments/Commands/UploadAttachment/UploadAttachmentCommandHandler.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Attachments/UploadAttachmentCommandHandlerTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 277/277 passed (+1 test).
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Passed (0 formatting changes).
- **Next action:**
  - None required. 5 attachments limit is fully operational and verified.

## 2026-11-07 — Add Duplicate File Check for Conversation Attachments

- **Status:** Completed and verified.
- **Requirement:**
  - Prevent duplicate file uploads within the same conversation (case-insensitive file name matching against active attachments).
- **Completed:**
  - **Application:**
    - Updated `UploadAttachmentCommandHandler`: Checks whether an active attachment with the same sanitized file name already exists in the conversation via `existingAttachments.Any(a => string.Equals(a.FileName, sanitizedFileName, StringComparison.OrdinalIgnoreCase))`.
    - If duplicate, throws `DomainException($"A file named '{sanitizedFileName}' already exists in this conversation.")` prior to storage upload (returning `400 Bad Request`).
  - **Tests:**
    - Added unit test `Handle_WhenFileWithSameNameAlreadyExistsInConversation_ShouldThrowDomainException` in `UploadAttachmentCommandHandlerTests.cs`.
- **Changed paths:**
  - `src/Tillegal.Application/Features/Attachments/Commands/UploadAttachment/UploadAttachmentCommandHandler.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Attachments/UploadAttachmentCommandHandlerTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 278/278 passed (+1 test).
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Passed (0 formatting changes).
- **Next action:**
  - None required. Duplicate file prevention is fully operational and verified.


## 2026-11-07 — Add SHA-256 Content Hash Duplicate Check and File Name Auto-Disambiguation

- **Status:** Completed and verified.
- **Requirement:**
  - Prevent uploading duplicate file content within the same conversation using SHA-256 hashing (`FileHash`), even if the file was renamed.
  - If content is distinct but file names collide (e.g. generic names like `image.png`), auto-disambiguate display names (`image (2).png`).
  - Max 5 files per conversation enforced.
- **Completed:**
  - **Domain:**
    - Added `FileHash` property (`string?`, hex string) to `UserAttachment` entity with factory `Create(...)` validation and constructor mapping.
    - Added `fileHash` parameter to `IUserAttachmentService.CreateAttachmentAsync` and `UserAttachmentService.CreateAttachmentAsync`.
  - **Persistence:**
    - Added `FileHash` to `UserAttachmentDataModel`.
    - Configured `FileHash` (`varchar(64)`) and index `IX_UserAttachments_FileHash` in `UserAttachmentConfiguration`.
    - Generated EF Core migration `20261107154004_AddFileHashToUserAttachment` and applied to PostgreSQL via `dotnet ef database update`.
    - Updated `UserAttachmentRepository` to map `FileHash`.
  - **Application & API:**
    - Added `FileHash` to `AttachmentDto` and `AttachmentResponse`.
    - Updated `UploadAttachmentCommandHandler`:
      - Computes SHA-256 hash using `SHA256.HashDataAsync`.
      - Checks if any existing attachment in the conversation has the same `FileHash`. If so, throws `DomainException("A file with identical content has already been uploaded to this conversation.")`.
      - If content differs but file name matches an existing file, auto-disambiguates using standard numbering (`file (2).pdf`).
  - **Tests:**
    - Updated `UserAttachmentServiceTests.cs` to test `CreateAttachmentAsync` with `FileHash`.
    - Updated `UploadAttachmentCommandHandlerTests.cs` to test duplicate content rejection and name disambiguation.
- **Changed paths:**
  - `src/Tillegal.Domain/Entities/UserAttachment.cs`
  - `src/Tillegal.Domain/Interfaces/Services/IUserAttachmentService.cs`
  - `src/Tillegal.Domain/Services/UserAttachmentService.cs`
  - `src/Tillegal.Persistence/DataModels/UserAttachmentDataModel.cs`
  - `src/Tillegal.Persistence/Configurations/UserAttachmentConfiguration.cs`
  - `src/Tillegal.Persistence/Repositories/UserAttachmentRepository.cs`
  - `src/Tillegal.Persistence/Migrations/20261107154004_AddFileHashToUserAttachment.cs`
  - `src/Tillegal.Persistence/Migrations/AppDbContextModelSnapshot.cs`
  - `src/Tillegal.Application/Features/Attachments/Models/AttachmentDto.cs`
  - `src/Tillegal.Application/Features/Attachments/Commands/UploadAttachment/UploadAttachmentCommandHandler.cs`
  - `src/Tillegal.Api/Contracts/Attachments/AttachmentContracts.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/UserAttachmentServiceTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Attachments/UploadAttachmentCommandHandlerTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tillegal-be.slnx`: 286/286 passed (279 unit tests + 7 architecture tests).
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Succeeded (0 formatting violations).
- **Next action:**
  - None required. SHA-256 duplicate detection and name disambiguation are fully operational.


## 2026-11-07 — Remove GET /api/attachments Endpoint

- **Status:** Completed and verified.
- **Requirement:**
  - Remove standalone endpoint `GET /api/attachments` as requested by user. Attachments are managed strictly within conversation context under `/api/conversations/{conversationId}/attachments`.
- **Completed:**
  - Removed `src/Tillegal.Api/Controllers/AttachmentsController.cs`.
  - Removed unused query `GetUserAttachmentsQuery` and handler `GetUserAttachmentsQueryHandler`.
  - Removed obsolete unit tests for `GetUserAttachmentsQuery` in `GetAttachmentQueriesHandlerTests.cs`.
- **Changed paths:**
  - `src/Tillegal.Api/Controllers/AttachmentsController.cs` (deleted)
  - `src/Tillegal.Application/Features/Attachments/Queries/GetUserAttachments/GetUserAttachmentsQuery.cs` (deleted)
  - `src/Tillegal.Application/Features/Attachments/Queries/GetUserAttachments/GetUserAttachmentsQueryHandler.cs` (deleted)
  - `tests/Tillegal.Application.UnitTests/Features/Attachments/GetAttachmentQueriesHandlerTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tillegal-be.slnx`: 284/284 passed (277 unit tests + 7 architecture tests).
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Succeeded (0 formatting violations).
- **Next action:**
  - None required. Endpoint successfully removed.


## 2026-11-10 — Add Admin Grant Lawyer Role Feature

- **Status:** Completed and verified.
- **Requirement:**
  - Provide an administrative feature allowing administrators to grant the lawyer role to standard users.
- **Completed:**
  - **Domain & Persistence:**
    - Added `GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)` to `IUserRepository` and `UserRepository` with case-insensitive matching handling both "Lawyer" and legacy seed name "Lawer".
    - Updated `UserRepository.UpdateAsync` to persist `RoleId`, `FullName`, and `PhoneNumber` changes in addition to `DeletedAt` (status).
    - Updated `AdminUserMappingConfig` to normalize role name `"Lawer"` to `"Lawyer"` in `UserDto`.
  - **Application Layer:**
    - Created `GrantLawyerRoleCommand` (`Guid UserId`).
    - Created `GrantLawyerRoleCommandValidator` ensuring `UserId` is not empty.
    - Created `GrantLawyerRoleCommandHandler` enforcing business rules:
      - Authenticated user check (`UnauthorizedException`).
      - Admin role check (`ForbiddenAccessException`).
      - Prevent administrators from modifying their own role (`DomainException`).
      - Target user existence check (`NotFoundException`).
      - Protection preventing administrators from having their role changed (`DomainException`).
      - Guard preventing users who already have the Lawyer role from being granted again (`DomainException`).
      - Guard preventing inactive or locked users from receiving lawyer privileges (`DomainException`).
      - Resolution of lawyer role and persistence of updated `RoleId` and `Role`.
  - **API Layer:**
    - Added endpoint `POST /api/admin/users/{id:guid}/grant-lawyer` to `AdminUsersController` returning updated `AdminUserApiResponse`.
  - **Tests:**
    - Added `GrantLawyerRoleCommandHandlerTests` (9 test cases covering all edge cases, exceptions, and happy path).
    - Added `GrantLawyerRoleCommandValidatorTests` (2 test cases).
    - Added `AdminUserMappingTests` (2 test cases verifying "Lawer" -> "Lawyer" normalization and standard role mapping).
- **Changed paths:**
  - `src/Tillegal.Domain/Interfaces/Repositories/IUserRepository.cs`
  - `src/Tillegal.Persistence/Repositories/UserRepository.cs`
  - `src/Tillegal.Application/Features/Admin/Users/Mappings/AdminUserMappingConfig.cs`
  - `src/Tillegal.Application/Features/Admin/Users/Commands/GrantLawyerRole/GrantLawyerRoleCommand.cs`
  - `src/Tillegal.Application/Features/Admin/Users/Commands/GrantLawyerRole/GrantLawyerRoleCommandValidator.cs`
  - `src/Tillegal.Application/Features/Admin/Users/Commands/GrantLawyerRole/GrantLawyerRoleCommandHandler.cs`
  - `src/Tillegal.Api/Controllers/AdminUsersController.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Admin/GrantLawyerRoleCommandHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Admin/GrantLawyerRoleCommandValidatorTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Admin/AdminUserMappingTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tillegal-be.slnx`: 298/298 passed (291 unit tests + 7 architecture tests).
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Succeeded (0 formatting violations).
- **Next action:**
  - None required. Feature is complete and verified.

## 2026-11-11 — Support Direct Google Sign-In Without Mandatory Separate Registration Step

- **Status:** Completed and verified.
- **Requirement:**
  - Modify Google login (`POST /api/auth/login/google`) so that if a user authenticates with Google, they can log in directly without requiring a separate manual registration step.
- **Completed:**
  - **Application Layer (`LoginWithGoogleCommandHandler`):**
    - Updated `LoginWithGoogleCommandHandler` to check if an account with the Google `sub` exists.
    - If not found, checks if an account exists with the verified Google email:
      - If found with no Google subject linked, automatically links `user.GoogleSubject` and persists the update via `IUnitOfWork`.
      - If found with a different Google subject, rejects with `DomainException` to prevent account hijacking.
    - If no account exists with either Google subject or email, automatically creates a new passwordless user with default role `"User"`, verified email, and full name (or email prefix fallback), persisting the entity via `IUnitOfWork`.
    - Generates Tillegal JWT and returns `GoogleLoginResult` for all successful flows.
  - **Persistence Layer (`UserRepository`):**
    - Updated `GetByGoogleSubjectAsync` to include `Role` navigation property (`.Include(u => u.Role)`).
    - Updated `UpdateAsync` to persist `dataModel.GoogleSubject = user.GoogleSubject`.
  - **Documentation:**
    - Updated `docs/api/authentication.md` to document immediate Google login, automatic subject linking for existing verified emails, and automatic account registration without a separate registration step.
  - **Tests:**
    - Updated and expanded `LoginWithGoogleCommandHandlerTests` with 8 comprehensive test cases covering:
      - Existing Google subject login.
      - Automatic user registration when account does not exist.
      - Automatic Google subject linking when user exists by email without subject.
      - Conflict rejection when email is associated with a different Google subject.
      - Unverified email rejection.
      - Missing subject rejection.
      - Missing email rejection.
      - Fallback to email username prefix when full name is empty.
- **Changed paths:**
  - `src/Tillegal.Application/Features/Auth/Commands/LoginWithGoogle/LoginWithGoogleCommandHandler.cs`
  - `src/Tillegal.Persistence/Repositories/UserRepository.cs`
  - `docs/api/authentication.md`
  - `tests/Tillegal.Application.UnitTests/Features/Auth/LoginWithGoogleCommandHandlerTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 297/297 tests passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 tests passed.
- **Next action:**
  - None required. Feature is complete and verified.

## 2026-11-11 — Update Admin Grant Lawyer Role HTTP Method to PUT

- **Status:** Completed and verified.
- **Requirement:**
  - Update the admin endpoint for granting the lawyer role to a user from `POST` to `PUT` (`PUT /api/admin/users/{id:guid}/grant-lawyer`).
- **Completed:**
  - **API Layer:**
    - Updated `AdminUsersController.GrantLawyerRole` HTTP attribute from `[HttpPost("{id:guid}/grant-lawyer")]` to `[HttpPut("{id:guid}/grant-lawyer")]`.
- **Changed paths:**
  - `src/Tillegal.Api/Controllers/AdminUsersController.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tillegal-be.slnx`: 304/304 passed (297 unit tests + 7 architecture tests).
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Succeeded (0 formatting violations).
- **Next action:**
  - Commit and push changes to remote repository.

## 2026-11-13 — Fix Supabase Storage Upload Failure for Vietnamese and Non-ASCII File Names

- **Status:** Completed and verified.
- **Requirement:**
  - Resolve upload failure returning HTTP 500 (`Response status code does not indicate success: 400 (Bad Request)`) when users upload files containing Vietnamese accented characters (e.g. `Ảnh1.png`), spaces, parentheses, or special characters.
- **Completed:**
  - **Application Layer (`UploadAttachmentCommandHandler`):**
    - Maintained original file name with full Unicode/Vietnamese support (`finalFileName`) in `UserAttachment.FileName` and DB for display and download.
    - Implemented `BuildSafeStorageFileName` and `SanitizeAsciiSegment` converting Vietnamese diacritics into ASCII equivalents (`Ảnh1` -> `Anh1`, `đ` -> `d`), replacing spaces/symbols with hyphens, and preserving safe alphanumeric characters for the Supabase storage object key.
  - **Infrastructure Layer (`SupabaseStorageService`):**
    - Handled robust `ContentType` parsing using `MediaTypeHeaderValue.TryParse` with a safe fallback to `application/octet-stream`.
    - Added `EnsureSuccessWithDetailsAsync` capturing the exact error body from Supabase Storage responses in exception messages instead of a generic status code failure.
  - **Tests:**
    - Added parameterized unit tests in `UploadAttachmentCommandHandlerTests.cs` covering Vietnamese file names (`Ảnh1.png`, `Đề cương hợp đồng (nháp).pdf`) and name auto-disambiguation formats.
- **Changed paths:**
  - `src/Tillegal.Application/Features/Attachments/Commands/UploadAttachment/UploadAttachmentCommandHandler.cs`
  - `src/Tillegal.Infrastructure/ExternalServices/Storage/SupabaseStorageService.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Attachments/UploadAttachmentCommandHandlerTests.cs`
  - `docs/development/agent-worklog.md`
- **Checks run:**
  - `dotnet test tests/Tillegal.Application.UnitTests/Tillegal.Application.UnitTests.csproj`: 299/299 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/Tillegal.ArchitectureTests.csproj`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --no-restore --verify-no-changes`: Succeeded (0 discrepancies).
- **Next action:**
  - Restart backend server and retest uploading `Ảnh1.png` in Swagger.


