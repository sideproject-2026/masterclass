# Backend — `src/`

Adds to the root [CLAUDE.md](../CLAUDE.md). Full reasoning: [09-code-conventions.md](../artifacts/design/09-code-conventions.md).

.NET 10 · minimal APIs · EF Core 10 · PostgreSQL · modular monolith.

---

## Where things go

```
Lms.AppHost/            Aspire orchestration. Dev only — nothing here ships.
Lms.ServiceDefaults/    AddServiceDefaults(): OTel, health, resilience.
Lms.Api/                Composition root. AddXModule + MapXEndpoints. No logic.
Lms.SharedKernel/       Result, Error, ids, IClock, PageRequest/QueryResult, messaging contracts.
Lms.MigrationService/   Runs migrations as a job.
Modules/Lms.Modules.X/            Domain/ · Features/ · Infrastructure/ · Endpoints/
Modules/Lms.Modules.X.Contracts/  DTOs, query interfaces, events. NO project references.
```

New use case → `/new-slice <Module> <UseCaseName> command|query`. Don't hand-roll the folder.

---

## Module boundaries — the ones that break silently

- Reference another Module only through its `*.Contracts`. Never `Domain`, `Features`, or `Infrastructure`.
- `*.Contracts` projects may reference **only `Lms.SharedKernel`** (a leaf) — never their owning Module, never another Module. That's what keeps mutual Catalog↔Enrollment references acyclic.
- No cross-Module foreign keys. `Enrollment.CourseId` is an indexed `Guid`.
- One `DbContext`, one schema per Module (`identity`, `catalog`, `enrollment`, `notifications`).
- Cross-Module reaction → `IEventBus`. Cross-Module read → a Contracts interface (`ICourseCurriculumQuery`, `IEnrollmentLookup`).
- `Lms.ArchitectureTests` enforces this. If it goes red, fix the code, not the test.

## Auth

- `AuthPolicies`, `Roles` and `RateLimitPolicies` live in **`Lms.SharedKernel.Authorization`**. Never redeclare a role or policy name in a module — and never reference `Lms.Api` from a module to reach one.
- The role claim is the short `"role"`, not `ClaimTypes.Role`. Issuer and validator must agree or every policy silently denies.
- Access tokens last **15 minutes** and cannot be revoked early; that window *is* the revocation window. Logout revokes the refresh token.
- Login returns one indistinguishable `401` for unknown email, wrong password and lockout. Never add a more helpful message.
- `caller.GetUserId()` from `Lms.SharedKernel.Http` — take the user id from the token, never from the request body.

## Handlers

```csharp
internal sealed class XHandler(XDbContext db, IClock clock) : ICommandHandler<XCommand, TResult>
{
    public async Task<Result<TResult>> HandleAsync(XCommand cmd, CancellationToken ct) { … }
}
```

- `internal sealed`. Primary constructors. `ct` last, always named `ct`.
- **Commands vs queries is not decoration** — `TransactionBehavior` decorates `ICommandHandler<,>` only. A write registered as a query silently loses its transaction.
- Behaviours are open-generic DI decorators. Never add a dispatcher, `ISender`, or reflection-based routing.
- Endpoints inject the handler interface directly.

## Result & errors

- Expected failure → `return SomeErrors.Thing;`. Exceptions are for programmer error and infrastructure only.
- One error catalogue per Module (`CatalogErrors`, `EnrollmentErrors`). Never an inline error string at a call site.
- `ErrorType` → HTTP status maps in `ToHttpResult()` and nowhere else.
- Combinators are `Map`, `Bind`, `Tap`, `Ensure`. Don't grow the set.

## Domain

- Invariants live in the entity. `course.Publish()` returns `Result` — it does not throw and it is not a validator's job.
- Entities are created via factory methods. **A mapper never populates an entity.**
- No public setters that can produce an invalid state. `Lesson.SetVideoContent(...)` / `SetReadingContent(...)`, and switching type clears the other side.
- Value objects: private ctor + `static Result<T> Create(...)`. Implicit conversion out to the primitive, never in.
- Typed ids everywhere: `CourseId`, `LessonId`, `UserId`, `EnrollmentId`. `Guid.CreateVersion7()`.

## EF Core

- `AsNoTracking()` on reads — it's the context default; opt *in* to tracking in command handlers.
- `AsSplitQuery()` for Course→Chapters→Lessons. Two collection levels in one query is a cartesian blowup.
- **Project to DTOs inside the query.** Never materialise an entity for a list view.
- Concurrency is Postgres `xmin`: a `uint` property with `IsRowVersion()`. No extra column, no migration.
- `ExecuteUpdateAsync` for set-based writes (reorder, outbox stamping). Don't load rows to change one column.
- Configuration in `IEntityTypeConfiguration<T>`. No data annotations on domain entities.
- **Never migrate at startup.** `Lms.MigrationService` runs as a job; the AppHost gates the API on it with `WaitForCompletion`.
- Identifiers are **snake_case** (`EFCore.NamingConventions`). Write raw SQL — index filters, check constraints — in snake_case to match.
- New module DbContext: expose an `AddXPersistence(...)` that registers only the context (the migration job calls that, not the full module), then call `.UseLmsConventions()` and `npgsql.UseLmsMigrationHistory(Schema, MigrationsHistoryTable)`, apply `ApplyStronglyTypedIdConventions()` in `ConfigureConventions`, and add one `AddScoped<DbContext>` line in `Lms.MigrationService/Program.cs`.
- **Never hand-edit a file under `Migrations/`.** They are generated and exempt from the house style in `.editorconfig`.
- `Tags` is `text[]` with a GIN index; search is a generated `tsvector` column.

## Pagination

- Handler returns `Result<QueryResult<T>>` (`Data` + `TotalCount`). Endpoint converts to `PagedResult<T>`.
- `Skip(`/`Take(` exist **only** in `SharedKernel.ToQueryResultAsync`.
- Order before paging, always with a unique tiebreaker (`.ThenBy(x => x.Id)`). Non-unique ordering makes pages non-deterministic in Postgres.

## Endpoints

- Route groups per audience; `.RequireAuthorization(AuthPolicies.X)` at the **group**.
- Role checks are not enough. Every `/api/studio/*` write asserts `course.InstructorId == caller.Id`. Every `/api/learn/*` read asserts enrolment unless `IsPreview`.
- Shapes are specified in [03-api-design.md](../artifacts/design/03-api-design.md). Don't invent one — if the spec is wrong, change the spec.
- All failures are `ProblemDetails`.

## Don't

Generic `IRepository<T>` · `XService` classes · `Helpers/` `Utils/` `Common/` folders · base handler classes · AutoMapper · MediatR · interfaces with one implementation and no named second case.
