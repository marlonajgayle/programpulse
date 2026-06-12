# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ProgramPulse is a single ASP.NET Core (`.NET 10`) minimal-API project (`src/ProgramPulse.Api`). It uses a vertical-slice architecture with a CQRS-flavored command/query + handler pattern, a `Result`/`Error` functional-error model, EF Core + SQL Server, ASP.NET Core Identity with JWT/cookie auth, and a transactional outbox for async work (e.g. emails). There is no separate test project yet.

## Commands

```bash
# Start dependencies (SQL Server on 1433, Mailpit SMTP on 1025 / web UI on 8025)
docker-compose up -d

# Run the API (defaults to Development env via launchSettings; see env note below)
dotnet run --project src/ProgramPulse.Api                       # http profile -> http://localhost:5017
dotnet run --project src/ProgramPulse.Api --launch-profile https # https://localhost:7093

# Build / restore
dotnet build
dotnet restore

# EF Core migrations (run from the project dir so the DbContext factory resolves)
dotnet ef migrations add <Name> --project src/ProgramPulse.Api
dotnet ef database update --project src/ProgramPulse.Api
```

Migrations are also applied automatically on startup by `DbInitializer.UseInitializeDatabaseAsync` (only when pending), which also seeds Identity roles.

API docs (Scalar UI) and OpenAPI are mapped via `MapApiDocumentation` and available in non-Production environments.

### Environment & configuration gotcha

`PersistenceConfiguration.AddPersistence` reads the connection string differently per environment:
- **Local** (and any env that isn't Development/Staging/Production): from `ConnectionStrings:DatabaseConnection` in `appsettings.Local.json`.
- **Development / Staging / Production**: from the `DATABASE_CONNECTION_STRING` environment variable (throws if missing).

`launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development`, so a plain `dotnet run` expects `DATABASE_CONNECTION_STRING` to be set. To use the Docker SQL Server with the appsettings connection string, run with `ASPNETCORE_ENVIRONMENT=Local`. Local email points at Mailpit (`localhost:1025`).

## Architecture

### Vertical slices (`Features/`)
Each feature folder (e.g. `Features/Faqs/Create`) is self-contained and typically holds three files:
- **`*Command.cs` / `*Query.cs`** — a `record` request plus its `*CommandHandler`/`*QueryHandler` class. Handlers take dependencies via primary constructors and expose `HandleAsync(request, ct)` returning `Result` / `Result<T>`.
- **`*Endpoint.cs`** — a class implementing `IEndpoint`. `MapEndpoint` maps the route, then chains `.HasApiVersion(ApiVersions.V1)`, `.WithValidation<TCommand>()`, `.RequireAuthorization(...)`, `.WithName(...)`, `.WithTags(...)`. The handler is injected as a route parameter and its result is returned via `result.ToHttpResult()`.
- **`*CommandValidator.cs`** — a FluentValidation `AbstractValidator<TCommand>`.

### Wiring (`Program.cs`)
- Endpoints are discovered by reflection (`AddEndpoints` scans for `IEndpoint`) and mapped under `api/v{version:apiVersion}` with a default IP fixed-window rate-limit policy (`MapApiEndpoints` in `SharedKernel/EndpointExtensions.cs`).
- **Handlers are NOT auto-registered** — each new handler must be added with `builder.Services.AddScoped<THandler>()` in `Program.cs`. Validators ARE auto-registered (`AddValidatorsFromAssemblyContaining<Program>`).

### Result / Error model (`SharedKernel/Primitives`)
Handlers never throw for expected failures — they return `Result.Failure(error)` / `Result<T>` with an `Error` (code, message, `ErrorType`). `ToHttpResult()` maps success to 200/201 (`Created` carries a `Location`)/202 (`Accepted`)/204, and failure to RFC-7807 `ProblemDetails` via `Error.ToProblemDetails()` (`ErrorType` → status code). Define feature-specific errors as static factory methods in a `*Errors` class (see `Domain/Entities/Faqs/FaqErrors.cs`).

### Domain entities (`Domain/`, `SharedKernel/`)
Aggregates derive from `AuditableEntity<T>` (→ `AggregateRoot<T>` → `BaseEntity<T>`). They use private setters and static factory / mutator methods (e.g. `Faq.Create`, `Faq.Update`) with guard clauses; EF materializes via a private parameterless ctor. IDs are `Guid.CreateVersion7()`.
- **Auditing & soft-delete are automatic** in `ApplicationDbContext.SaveChangesAsync`: `IAuditableEntity` gets created/modified stamps (current user or `"system"`), and deletes of `ISoftDeletable` entities are converted to `IsDeleted = true` updates. A global query filter excludes soft-deleted rows. Do not set audit fields or hard-delete soft-deletable entities manually.

### Persistence (`Infrastructure/Persistence`)
`ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>` and is the single read/write surface. Entity mappings live in `Configurations/` and are auto-applied via `ApplyConfigurationsFromAssembly`. Handlers depend on the `IApplicationDbContext` interface, not the concrete context.

### Transactional outbox (`Infrastructure/Messaging/Outbox`)
For async side-effects (e.g. emails), handlers publish a domain event via `IOutboxPublisher.Add(type, payload)`, which adds an `OutboxMessage` to the context but does NOT save — it rides along with the handler's own `SaveChangesAsync` (same transaction). The `OutboxProcessor` background service polls every 15s and dispatches unprocessed messages through `OutboxDispatcher` to the matching `IDomainEventHandler<T>` (event handlers live in `Features/Notifications/EventHandlers`). Failures are recorded on the message and retried.

### Cross-cutting infrastructure (`Infrastructure/`, registered in `Program.cs`)
Serilog logging (with sensitive-data masking), global exception handling → ProblemDetails, FluentValidation filter, security headers, IP rate limiting, health checks (`/health`), API versioning + OpenAPI, JWT + cookie auth (`AuthCookieService`, `TokenService`), and FluentEmail/Razor email templating.

### Auth
Authorization uses named policies in `Domain/Authorization/AuthorizationPolicies.cs` (`AdminOnly`, `Authenticated`) — reference these constants in `.RequireAuthorization(...)` rather than inlining role checks. Roles are defined in `Roles.cs` and seeded at startup.

## Conventions for adding a feature

1. Create `Features/<Area>/<Action>/` with `<Action>Command.cs` (record + handler), `<Action>Endpoint.cs` (`IEndpoint`), and `<Action>CommandValidator.cs` if it has input.
2. Register the handler in `Program.cs` (`AddScoped`).
3. Return `Result`/`Result<T>`; add feature errors to a `*Errors` class.
4. For async side-effects, publish via `IOutboxPublisher` and add an `IDomainEventHandler<T>` under `Features/Notifications/EventHandlers`.
5. For new entities: derive from `AuditableEntity<T>`, add a `DbSet` to `ApplicationDbContext` + an `IEntityTypeConfiguration` in `Configurations/`, then add a migration.
