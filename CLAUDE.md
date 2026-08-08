# Learning Management System

Engineering-focused LMS (dometrain-style). Instructors author courses; students browse, enrol, and work through them.
**Modular monolith** — .NET 10 API + TanStack Start web, PostgreSQL, Azure.

**Status:** design complete, no code yet. First card is `F-1` in [artifacts/design/08-delivery-plan.md](artifacts/design/08-delivery-plan.md).

---

## Read before writing code

| Doc | For |
|---|---|
| [09-code-conventions.md](artifacts/design/09-code-conventions.md) | **Patterns, naming, the mediator, what we don't do.** Read this first. |
| [01-architecture.md](artifacts/design/01-architecture.md) | Module boundaries and project layout |
| [02-domain-model.md](artifacts/design/02-domain-model.md) | Entities, invariants, EF conventions |
| [03-api-design.md](artifacts/design/03-api-design.md) | Endpoint contracts — the API is specified, don't invent shapes |
| [08-delivery-plan.md](artifacts/design/08-delivery-plan.md) | What we're building this sprint |

Scoped rules load automatically: [src/CLAUDE.md](src/CLAUDE.md) when working on the backend, [web/CLAUDE.md](web/CLAUDE.md) on the frontend.
New use case → `/new-slice <Module> <UseCaseName> command|query`.

Vocabulary: **Course → Chapter → Lesson** (Video or Reading). "Module" always means a code Module (Catalog, Enrollment…), never a course.

---

## Commands

```bash
dotnet run --project src/Lms.AppHost
```
Starts API + PostgreSQL + Azurite + the migration job, with the Aspire dashboard. Requires Docker.

```bash
dotnet build && dotnet test
```

```bash
dotnet ef migrations add <Name> --project src/Modules/Lms.Modules.<Module> --startup-project src/Lms.MigrationService --context <Module>DbContext --output-dir Infrastructure/Migrations
```
`dotnet-ef` is pinned in `.config/dotnet-tools.json` — run `dotnet tool restore` first. Its version must match the EF Core package major; never `--prerelease`.

```bash
./scripts/reset-local-data.ps1
```
Wipes the Docker volumes. The reset is the volume, not the process — restarting the AppHost does **not** clear database state.

Local dev is **Aspire only**. Postgres and Azurite run with `WithDataVolume()` + `ContainerLifetime.Persistent`, so data survives restarts. A broken local DB is fixed with `docker volume rm`, not by restarting.

---

## Rules that are easy to get wrong

**Architecture**
- A Module references only another Module's `*.Contracts` — never its `Domain`, `Features`, or `Infrastructure`.
- No cross-Module foreign keys. `Enrollment.CourseId` is an indexed `Guid`, not an FK.
- One `DbContext` and one schema per Module.
- Cross-Module reactions go through `IEventBus`. Synchronous reads go through a Contracts interface.

**Code**
- Expected failures return `Result<T>`. Exceptions are for programmer errors and infrastructure only.
- **Rule of two** — introduce an abstraction on the second instance, not the first. If you can't name the second case, don't add the interface.
- Entities are constructed via their own factory methods. **A mapper never populates an entity.**
- No `Services/`, `Helpers/`, `Utils/`, or `Common/` folders. Name things after what they do.
- Handlers are `internal sealed`. One use case per folder under `Features/`.
- Pipeline behaviours are DI decorators over handler interfaces — no reflection-based dispatch, no `ISender`.
- Paging: handlers return `QueryResult<T>` (`Data` + `TotalCount`); endpoints convert to `PagedResult<T>`. `Skip(`/`Take(` appear **only** in `SharedKernel.ToQueryResultAsync`. Always order with a unique tiebreaker before paging.

**Data**
- `AsNoTracking()` on every read path; `AsSplitQuery()` for Course→Chapters→Lessons.
- UUIDv7 keys generated in app code (`Guid.CreateVersion7()`). Never `gen_random_uuid()` as a default.
- Optimistic concurrency is Postgres `xmin` — a `uint` property with `IsRowVersion()`.
- Project to DTOs inside the query. Never materialise an entity for a list view.
- **Never migrate on startup.** `Lms.MigrationService` runs as a job.

**Security — these are the ones that matter**
- No access token reachable from browser JavaScript. The TanStack Start server holds an `HttpOnly` session cookie and calls the API server-side.
- Every `/api/studio/*` write checks `course.InstructorId == caller.Id`. Holding the `Instructor` role is not authorisation to edit someone else's course.
- Every `/api/learn/*` read checks enrolment unless `lesson.IsPreview`. **The gate is the API's 403, not a hidden button.**
- `externalVideoId` is only ever returned from `GET /api/learn/lessons/{id}` — never from the public course-detail payload.
- All rendered markdown is sanitised. Instructors are curated, not trusted.

**Frontend**
- Components render, hooks decide.
- Zod at every boundary; infer types from schemas, never hand-write a duplicate.
- Server functions are the only thing that calls the API. No `fetch` in a component.
- Banned: `any`, `as` (except `as const`), non-null `!`, default exports, `useEffect` for fetching.
- A card isn't done until it works at 375px in **both** light and dark themes.

---

## Working preferences

- Design principles matter here. Prefer the clean solution and say why; don't silently take a shortcut.
- **Studio UI is never polished.** shadcn defaults, forever. Only the public surface gets a design pass (Sprint 28). Fiddling with Studio CSS is procrastination.
- Flag it when a change contradicts a design doc — update the doc rather than letting them drift.
- MediatR and AutoMapper are commercially licensed. We use our own mediator (§2 of the conventions) and Mapperly.
