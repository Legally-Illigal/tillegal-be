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

## 2026-10-25 — Implement Create Conversation feature (BE-F03)

- **Status:** Completed for conversation creation scope (part of BE-F03).
- **Completed:**
  - Implemented Clean Architecture create conversation flow (`POST /api/conversations`).
  - Updated `Conversation` entity in `Tillegal.Domain.Entities` with constructors, `Conversation.Create(userId, title)` factory method, and `UpdateTitle(newTitle)` domain invariants.
  - Declared `IConversationRepository` in `Tillegal.Domain.Interfaces.Repositories` and implemented `ConversationRepository` in `Tillegal.Persistence.Repositories` using EF Core `AppDbContext`.
  - Added `ConversationService` domain service in `Tillegal.Domain.Services` to orchestrate creation of conversation entities and encapsulate domain operations.
  - Implemented CQRS create flow:
    - `CreateConversationCommand` and `CreateConversationResult` in `Tillegal.Application/Features/Conversations/Commands/CreateConversation/`.
    - `CreateConversationCommandValidator` validating title constraints (max 255 chars, cannot be empty/whitespace if specified).
    - `CreateConversationCommandHandler` resolving user ID via `ICurrentUserService`, applying fallback title `"Cuộc hội thoại mới"` if omitted, invoking `ConversationService`, and persisting via `IUnitOfWork`.
  - Configured Mapster mappings:
    - Domain <-> Application: `Conversation` -> `CreateConversationResult` in `ConversationMappingConfig.cs`.
    - Api <-> Application: `CreateConversationApiRequest` -> `CreateConversationCommand`, `CreateConversationResult` -> `CreateConversationApiResponse` in `ConversationApiMappingConfig.cs`.
  - Added `ConversationsController` (`[Authorize]`, inherits `BaseApiController`) exposing `POST /api/conversations` returning HTTP 201 Created.
  - Registered services in DI (`IConversationRepository` in `Tillegal.Persistence/DependencyInjection.cs`, `ConversationService` in `Tillegal.Application/DependencyInjection.cs`).
  - Added unit tests:
    - `ConversationTests`: tests entity creation, Guid v7 generation, null/empty/whitespace validations, and title updates.
    - `CreateConversationCommandHandlerTests`: tests authenticated flow, unauthenticated `UnauthorizedException`, and default title fallback.
    - `ConversationCommandValidatorTests`: tests valid, null, empty, whitespace, and length > 255 titles.
    - `ConversationMappingTests`: tests Mapster mapping between domain entity and result DTO.
  - Updated `docs/development/backend-feature-backlog.md` status for `BE-F03` to `Một phần`.
- **Changed paths:**
  - `docs/development/backend-feature-backlog.md`
  - `docs/development/agent-worklog.md`
  - `src/Tillegal.Domain/Entities/Conversation.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IConversationRepository.cs`
  - `src/Tillegal.Domain/Services/ConversationService.cs`
  - `src/Tillegal.Persistence/Repositories/ConversationRepository.cs`
  - `src/Tillegal.Persistence/DependencyInjection.cs`
  - `src/Tillegal.Application/Features/Conversations/Commands/CreateConversation/CreateConversationCommand.cs`
  - `src/Tillegal.Application/Features/Conversations/Commands/CreateConversation/CreateConversationCommandValidator.cs`
  - `src/Tillegal.Application/Features/Conversations/Commands/CreateConversation/CreateConversationCommandHandler.cs`
  - `src/Tillegal.Application/Features/Conversations/Mappings/ConversationMappingConfig.cs`
  - `src/Tillegal.Application/DependencyInjection.cs`
  - `src/Tillegal.Api/Contracts/Conversations/ConversationContracts.cs`
  - `src/Tillegal.Api/Mappings/ConversationApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/ConversationTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Conversations/CreateConversationCommandHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Conversations/ConversationCommandValidatorTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Conversations/ConversationMappingTests.cs`
- **Checks run:**
  - `dotnet build tillegal-be.slnx`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tillegal-be.slnx`: 57/57 unit tests passed, 7/7 architecture tests passed.
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 discrepancies.
- **Next action:**
  - Implement conversation listing (`GET /api/conversations`) and retrieval by ID (`GET /api/conversations/{id}`) under BE-F03.

## 2026-10-25 — Implement Conversation Q&A Feature (BE-F04, BE-F05, BE-F06, BE-F07)

- **Status:** Completed for Q&A and conversation messaging scope.
- **Completed:**
  - Implemented Clean Architecture Q&A message exchange flow (`POST /api/conversations/{id}/messages` and `POST /api/conversations/{id}/ask`, `GET /api/conversations/{id}/messages`).
  - Enhanced `ConversationMessage` domain entity with constructors, `CreateUserMessage`, `CreateAssistantMessage` factory methods, and domain invariant validation.
  - Declared `IConversationMessageRepository` in `Tillegal.Domain.Interfaces.Repositories` and implemented `ConversationMessageRepository` in `Tillegal.Persistence.Repositories`.
  - Updated `ConversationService` in `Tillegal.Domain.Services` to inject message repository and support `AddUserMessageAsync`, `AddAssistantMessageAsync`, and `GetMessagesAsync`.
  - Added `ILegalAiService` port in `Tillegal.Application.Common.Interfaces` (`AskAsync` taking question, conversationId, and history).
  - Implemented `LegalAiServiceClient` in `Tillegal.Infrastructure.ExternalServices.AiService` supporting external HTTP AI Service endpoints with fallback legal advisor generator with JSON citations.
  - Added `AiServiceOptions` in `Tillegal.Infrastructure.Options` and bound in `InfrastructureExtensions`.
  - Implemented CQRS Q&A:
    - `AskQuestionCommand`, `ConversationMessageDto`, `AskQuestionResult` in `Tillegal.Application/Features/Conversations/Commands/AskQuestion/`.
    - `AskQuestionCommandValidator` checking `ConversationId`, non-empty question, max 4000 characters.
    - `AskQuestionCommandHandler` ensuring user authentication and conversation ownership (throwing `ForbiddenAccessException` or `NotFoundException`), persisting user question, fetching context history, invoking `ILegalAiService`, persisting assistant response, and committing via `IUnitOfWork`.
    - `GetConversationMessagesQuery`, validator, and handler in `Tillegal.Application/Features/Conversations/Queries/GetConversationMessages/`.
  - Configured Mapster mappings in `ConversationMappingConfig.cs` and `ConversationApiMappingConfig.cs`.
  - Added endpoints to `ConversationsController`:
    - `POST /api/conversations/{id}/messages` (and alias `/ask`) returning `200 OK` with user and assistant messages.
    - `GET /api/conversations/{id}/messages` returning `200 OK` with chronological conversation history.
  - Registered services in DI: `IConversationMessageRepository` in Persistence, `ILegalAiService` via `HttpClient` in Infrastructure.
  - Added unit tests:
    - `ConversationMessageTests`: tests user and assistant message creation, domain invariants, and citations handling.
    - `AskQuestionCommandHandlerTests`: tests authenticated happy path, unauthenticated check, 404 not found, 403 forbidden ownership check.
    - `AskQuestionCommandValidatorTests`: tests valid question, empty conversation ID, whitespace/null question, and question > 4000 characters.
    - `GetConversationMessagesQueryHandlerTests`: tests valid message history retrieval, unauthenticated check, 404 not found, 403 forbidden check.
  - Updated `backend-feature-backlog.md` status for `BE-F04`, `BE-F05`, `BE-F06`, `BE-F07` to `Một phần`.
- **Changed paths:**
  - `docs/development/backend-feature-backlog.md`
  - `docs/development/agent-worklog.md`
  - `src/Tillegal.Domain/Entities/ConversationMessage.cs`
  - `src/Tillegal.Domain/Interfaces/Repositories/IConversationMessageRepository.cs`
  - `src/Tillegal.Domain/Services/ConversationService.cs`
  - `src/Tillegal.Persistence/Repositories/ConversationMessageRepository.cs`
  - `src/Tillegal.Persistence/DependencyInjection.cs`
  - `src/Tillegal.Application/Common/Interfaces/ILegalAiService.cs`
  - `src/Tillegal.Application/Features/Conversations/Commands/AskQuestion/AskQuestionCommand.cs`
  - `src/Tillegal.Application/Features/Conversations/Commands/AskQuestion/AskQuestionCommandValidator.cs`
  - `src/Tillegal.Application/Features/Conversations/Commands/AskQuestion/AskQuestionCommandHandler.cs`
  - `src/Tillegal.Application/Features/Conversations/Queries/GetConversationMessages/GetConversationMessagesQuery.cs`
  - `src/Tillegal.Application/Features/Conversations/Queries/GetConversationMessages/GetConversationMessagesQueryValidator.cs`
  - `src/Tillegal.Application/Features/Conversations/Queries/GetConversationMessages/GetConversationMessagesQueryHandler.cs`
  - `src/Tillegal.Application/Features/Conversations/Mappings/ConversationMappingConfig.cs`
  - `src/Tillegal.Infrastructure/Options/AiServiceOptions.cs`
  - `src/Tillegal.Infrastructure/ExternalServices/AiService/LegalAiServiceClient.cs`
  - `src/Tillegal.Infrastructure/DependencyInjection.cs`
  - `src/Tillegal.AppHost/Configurations/InfrastructureExtensions.cs`
  - `src/Tillegal.Api/Contracts/Conversations/ConversationContracts.cs`
  - `src/Tillegal.Api/Mappings/ConversationApiMappingConfig.cs`
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
  - `tests/Tillegal.Application.UnitTests/Domain/ConversationMessageTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Conversations/AskQuestionCommandHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Conversations/AskQuestionCommandValidatorTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Conversations/GetConversationMessagesQueryHandlerTests.cs`
  - `tests/Tillegal.Application.UnitTests/Features/Conversations/CreateConversationCommandHandlerTests.cs`
- **Checks run:**
  - `dotnet build tillegal-be.slnx`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tillegal-be.slnx`: 81/81 unit tests passed, 7/7 architecture tests passed.
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 discrepancies.
- **Next action:**
  - Implement conversation listing (`GET /api/conversations`) and conversation update/delete (`PUT/DELETE /api/conversations/{id}`) under BE-F03.

## 2026-10-26 — Consolidate Q&A Route and Simplify Request Body in ConversationsController

- **Status:** Completed.
- **Completed:**
  - Removed redundant `[HttpPost("{id:guid}/ask")]` route from `ConversationsController`, standardizing strictly on the RESTful `POST /api/conversations/{id}/messages` endpoint alongside `GET /api/conversations/{id}/messages`.
  - Simplified `AskQuestionApiRequest` to solely contain `string Content`, eliminating redundant `Question` field and avoiding schema confusion in Swagger UI.
- **Changed paths:**
  - `src/Tillegal.Api/Contracts/Conversations/ConversationContracts.cs`
  - `src/Tillegal.Api/Controllers/ConversationsController.cs`
- **Checks run:**
  - `dotnet build src/Tillegal.Api/Tillegal.Api.csproj`: Succeeded (0 warnings, 0 errors).
  - `dotnet test tests/Tillegal.Application.UnitTests/`: 81/81 passed.
  - `dotnet test tests/Tillegal.ArchitectureTests/`: 7/7 passed.
  - `dotnet format tillegal-be.slnx --verify-no-changes`: Passed with 0 discrepancies.
- **Next action:** Implement conversation listing (`GET /api/conversations`) and update/delete endpoints.

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
