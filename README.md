# Titan Fitness — Staff Portal API

A DDD/CQRS backend for the Titan Fitness staff portal assignment: branches,
studios, members, plans, memberships (with freezes and guest passes),
trainers, class sessions/bookings, and check-ins.

**Read `DESIGN_DECISIONS.md` first.** The assignment says the design
decisions are what's being assessed — that file explains every aggregate
boundary and the reasoning behind it, tied back to specific sentences in the
business logic doc.

## Stack

- **Domain-Driven Design**: aggregates, entities, value objects, domain
  services, repository interfaces — all in `TitanFitness.Domain`, with zero
  dependencies on anything else.
- **CQRS**: MediatR commands/queries in `TitanFitness.Application`.
- **FluentValidation**: one validator per command, run automatically by a
  MediatR pipeline behavior before the handler executes.
- **EF Core 8, Code First, Fluent API**: `TitanFitness.Infrastructure`
  (SQL Server). No data annotations on domain entities — every mapping rule
  (max lengths, keys, relationships, owned value objects) lives in
  `Persistence/Configurations/*Configuration.cs`.
- **Repository pattern + Unit of Work**: one repository interface per
  aggregate root (declared in Domain, implemented in Infrastructure), plus
  `IUnitOfWork.SaveChangesAsync()` committing everything as one transaction.
- **ASP.NET Core 8 Web API** with Swagger, in `TitanFitness.Api`.
- **xUnit tests** in `tests/TitanFitness.Domain.Tests` covering the
  trickiest invariants (AgreedTerms immutability, freeze budget, waitlist
  promotion, cancellation finality).

## Project layout

```
TitanFitness.sln
src/
  TitanFitness.Domain/          <- no dependencies on anything
  TitanFitness.Application/     <- depends on Domain only
  TitanFitness.Infrastructure/  <- depends on Application + Domain, EF Core
  TitanFitness.Api/             <- depends on Infrastructure, ASP.NET Core
tests/
  TitanFitness.Domain.Tests/    <- depends on Domain only
DESIGN_DECISIONS.md
README.md (this file)
```

Dependencies only ever point inward (Api → Infrastructure → Application →
Domain); Domain never references anything above it.

## Running it locally

This solution was built and reviewed without a live .NET SDK / NuGet
connection in the environment that generated it — **you'll need to restore
and build it yourself the first time**, and fix up anything a real compiler
catches that a careful read-through didn't. Steps:

1. **Prerequisites**: [.NET 8 SDK](https://dotnet.microsoft.com/download),
   and either SQL Server / SQL Server Express, or `sqllocaldb` (ships with
   Visual Studio).

2. **Restore and build**:
   ```bash
   cd TitanFitness
   dotnet restore
   dotnet build
   ```

3. **Create the database migration** (there isn't one checked in, since
   generating it requires a live SDK + the `dotnet-ef` tool):
   ```bash
   dotnet tool install --global dotnet-ef   # if you don't already have it
   dotnet ef migrations add InitialCreate \
     --project src/TitanFitness.Infrastructure \
     --startup-project src/TitanFitness.Api
   ```
   A design-time factory (`TitanFitnessDbContextFactory`) is already in
   place so this works without needing the API's DI container spun up.

4. **Run the API** — it applies pending migrations automatically on startup
   in the `Development` environment:
   ```bash
   dotnet run --project src/TitanFitness.Api
   ```
   Swagger UI opens at `https://localhost:5081/swagger` (or check the
   console output for the actual port).

5. **Run the tests**:
   ```bash
   dotnet test
   ```

6. **Connection string**: `src/TitanFitness.Api/appsettings.json` defaults
   to `(localdb)\mssqllocaldb`. Change `ConnectionStrings:TitanFitness` if
   you're pointing at a different SQL Server instance.

## API surface

All endpoints are under `/api`. A few worth knowing about:

| Endpoint | Backs |
|---|---|
| `GET /api/dashboard/stats` | Figure 1 — Dashboard |
| `GET /api/members?search=` | Figure 2 — Member Directory |
| `POST /api/members` | Figure 3 — Add New Member |
| `GET /api/members/{id}/memberships` | Figure 4/5 — current plan card |
| `POST /api/memberships/{id}/renew-or-change-plan` | Figure 5 (Renew) & Figure 6 (Change Plan) |
| `POST /api/memberships/{id}/freeze` | Figure 7 — Freeze Membership |
| `GET /api/class-sessions?date=&branchId=` | Figure 8 — Class Schedule |
| `POST /api/class-sessions` | Figure 9 — Add New Class |
| `POST /api/class-sessions/{id}/bookings` | Figures 10/11 — Book Session |
| `GET /api/trainers`, `POST/PUT` | Figures 12–15 — Trainer Directory/Add/Update |
| `GET /api/plans`, `POST/PUT`, `.../publish`, `.../retire` | Figures 16–19 — Plan Catalogue/Add/Update |

Every write endpoint returns `422 Unprocessable Entity` with a JSON problem
body when a business rule is violated (e.g. freezing past the allowance,
double-booking a session), and `400 Bad Request` for malformed input
(missing fields, bad lengths) — see `Api/Middleware/ExceptionHandlingMiddleware.cs`.

## Submission checklist (from the assignment)

- [ ] Confirm your full name (4 parts) is set correctly on your GitHub commit/profile before pushing.
- [ ] Push to GitHub.
- [ ] Email the repo link to **a.hajali@skyits.com** before the 01/09/2026 deadline.
