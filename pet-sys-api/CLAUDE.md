# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project rule: no AI authorship attribution

Never add yourself as a co-author, contributor, or otherwise indicate that Claude/AI generated the work. This applies to:
- Git commits: do NOT add `Co-Authored-By: Claude ...` trailers.
- Pull requests: do NOT mention Claude/AI generation in the PR description.
- Code comments: do NOT reference Claude or AI assistance.

## Project overview

"4 Patas" is a health/appointment management system for a 24-hour veterinary clinic. It replaces a paper-based record system and lets clients book appointments and view their pets' medical history. The clinic also handles hospitalizations (internación) and surgeries.

Three roles drive the functional scope (from the requirements gathering doc):

- **Client (Usuario)**: register, log in, edit own profile, book consultation appointments, view pet info and medical history, notify end of hospitalization.
- **Administrator**: create/edit/delete veterinarian profiles, manage veterinarians' schedules, suspend client accounts.
- **Veterinarian**: create a pet and assign it to a client, edit/delete pets, view client and pet info, schedule surgery/hospitalization appointments, manage a pet's medical history.

`Domain` (entities + validation + repository interfaces) and the EF Core/PostgreSQL setup in `Infrastructure`/`Web` exist. `Application` is still empty, and `Web` has no controllers/auth yet — expect to add those when implementing the features above. See Architecture below for what's actually wired up vs. still scaffolded.

## Commands

This is a .NET 8 solution (`pet-sys-api.sln`) built with the standard `dotnet` CLI.

```
dotnet build                      # build the whole solution
dotnet build Domain                # build a single project (Domain, Application, Infrastructure, or Web)
dotnet run --project Web           # run the API (Swagger UI opens automatically in Development)
dotnet watch --project Web run     # run with hot reload
dotnet ef migrations add <Name> --project Infrastructure --startup-project Web   # add an EF Core migration
dotnet ef database update --project Infrastructure --startup-project Web        # apply migrations to the DB
dotnet test                                                                      # run every test project
dotnet test --filter FullyQualifiedName~TestName                                 # run a single test
```

First-time setup needs the Postgres connection string in user secrets (never in `appsettings.json` — see Cross-cutting decisions):
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<value>" --project Web
```
Get `<value>` from a teammate out-of-band (password manager / DM), not from git.

The Web project launches on `http://localhost:5079` (see `Web/Properties/launchSettings.json`), with Swagger UI at `/swagger` in the Development environment. `Web/Web.http` is a scratch file for manual REST Client requests against the running API.

## Architecture

Clean Architecture with four projects and a strict, one-directional dependency chain enforced by project references:

```
Domain  <-  Application  <-  Infrastructure  <-  Web
```

- **Domain**: POCO entities and enums only (`Domain/Entities/`). No dependencies on other projects. `Domain/Interfaces/` defines repository contracts: a generic `IBaseRepository<T>` (`GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsAsync`, all keyed by `int id`) plus entity-specific interfaces `IUserRepository`, `IPetRepository`, `IConsultationRepository` that extend it with query methods (e.g. `GetByEmailAsync`, `GetByClientIdAsync`, `GetByStatusAsync`).
- **Application**: intended for use cases/business logic. References `Domain` (fixed — this `ProjectReference` was missing until now even though it was already documented as existing). Scaffolded folders: `Interfaces/`, `Models/`, `Services/` (all currently empty). Has no package dependencies at all (its stray `EntityFramework` 6.5.2 + `Microsoft.EntityFrameworkCore.Tools` references were removed — this layer should stay persistence-ignorant).
- **Infrastructure**: persistence and other external concerns. References `Application` (not `Domain` directly anymore — `Domain` types now reach `Infrastructure` transitively through `Application`, matching the diagram above; previously `Infrastructure` referenced `Domain` directly and skipped `Application` entirely).
  - `Infrastructure/Context/ApplicationDbContext.cs` — a real EF Core `DbContext` on **Npgsql/PostgreSQL** (provider decision made; currently pointed at a Supabase-hosted instance). Maps the `User` hierarchy (`Client`/`Admin`/`Veterinarian`) via `HasDiscriminator<string>("UserType")`, i.e. **TPH (Table-Per-Hierarchy)** — note this contradicts the TPT decision this file documented earlier; if TPT is still the intended design, `OnModelCreating` needs `UseTptMappingStrategy()` instead of the discriminator, otherwise treat TPH as the now-current decision. `Infrastructure/Migrations/` has one applied migration (`InitialCreate`). `Infrastructure/Context/ApplicationDbContextFactory.cs` is the design-time factory `dotnet ef` uses; it reads the connection string from `Web/appsettings.json`, user secrets, then env vars (see Commands).
  - `Infrastructure/Repositories/BaseRepository.cs` is now wired directly to `ApplicationDbContext` (`protected readonly` field, constructor-injected — was briefly `public`, tightened so only the entity-specific repository subclasses can reach it), reading/writing through `context.Set<T>()` — the old in-memory `List<T>` implementation is gone. `UserRepository`, `PetRepository`, `ConsultationRepository` (also in `Infrastructure/Repositories/`) each extend `BaseRepository<T>` and implement their entity-specific interface's extra query methods (`GetByEmailAsync`/`GetByRoleAsync<T>`, `GetByClientIdAsync`, `GetByStatusAsync`/`GetByVeterinarianIdAsync`).
  - Every `BaseRepository<T>` write method (`AddAsync`, `UpdateAsync`, `DeleteAsync`) calls `SaveChangesAsync()` itself — there's no Unit of Work, so a service that needs to touch two repositories as one atomic operation can't yet do that through the repositories alone. Worth introducing (e.g. stop auto-saving in the repositories and add an `IUnitOfWork.SaveChangesAsync()` the caller invokes once) before `Application` services start composing multi-entity operations — not done yet since it would change the repository contract and there are no callers to validate it against.
  - **Previously fixed**: the repository files under `Infrastructure/Repositories/` had `using System.Data.Entity;` (EF6) instead of `using Microsoft.EntityFrameworkCore;`; it compiled fine because both namespaces expose same-shaped `FirstOrDefaultAsync`/`ToListAsync`/`AnyAsync` extension methods on `IQueryable<T>`, but at runtime EF6's async LINQ operators require `IDbAsyncQueryProvider`, which EF Core doesn't implement — every repository method that used them threw `InvalidOperationException` against a real EF Core provider (in-memory or Postgres). The root cause was `Web.csproj`/`Application.csproj` referencing the old EF6 `EntityFramework` package (6.5.2) alongside EF Core, which let the wrong extension methods resolve silently at compile time; that package reference has since been removed from both, so this class of bug shouldn't resurface.
- **Web**: ASP.NET Core Web API host (`Web/Program.cs`), references `Infrastructure` (which now pulls in both `Application` and `Domain` transitively — no direct `ProjectReference` to either is needed from `Web`). Registers `ApplicationDbContext` via `AddDbContext`/`UseNpgsql`. Uses controllers (`AddControllers`/`MapControllers`) plus Swashbuckle for Swagger. `Controllers/` folder is scaffolded but empty.

### Domain model

- `User` (`Domain/Entities/User.cs`) is an **abstract base class** with `Id`, `FullName`, `Password`, `Email`, `Phone`, and a soft-delete flag `IsDeleted`. Validation is expressed via `System.ComponentModel.DataAnnotations` attributes (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Phone]`) rather than in constructors/methods.
- Three concrete roles inherit from `User`, matching the three roles above: `Client` (adds `Dni` and a `Pets` collection), `Admin` (no extra fields), `Veterinarian` (adds a `Consultations` collection). This maps directly to the client/administrator/veterinarian roles in the requirements — role-specific behavior belongs on these subclasses or in services that branch on role, not on `User` itself.
- `Pet` belongs to a `Client` (`ClientId` FK) and has many `Consultation`s. All entity IDs and FK properties (`Pet.Id`/`ClientId`, `Consultation.Id`/`PetId`/`VeterinarianId`) are typed as `int`, matching `User.Id` — keep this convention when adding related entities.
- `Domain/Interfaces/IEntity` (`int Id { get; set; }`) is the constraint `BaseRepository<T> where T : class, IEntity` relies on instead of reflection; all `Domain/Entities` classes implement it. `Domain/Interfaces/IBaseRepository<T>` (and its `IUserRepository`/`IPetRepository`/`IConsultationRepository` specializations) and the EF-Core-backed `Infrastructure/Repositories/BaseRepository<T>` key all lookups by `int id`, consistent with the `int` entity ID convention.
  - `BaseRepository<T>.GetByIdAsync` returns `T?` — `null` when the id doesn't exist, it does **not** throw. `UpdateAsync(int id, T entity)` looks up the tracked entity by `id`, copies `entity`'s values onto it via `CurrentValues.SetValues(entity)`, and returns the updated tracked entity, or `null` if `id` wasn't found — callers must pass an `entity` whose own `Id` matches `id`, since `SetValues` also copies the key property and EF Core throws `InvalidOperationException` ("part of a key and so cannot be modified") if that differs from the tracked entity's actual key. `DeleteAsync`/`ExistsAsync` do **not** throw on a missing id (delete is a no-op, exists returns `false`).
- `Consultation` links a `Pet` and a `Veterinarian`, with a `Status` (`StatusConsultation` enum: `Pending`, `Completed`, `Cancelled`) and a `Date`. This is the appointment/consultation entity referenced by the "agendar turnos" and "historial médico" features.
  - **Open question**: how hospitalizations/surgeries should be modeled is not yet decided. Options considered: extend `Consultation` with a `Type` field (Consulta/Cirugía/Internación), or add separate `Surgery`/`Hospitalization` entities linked to `Pet`. Resolve this before implementing the veterinarian's "agendar turnos de cirugías e internaciones" feature.
- "Notificar fin de internación" (listed under the client's functionalities in the requirements doc) is actually the **system notifying the client** that their pet's hospitalization has ended — not a client-initiated action. Model it as a notification triggered by a status change (e.g., a vet/admin marking hospitalization complete), not as an endpoint the client calls.

## Cross-cutting decisions

- **Auth**: JWT-based authentication, with ASP.NET Core Identity used specifically for password hashing (not the full Identity user-management stack).
- **DTO mapping**: manual mapping (constructors/explicit mapper methods) between `Domain` entities and Application/Web DTOs — no AutoMapper or similar.
- **Persistence**: PostgreSQL via EF Core/Npgsql (decided — see Architecture/Infrastructure above). Connection string is **never committed**: it lives in `dotnet user-secrets` locally (`Web`/`Infrastructure` share one `UserSecretsId` so the `dotnet ef` design-time factory can read it too) and must be injected as a `ConnectionStrings__DefaultConnection` env var / GitHub Actions secret in CI. `Web/appsettings.json` only holds an empty placeholder for that key — do not put a real value back into it.
- **Testing**: xUnit, one test project per layer (mirrors the Clean Architecture split), added as each layer gets testable behavior rather than scaffolded up front. `Domain.Tests` covers entity `DataAnnotations` validation. `Infrastructure.Tests` covers `BaseRepository<T>` behavior plus `UserRepositoryTests`/`PetRepositoryTests`/`ConsultationRepositoryTests` for the entity-specific query methods; it references `Microsoft.EntityFrameworkCore.InMemory` and spins up a real `ApplicationDbContext` per test via `TestHelpers/InMemoryDbContextFactory.Create()` (unique in-memory database name per call) rather than mocking — `TestHelpers/EntityFactory.cs` has the shared valid-entity builders (`Client`/`Veterinarian`/`Admin`/`Pet`/`Consultation`) tests seed through. No `Application.Tests`/`Web.Tests` yet since those layers have no logic to test.
- **Frontend**: a separate SPA (React/Angular/etc., own repo) will consume this API — keep CORS configuration in mind in `Web/Program.cs` once endpoints are built out.
