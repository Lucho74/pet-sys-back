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

`Domain` (entities + validation + repository interfaces) and EF Core/PostgreSQL persistence in `Infrastructure` are wired up. `Application`/`Web` have CRUD for `User` (as `Client`), `Pet`, and `Consultation`, but no auth yet; expect to add JWT auth when implementing the remaining features above.

## Commands

This is a .NET 8 solution (`pet-sys-api.sln`) built with the standard `dotnet` CLI.

```
dotnet build                      # build the whole solution
dotnet build Domain                # build a single project (Domain, Application, Infrastructure, or Web)
dotnet run --project Web           # run the API (Swagger UI opens automatically in Development)
dotnet watch --project Web run     # run with hot reload
```

There is no test project yet. When one is added, run a single test with `dotnet test --filter FullyQualifiedName~TestName`.

The Web project launches on `http://localhost:5079` (see `Web/Properties/launchSettings.json`), with Swagger UI at `/swagger` in the Development environment. `Web/Web.http` is a scratch file for manual REST Client requests against the running API.

## Architecture

Clean Architecture with four projects and a strict, one-directional dependency chain enforced by project references:

```
Domain  <-  Application  <-  Infrastructure  <-  Web
```

- **Domain**: POCO entities and enums only (`Domain/Entities/`). No dependencies on other projects. `Domain/Interfaces/` defines repository contracts: a generic `IBaseRepository<T>` (`GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsAsync`, all keyed by `int id`) plus entity-specific interfaces `IUserRepository`, `IPetRepository`, `IConsultationRepository` that extend it with query methods (e.g. `GetByClientIdAsync`, `GetByStatusAsync`, `GetByVeterinarianIdAsync`).
- **Application**: use cases/business logic. References `Domain`. `Models/` has `CreateUserDTO`/`UserDTO`, `CreatePetDTO`/`PetDTO`, and `CreateConsultationDTO`/`ConsultationDTO`; `Services/UserServices.cs`, `Services/PetServices.cs`, and `Services/ConsultationServices.cs` implement CRUD for `User` (on `IUserRepository`), `Pet` (on `IPetRepository`), and `Consultation` (on `IConsultationRepository`) respectively, mapping manually to/from the DTOs, and implement real interfaces (`IUserServices`, `IPetServices`, `IConsultationServices`) that DI resolves against (`AddScoped<IUserServices, UserServices>()`, same for `Pet`/`Consultation`). `ConsultationServices` is the first service to use a private `MapToDTO` explicit-mapper method instead of repeating the object initializer at every return site — `UserServices`/`PetServices` still inline it; either form is the documented convention (see Cross-cutting decisions). `Application/Exceptions/` has `NotFoundException` and `BadRequestException`: services throw these instead of returning `null`/`false` for "not found" or "invalid input" — see Cross-cutting decisions below for the full convention.
- **Infrastructure**: persistence and other external concerns. References `Domain` directly, **not** `Application` — the real reference chain today is `Domain <- Infrastructure <- Web`, skipping `Application` at the project-reference level (Web still references `Application` directly for DI). `Infrastructure/Context/ApplicationDbContext.cs` is a real EF Core `DbContext` on **Npgsql/PostgreSQL**, currently pointed at a Supabase-hosted instance. `Infrastructure/Repositories/BaseRepository<T>` is **EF-Core backed** (queries `_context.Set<T>()` directly) — it is not an in-memory `List<T>` implementation. `UserRepository`, `PetRepository`, `ConsultationRepository` extend `BaseRepository<T>` and implement their respective entity-specific interfaces (`ConsultationRepository` adds `GetByPetIdAsync`, `GetByVeterinarianIdAsync`, `GetByStatusAsync`). `ApplicationDbContextFactory` is the design-time factory `dotnet ef` uses; it reads the connection string from `Web/appsettings.json`.
- **Web**: ASP.NET Core Web API host (`Web/Program.cs`), references both `Application` and `Infrastructure`. Registers `ApplicationDbContext` via `AddDbContext`/`UseNpgsql`, DI for `IUserRepository`/`IUserServices`, `IPetRepository`/`IPetServices`, and `IConsultationRepository`/`IConsultationServices`, and a global exception handler (`AddExceptionHandler<AppExceptionHandler>()` + `AddProblemDetails()` + `app.UseExceptionHandler()`) — `Web/ExceptionHandling/AppExceptionHandler.cs` implements .NET 8's `IExceptionHandler`, mapping `NotFoundException`→`404` and `BadRequestException`→`400`; anything else falls through (`TryHandleAsync` returns `false`) to the default 500 behavior. `Controllers/UserController.cs`, `Controllers/PetController.cs`, and `Controllers/ConsultationController.cs` each expose full REST CRUD (`GET`/`GET {id}`/`POST`/`PUT {id}`/`DELETE {id}`) at `api/user`, `api/pet`, and `api/consultation` respectively, and neither does any null-checking itself — they just call the service and let exceptions propagate to the handler. `ConsultationController` additionally exposes `GET api/consultation/pet/{petId}`, `GET api/consultation/veterinarian/{veterinarianId}`, and `GET api/consultation/status/{status}` as unauthenticated filter endpoints (see Auth note below). Action method names match the verb used at the repository/service layer for that HTTP verb (`Add{Entity}` for `POST`, not `Create{Entity}`) — keep this alignment (`AddAsync`/`Add{Entity}Async`/`Add{Entity}`, `UpdateAsync`/.../`Update{Entity}`, etc.) across all three layers when adding new entities.

### Domain model

- `User` (`Domain/Entities/User.cs`) is an **abstract base class** with `Id`, `FullName`, `Password`, `Email`, `Phone`, and a soft-delete flag `IsDeleted`. Validation is expressed via `System.ComponentModel.DataAnnotations` attributes (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Phone]`) rather than in constructors/methods.
- Three concrete roles inherit from `User`, matching the three roles above: `Client` (adds `Dni` and a `Pets` collection), `Admin` (no extra fields), `Veterinarian` (adds a `Consultations` collection). This maps directly to the client/administrator/veterinarian roles in the requirements — role-specific behavior belongs on these subclasses or in services that branch on role, not on `User` itself.
- `Pet` belongs to a `Client` (`ClientId` FK) and has many `Consultation`s. All entity IDs and FK properties (`Pet.Id`/`ClientId`, `Consultation.Id`/`PetId`/`VeterinarianId`) are typed as `int`, matching `User.Id` — keep this convention when adding related entities.
- `Domain/Interfaces/IBaseRepository<T>` and its EF-Core-backed `Infrastructure/Repositories/BaseRepository<T>` key all lookups by `int id`, consistent with the `int` entity ID convention.
  - `BaseRepository<T>.GetByIdAsync` returns `null` (via `FirstOrDefaultAsync`) when the id doesn't exist — it does **not** throw; repository-level "not found" is still a plain `null`, only the `Application` layer above it turns that into `NotFoundException` (see Cross-cutting decisions). `DeleteAsync` is a no-op on a missing id; `ExistsAsync` returns `false`. `UpdateAsync` looks the entity up first and returns `null` if missing, otherwise applies `CurrentValues.SetValues` and saves.
  - `T` must implement `Domain.Interfaces.IEntity` (constrained via `where T : class, IEntity`) rather than relying on reflection over an `Id` property.
- `Consultation` links a `Pet` and a `Veterinarian`, with a `Status` (`StatusConsultation` enum: `Pending`, `Completed`, `Cancelled`) and a `Date`. This is the appointment/consultation entity referenced by the "agendar turnos" and "historial médico" features. `ConsultationServices.AddConsultationAsync` always sets `Status = StatusConsultation.Pending` on create (a new consultation starts pending, like booking an appointment) — `CreateConsultationDTO` deliberately has no `Status` field for this reason; `ConsultationDTO` (used for `GET`/`PUT`) does include `Status` so a vet/admin can move it to `Completed`/`Cancelled` via `PUT`. Both `AddConsultationAsync` and `UpdateConsultationAsync` validate `PetId` (must resolve via `IPetRepository.GetByIdAsync`) and `VeterinarianId` (must resolve via `IUserRepository.GetByIdAsync` to a `Veterinarian`, not just any `User`) before saving, throwing `BadRequestException` otherwise — same cross-entity-validation convention as `PetServices.AddPetAsync`'s `ClientId` check.
  - **Open question**: how hospitalizations/surgeries should be modeled is not yet decided. Options considered: extend `Consultation` with a `Type` field (Consulta/Cirugía/Internación), or add separate `Surgery`/`Hospitalization` entities linked to `Pet`. Resolve this before implementing the veterinarian's "agendar turnos de cirugías e internaciones" feature.
- "Notificar fin de internación" (listed under the client's functionalities in the requirements doc) is actually the **system notifying the client** that their pet's hospitalization has ended — not a client-initiated action. Model it as a notification triggered by a status change (e.g., a vet/admin marking hospitalization complete), not as an endpoint the client calls.
- `ApplicationDbContext` maps the `User` hierarchy (`Client`/`Admin`/`Veterinarian`) via `HasDiscriminator<string>("UserType")`, i.e. **TPH (Table-Per-Hierarchy)** — note this contradicts the TPT decision this file documented earlier; if TPT is still the intended design, `OnModelCreating` needs `UseTptMappingStrategy()` instead of the discriminator, otherwise treat TPH as the now-current decision. `Pet`/`Consultation` FKs cascade-delete from their parent (`Client`→`Pet`, `Pet`→`Consultation`), while `Consultation`→`Veterinarian` is `Restrict`.

## Cross-cutting decisions

- **Auth**: JWT-based authentication, with ASP.NET Core Identity used specifically for password hashing (not the full Identity user-management stack). Not implemented yet — `UserServices.AddUserAsync` currently stores `CreateUserDTO.Password` as-is, unhashed. All `Consultation` endpoints are open for now; once JWT auth lands, revisit `ConsultationController`'s `pet/{petId}`, `veterinarian/{veterinarianId}`, and `status/{status}` filter endpoints (and possibly the `PetId`/`VeterinarianId` FK checks in `ConsultationServices`) to restrict them to `Admin`/`Veterinarian` roles — flagged by the user as a deliberate follow-up, not yet implemented.
- **DTO mapping**: manual mapping (constructors/explicit mapper methods) between `Domain` entities and Application/Web DTOs — no AutoMapper or similar. See `UserServices` for the established pattern.
- **Input validation**: DataAnnotations on the DTOs (mirroring the constraints on the corresponding `Domain` entity) plus `[ApiController]`'s automatic `ModelState` validation — invalid input short-circuits with `400` before the controller action runs.
- **Error handling / "not found"**: services throw `Application.Exceptions.NotFoundException` or `BadRequestException` instead of returning `null`/`false` or using a tuple return — this **replaces** the earlier null-return/tuple convention (an older iteration of this codebase used bare `null`/`false` plus manual `if (x == null) return NotFound();` checks in every controller action, and `PetServices.UpdatePetAsync` briefly returned a `(PetDTO?, bool)` tuple to distinguish "not found" from "invalid ClientId" before exceptions existed; both are gone now). `Web/ExceptionHandling/AppExceptionHandler.cs` (a .NET 8 `IExceptionHandler`) is the single place that maps exception type → HTTP status code; controllers should never `try/catch` or null-check service results themselves — just call the service and return `Ok`/`CreatedAtAction`/`NoContent` directly, same as `UserController`/`PetController`. Cross-entity validation (e.g. `Pet.ClientId` must reference an existing `Client`) now throws `BadRequestException` from `PetServices.AddPetAsync`/`UpdatePetAsync` (both inject `IUserRepository` and check `is not Client`) instead of returning `null`/a tuple. Any exception type not handled by `AppExceptionHandler` falls through to the default 500 behavior, so add a case there (not a controller-level catch) when a new failure mode needs its own status code.
- **Persistence**: PostgreSQL via EF Core/Npgsql (decided — see Architecture/Infrastructure above).
  - **Known issue**: `Web/appsettings.json` currently has the real Supabase connection string (including password) committed in plaintext. It should be moved to `dotnet user-secrets` / an env var and the leaked credential rotated — don't copy this pattern for new config.
- **Testing**: no test project exists yet on `main`; framework not decided.
- **Frontend**: a separate SPA (React/Angular/etc., own repo) will consume this API — keep CORS configuration in mind in `Web/Program.cs` once endpoints are built out.
