# Sprint Log

Recorded actuals per card. Updated as the **last action of every card**, before the PR is reported.

This exists for one reason: [08 §2.3](../design/08-delivery-plan.md) re-baselines the whole 33-sprint schedule on measured velocity after Sprint 3. The 5 pts/week figure is a hypothesis until this table disagrees with it.

> 1 point ≈ 2 focused hours. Record **actual** points as hours ÷ 2, honestly — an inflated actual hides a velocity problem until it is expensive.

---

## Velocity

| Sprint | Dates | Planned | Completed | Velocity | Notes |
|---|---|---:|---:|---:|---|
| 1 | Aug 10–16 | 5 | 5 | 5.0 | ✅ Both cards done. Started early (Aug 8) in one sitting, so this is **not** a valid velocity sample — see caveat. |
| 2 | Aug 17–23 | 5 | 5 | 5.0 | ✅ `F-3` + `F-4`. Also completed early, same sitting. Estimates held on both. |

> **Caveat on Sprint 1.** These five points were completed in a single continuous session rather
> than across a week of evenings. The estimate held, but it says nothing yet about sustained
> pace under real conditions. Treat Sprints 2–3 as the actual calibration.

**Rolling average:** — (needs 3 sprints)
**Re-baseline checkpoints:** end of Sprint 3, end of Sprint 9

---

## Sprint 1 — Aug 10–16, 2026

**Goal:** *The stack starts with one command.*
**Demo:** `dotnet run` on the AppHost → dashboard green, web page renders API data through a server function, CI passes on a PR.

### `F-1` Solution skeleton + SharedKernel

| | |
|---|---|
| **Estimate** | 3 pts |
| **Actual** | 3 pts |
| **Area** | `api` |
| **Branch** | `feat/f-1-solution-skeleton` |
| **PR** | pending — `gh` not yet installed |
| **Started / Finished** | 2026-08-08 / 2026-08-08 |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] `dotnet build` clean — 0 warnings, 0 errors, with `TreatWarningsAsErrors=true`
- [x] `dotnet test` green — **60 passed**, 0 failed
- [x] Solution contains all 12 projects from [01 §3](../design/01-architecture.md)
- [x] `*.Contracts` projects reference only `Lms.SharedKernel` (see deviation below)
- [x] SharedKernel covers Result, pagination, messaging contracts, events, clock, typed IDs

**Shipped**
- Build config: `global.json` (SDK pinned), `Directory.Build.props` (net10.0, nullable, warnings-as-errors), `Directory.Packages.props` (central package management, 7 packages), `.editorconfig`, `.gitattributes`
- 12 projects: `Lms.Api`, `Lms.SharedKernel`, `Lms.SharedKernel.Persistence`, 5 Modules, 3 Contracts, `Lms.UnitTests`
- `Results/` — `Result`, `Result<T>`, `Error`, `ErrorType`, `Unit`, combinators `Map`/`Bind`/`Tap`/`Ensure` + async variants
- `Pagination/` — `PageRequest` (clamping), `QueryResult<T>` (`Data` + `TotalCount`), `PagedResult<T>`; `ToQueryResultAsync` in `.Persistence`
- `Messaging/` — `ICommand`/`IQuery`/handlers/`IIdempotent` (contracts only; decorators are `F-8`)
- `Events/` — `IDomainEvent`, `IEventBus`, `IEventHandler<T>`, `InProcessEventBus`
- `Time/`, `Authorization/`, `Identifiers/` — 5 UUIDv7 typed ids with a generic JSON converter
- Module stubs with `AddXModule`/`MapXEndpoints`, wired into `Program.cs`

**Decisions**
1. **`Lms.SharedKernel.Persistence` split out.** `ToQueryResultAsync` needs EF Core; putting it in `SharedKernel` would make EF transitively visible from every Module's `Domain`, weakening the arch rule in [01 §4.1](../design/01-architecture.md). One extra project, guardrail intact.
2. **Shouldly, not FluentAssertions.** FluentAssertions v8+ is commercially licensed — the same trap as MediatR and AutoMapper. Caught at package-selection time.
3. **xunit v3 (3.2.2), not v2.** The `dotnet new xunit` template still emits v2; project files were rewritten by hand.
4. **Four CA analyzer rules disabled with written justification** in `.editorconfig` (CA1000, CA1711, CA1716 — library-author rules that do not fit a first-party app). `CA1805` was a fair catch and was fixed properly (`Unit.Value` became an expression-bodied property).
5. **Typed ids live in `SharedKernel`, not per-Module Contracts.** `CourseId` is written by Catalog and referenced by Enrollment — shared vocabulary, not domain logic.

**Deviations from the design docs**
- **`*.Contracts` projects reference `Lms.SharedKernel`.** [01 §2.2](../design/01-architecture.md) originally said Contracts have *no* project references. They cannot: DTOs are typed with `CourseId` and `PagedResult<T>`. Since `SharedKernel` is a leaf, the acyclicity argument is unaffected. **Doc updated in this PR** (`01-architecture.md` §2.2 and `src/CLAUDE.md`).

---

### `F-2` Aspire AppHost with persistent data

| | |
|---|---|
| **Estimate** | 2 pts |
| **Actual** | 2 pts |
| **Area** | `infra` |
| **Branch** | `feat/f-2-aspire-apphost` |
| **PR** | pending — `gh` not yet installed |
| **Started / Finished** | 2026-08-08 / 2026-08-08 |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] Dashboard up at `https://localhost:17298`; postgres, pgweb, azurite and api all started
- [x] `api` healthy — `GET /health/live` → `200 {"status":"healthy"}` on both http and https
- [x] Blob containers `course-assets` and `lesson-attachments` created in Azurite (verified in `__azurite_db_blob__.json`)
- [x] **Persistence proven the hard way** — see below
- [x] `scripts/reset-local-data.ps1` written, with per-target flags and a confirmation prompt

**Verification — persistence**
Tested destructively rather than by restarting the process, because only the destructive
version actually exercises `WithDataVolume()`:

1. Inserted a row into `lmsdb` at `03:54:51`
2. Killed the AppHost, then **`docker rm -f` on every container**
3. Confirmed the named volumes survived
4. Restarted the AppHost — a **new** container (`97f71b7ada52`, created `03:57:21`)
5. Queried: the row was still there, **2.5 minutes older than the container holding it**

Also confirmed the weaker property along the way: with `ContainerLifetime.Persistent`, killing
the AppHost leaves the containers running, so a normal restart skips container startup entirely.

**Shipped**
- `src/Lms.AppHost` — Aspire 13.4.6 AppHost, added to the solution
- PostgreSQL 18.3 with `WithDataVolume("lms-postgres-data")` + `ContainerLifetime.Persistent` + pgweb; database `lmsdb`
- Azurite 3.35.0 with `WithDataVolume("lms-azurite-data")` + `ContainerLifetime.Persistent`; both blob containers
- `Lms.Api` wired with references to the database and both containers, `WaitFor(postgres)`
- `scripts/reset-local-data.ps1` — removes containers holding the volumes first (they lock them), then the volumes

**Decisions**
1. **`AddBlobContainer`, not `AddBlobs`.** `AddBlobs` only names the blob service endpoint; it does not create a container. Verified by inspecting the Aspire assembly, then confirmed against Azurite's metadata. The overload on `IResourceBuilder<AzureBlobStorageResource>` is obsolete — the current one hangs off `IResourceBuilder<AzureStorageResource>` directly, which `TreatWarningsAsErrors` caught at build time.
2. **Named volumes rather than Aspire's generated names**, so `reset-local-data.ps1` can target them predictably.
3. **`ServiceDefaults` deliberately excluded** — that is `F-3`. The AppHost grows with the migration service (`F-4`) and the Vite app (`F-7`).

**Deviations from the design docs**
- None. [06 §3.1](../design/06-tech-stack.md) already specified `AddBlobContainer`; the first draft of `AppHost.cs` used `AddBlobs` by mistake and was corrected.

---

## Sprint 2 — Aug 17–23, 2026

**Goal:** *A real database, behind a real health check.*

### `F-3` ServiceDefaults, health, OpenAPI, ProblemDetails

| | |
|---|---|
| **Estimate** | 3 pts |
| **Actual** | 3 pts |
| **Area** | `api` |
| **Branch** | `feat/f-3-service-defaults` (stacked on `feat/f-2-aspire-apphost`) |
| **PR** | pending — `gh` not yet installed |
| **Started / Finished** | 2026-08-08 / 2026-08-08 |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] `GET /health/live` → `200`, liveness check only, does not touch dependencies
- [x] `GET /health/ready` → `200`, the place dependency checks will register
- [x] `GET /openapi/v1.json` → `200`, valid OpenAPI 3.1.1 document
- [x] Document viewer reachable in Development
- [x] Unhandled exceptions become RFC 9457 ProblemDetails, never a stack trace
- [x] `ErrorType` → HTTP status mapped in exactly one place
- [x] Build clean, **78 tests** passing (was 60)

**Verified live** against a running AppHost:
```
/health/live     200  {"status":"Healthy","checks":[{"name":"self",...}]}
/health/ready    200  {"status":"Healthy",...}
/openapi/v1.json 200  openapi 3.1.1, "paths": {}   ← health correctly excluded
/scalar/v1       200  html
```

**Shipped**
- `Lms.ServiceDefaults` — OpenTelemetry (traces/metrics/logs), health checks, service discovery, HTTP resilience
- `Lms.SharedKernel.Http` — `ToHttpResult()`, `ToCreatedResult()`, `ToPagedHttpResult()`, `HttpResults.Problem()`, `PagingParams` with query-string binding
- `GlobalExceptionHandler` — `IExceptionHandler` producing ProblemDetails with a correlated `traceId` and a deliberately generic message
- `Program.cs` — ServiceDefaults, exception handler, string enum serialisation, OpenAPI, Scalar, `MapDefaultEndpoints()`
- 18 new tests, mostly pinning the `ErrorType` → status mapping

**Decisions**
1. **Health endpoints are mapped in every environment, not just Development.** The Aspire template restricts them because a detailed payload leaks dependency names and failure reasons — but Container Apps must probe them in production. Resolved by hiding the *detail* rather than the endpoint: outside Development the body is a single status word.
2. **`/health/live` deliberately does not check dependencies.** A failing database must not get the container killed and restarted in a loop. Readiness is where dependency checks go.
3. **A third SharedKernel project (`.Http`).** Same reasoning as `.Persistence`: `SharedKernel` must stay free of a framework reference because `*.Contracts` projects reference it and have to remain plain DTOs.
4. **Scalar instead of Swagger UI** — see deviation below.
5. **Both health endpoints are `ExcludeFromDescription()`** — they are operational, not part of the API. Confirmed by `"paths": {}` in the generated document.

**Deviations from the design docs**
- **`MapSwaggerUi()` does not exist.** [03 §1](../design/03-api-design.md) and [06 §1](../design/06-tech-stack.md) both claimed it ships with `Microsoft.AspNetCore.OpenApi`. Inspecting the 10.0.10 assembly, the only mapping method is `MapOpenApi` — the package deliberately ships no UI. Switched to **Scalar** (`MapScalarApiReference()`, free and OSS). **Both docs corrected in this PR.**
- **Security fix, unplanned.** `Microsoft.AspNetCore.OpenApi` 10.0.10 pulls in `Microsoft.OpenApi` 2.0.0, which carries a **known high-severity advisory (GHSA-v5pm-xwqc-g5wc)**. `TreatWarningsAsErrors` turned NU1903 into a build failure. Pinned transitively to 2.11.0 in `Directory.Packages.props`, with a note to remove the pin once the parent ships a patched reference.

**Not yet verified**
- `GlobalExceptionHandler` is covered by design but not exercised end to end — there is no endpoint that throws yet. It will be hit by integration tests from `S-2` onward.

### `F-4` MigrationService and EF conventions

| | |
|---|---|
| **Estimate** | 2 pts |
| **Actual** | 2 pts |
| **Area** | `api` |
| **Branch** | `feat/f-4-migration-service` (stacked on `feat/f-3-service-defaults`) |
| **PR** | pending — `gh` not yet installed |
| **Started / Finished** | 2026-08-08 / 2026-08-08 |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] Migration job runs to completion **before** the API starts (`WaitForCompletion`)
- [x] Job exits non-zero on failure, so a deploy pipeline stops rather than rolling out against an unmigrated schema
- [x] UUIDv7 keys generated in application code, no database default
- [x] `xmin` available as an optimistic-concurrency token
- [x] Typed-ID value converters applied by convention, not per entity
- [x] Build clean, **93 tests** passing (was 78)

**Verified against real PostgreSQL**
```
notifications.__ef_migrations_history_notifications
  20260808050355_InitialNotifications | 10.0.10      ← proves the job ran

notifications.outbox_messages
  id              uuid                     not null   (no default — UUIDv7 from app code)
  payload         jsonb                    not null
  recipient_email character varying(256)   not null
  …
  "ix_outbox_messages_pending" btree (sent_at) WHERE sent_at IS NULL
```
Migration process had already exited when the API came up; `/health/ready` → `200`.

**Shipped**
- `Lms.SharedKernel.Persistence` — `StronglyTypedIdConverter<TId>` + `ApplyStronglyTypedIdConventions()`, `IsXminConcurrencyToken()`, `UseLmsConventions()`, `UseLmsMigrationHistory()`
- `Lms.MigrationService` — worker resolving `DbContext` (agnostic of module count), source-generated logging, execution strategy for transient connection faults, non-zero exit on failure
- `NotificationsDbContext` + `OutboxMessage` + configuration + the `InitialNotifications` migration
- `.config/dotnet-tools.json` pinning `dotnet-ef`
- 15 new tests

**Decisions**
1. **`EFCore.NamingConventions` for snake_case.** EF Core 10 has no built-in convention. pgweb is in the AppHost specifically so the database can be read by hand, and quoted `"PascalCase"` identifiers make that miserable.
2. **`IsRowVersion()` alone is enough for `xmin`.** The Npgsql model-finalising convention points the column at `xmin` and suppresses its DDL. An explicit `HasColumnName("xmin")` was redundant and did not compile — removed.
3. **`.editorconfig` exempts `**/Migrations/*.cs`.** `dotnet ef migrations add` writes code that violates the house style (file-scoped namespaces, formatting), and would reintroduce it on every future card. Generated code is not hand-edited.
4. **`dotnet-ef` pinned to 10.0.10 via a tool manifest.** `dotnet tool install --prerelease` pulled an EF **11 preview** tool against EF Core 10 packages. Corrected, then pinned so every machine and CI agent matches.
5. **The runner resolves `DbContext`, not concrete types**, so adding a module means one registration line in `Lms.MigrationService/Program.cs` and nothing else.

**Deviations from the design docs**
- **`OutboxMessage` was built here, not in `P-7` (Sprint 26).** A migration pipeline you cannot verify is a bad card, and an empty initial migration proves nothing. The table is fully specified in [02 §5](../design/02-domain-model.md), depends on no other entity, and is infrastructure rather than domain. **`P-7` now needs only the sender and the event handler** — noted against that card in [08 §4](../design/08-delivery-plan.md) so the estimate is not double-counted.
- **Docs reconciled in this PR:** `06-tech-stack.md` package list + `dotnet-ef` pinning note, `CLAUDE.md` commands (was a TODO stub), `src/CLAUDE.md` EF conventions, `02-domain-model.md` §5.

---

## Card template

```markdown
### `X-N` Title

| | |
|---|---|
| **Estimate** | N pts |
| **Actual** | N pts |
| **Area** | api / web / infra / both |
| **Branch** | feat/x-n-slug |
| **PR** | #N |
| **Started / Finished** | YYYY-MM-DD / YYYY-MM-DD |
| **Status** | Done |

**Acceptance criteria**
- [x] …

**Shipped** — what exists now that did not before.
**Decisions** — anything chosen at implementation time that a future reader would ask "why?" about.
**Deviations from the design docs** — what differed, and whether the doc was updated to match.
```

---

## Decision index

Implementation decisions worth finding later. Full context lives in the card entry above.

| Date | Card | Decision |
|---|---|---|
| 2026-08-08 | `F-1` | `Lms.SharedKernel.Persistence` split out so EF Core stays invisible to Module `Domain` folders |
| 2026-08-08 | `F-1` | Shouldly over FluentAssertions — v8+ of the latter is commercially licensed |
| 2026-08-08 | `F-1` | xunit v3; the `dotnet new xunit` template still emits v2 |
| 2026-08-08 | `F-1` | CA1000/CA1711/CA1716 disabled with justification; they are library-author rules |
| 2026-08-08 | `F-1` | Typed ids live in SharedKernel — shared vocabulary, not owned by one Module |
| 2026-08-08 | `F-1` | `*.Contracts` may reference SharedKernel; design doc amended to match |
| 2026-08-08 | `F-2` | `AddBlobContainer` on the storage resource — `AddBlobs` names an endpoint, it does not create a container |
| 2026-08-08 | `F-2` | Named Docker volumes (`lms-postgres-data`, `lms-azurite-data`) so the reset script can target them |
| 2026-08-08 | `F-3` | Health endpoints mapped in all environments; detail hidden outside Development rather than the endpoint |
| 2026-08-08 | `F-3` | `/health/live` never touches dependencies — a bad DB must not restart-loop the container |
| 2026-08-08 | `F-3` | `Lms.SharedKernel.Http` added so `SharedKernel` stays framework-free for Contracts |
| 2026-08-08 | `F-3` | **Scalar** replaces the non-existent `MapSwaggerUi`; both design docs corrected |
| 2026-08-08 | `F-3` | Pinned `Microsoft.OpenApi` 2.11.0 — transitive 2.0.0 has advisory GHSA-v5pm-xwqc-g5wc |
| 2026-08-08 | `F-4` | snake_case via `EFCore.NamingConventions` — pgweb exists so the DB can be read by hand |
| 2026-08-08 | `F-4` | `IsRowVersion()` alone maps `xmin`; naming the column explicitly is redundant |
| 2026-08-08 | `F-4` | Generated migrations exempt from house style in `.editorconfig`; never hand-edited |
| 2026-08-08 | `F-4` | `dotnet-ef` pinned in `.config/dotnet-tools.json` — must match the EF Core major |
| 2026-08-08 | `F-4` | Outbox **table** pulled forward from `P-7` so the migration pipeline is verifiable |
