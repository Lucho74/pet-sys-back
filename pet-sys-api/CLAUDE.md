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

The codebase currently only implements the `Domain` layer (entities + validation). Application, Infrastructure, and Web (controllers/EF configuration/auth) are scaffolded but not yet built out — expect to add to those layers when implementing the features above.

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

- **Domain**: POCO entities and enums only (`Domain/Entities/`). No dependencies on other projects. Has an `Interfaces/` folder scaffolded for future repository/domain-service contracts.
- **Application**: intended for use cases/business logic. References `Domain`. Scaffolded folders: `Interfaces/`, `Models/`, `Services/` (all currently empty).
- **Infrastructure**: intended for persistence (EF Core DbContext, repository implementations) and other external concerns. References `Application`. Scaffolded folder: `Repositories/` (currently empty). No ORM/DB provider package is referenced yet.
- **Web**: ASP.NET Core Web API host (`Web/Program.cs`), references both `Application` and `Infrastructure`. Uses controllers (`AddControllers`/`MapControllers`) plus Swashbuckle for Swagger. `Controllers/` folder is scaffolded but empty.

### Domain model

- `User` (`Domain/Entities/User.cs`) is an **abstract base class** with `Id`, `FullName`, `Password`, `Email`, `Phone`, and a soft-delete flag `IsDeleted`. Validation is expressed via `System.ComponentModel.DataAnnotations` attributes (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Phone]`) rather than in constructors/methods.
- Three concrete roles inherit from `User`, matching the three roles above: `Client` (adds `Dni` and a `Pets` collection), `Admin` (no extra fields), `Veterinarian` (adds a `Consultations` collection). This maps directly to the client/administrator/veterinarian roles in the requirements — role-specific behavior belongs on these subclasses or in services that branch on role, not on `User` itself.
- `Pet` belongs to a `Client` (`ClientId` FK) and has many `Consultation`s. `Id`, `PetId`, `VeterinarianId`, and `Consultation.Id` are typed as `string` (not `int`) even though marked `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` — match this convention when adding related entities.
- `Consultation` links a `Pet` and a `Veterinarian`, with a `Status` (`StatusConsultation` enum: `Pending`, `Completed`, `Cancelled`) and a `Date`. This is the appointment/consultation entity referenced by the "agendar turnos" and "historial médico" features.
  - **Open question**: how hospitalizations/surgeries should be modeled is not yet decided. Options considered: extend `Consultation` with a `Type` field (Consulta/Cirugía/Internación), or add separate `Surgery`/`Hospitalization` entities linked to `Pet`. Resolve this before implementing the veterinarian's "agendar turnos de cirugías e internaciones" feature.
- "Notificar fin de internación" (listed under the client's functionalities in the requirements doc) is actually the **system notifying the client** that their pet's hospitalization has ended — not a client-initiated action. Model it as a notification triggered by a status change (e.g., a vet/admin marking hospitalization complete), not as an endpoint the client calls.
- No `DbContext` exists yet. When adding one (in `Infrastructure`):
  - Database provider is **not decided yet** — don't assume SQL Server/PostgreSQL/SQLite without confirming.
  - Map the `User` hierarchy (`Client`/`Admin`/`Veterinarian`) using **TPT (Table-Per-Type)** — one table per class, not the EF Core TPH default.

## Cross-cutting decisions

- **Auth**: JWT-based authentication, with ASP.NET Core Identity used specifically for password hashing (not the full Identity user-management stack).
- **DTO mapping**: manual mapping (constructors/explicit mapper methods) between `Domain` entities and Application/Web DTOs — no AutoMapper or similar.
- **Testing**: no test project exists yet; framework not decided.
- **Frontend**: a separate SPA (React/Angular/etc., own repo) will consume this API — keep CORS configuration in mind in `Web/Program.cs` once endpoints are built out.
