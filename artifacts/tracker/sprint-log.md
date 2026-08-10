# Sprint Log

Progress board and recorded actuals. Updated as the **last action of every card**.

*Last updated: 2026-08-10 · Sprint 6 merged as PR #11, Sprint 7 opened*

---

## Status at a glance

```
Points   ███████░░░░░░░░░░░░░░░░░░░░░░░   31 / 145   (21%)
Sprints  ██████░░░░░░░░░░░░░░░░░░░░░░░░    6 / 31
Cards    ███████░░░░░░░░░░░░░░░░░░░░░░░   13 / 58
```

| | |
|---|---|
| **Phase** | 2 of 8 — Auth & Design System · only `A-5` and `SP-1` remain |
| **Last completed** | Sprint 6 — *Every future screen inherits a look* — **8 of 5 points**, clearing two sprints of debt |
| **In progress** | **Sprint 7** — `A-5` route guards + login/register pages (3) · `SP-1` YouTube IFrame spike (2) |
| **Next milestone** | **M1 Hello, deployed** — Sprint 9, **11 Oct 2026** |
| **Schedule** | On plan. Not re-dated — see the re-baseline below. |
| **Tests** | **214 green** (133 unit · 35 architecture · 39 integration · 8 web) |
| **Build** | Clean, warnings-as-errors |
| **Open branches** | `feat/a-5-route-guards` — Sprint 7 in progress |
| **Carried work** | **None.** |

### Phases

| # | Phase | Sprints | Pts | Status |
|---|---|---|---:|---|
| 1 | Foundation | 1–3 | 15 | ✅ **Done** — `F-1`…`F-7` |
| 2 | Auth & Design System | 4–7 | 20 | 🔵 **In progress** — `A-1`…`A-4`, `A-6`, `W-1` done; `A-5` and `SP-1` remain |
| 3 | Deploy | 8–9 | 9 | ⬜ `D-1`…`D-3` → **M1** |
| 4 | Instructor Studio | 10–16 | 35 | ⬜ `S-1`…`S-12`, `W-2` → **M2** |
| 5 | Catalog & Enrollment | 17–19, 22 | 20 | ⬜ `C-1`…`C-9`, `W-3` → **M3** |
| 6 | Player & Completion | 23–26 | 21 | ⬜ `P-1`…`P-8` → **M4** |
| 7 | Design pass | 27–28 | 10 | ⬜ `W-4`, `W-5`, `C-8` |
| 8 | Hardening & Launch | 29–31 | 15 | ⬜ `H-1`…`H-7` → **M5** |

*Sprints 20–21 (21 Dec – 3 Jan) are planned at zero. Sprints 32–33 are buffer.*

### Milestones

| | Milestone | Sprint | Date | Status |
|---|---|---|---|---|
| **M1** | Hello, deployed | 9 | 11 Oct 2026 | ⬜ |
| **M2** | An instructor can publish | 16 | 29 Nov 2026 | ⬜ |
| **M3** | A student can find and enroll | 22 | 10 Jan 2027 | ⬜ |
| **M4** | MVP feature-complete | 26 | 7 Feb 2027 | ⬜ |
| **M5** | Launch-ready | 31 | 14 Mar 2027 | ⬜ |

### Cards delivered

| Card | Title | Pts | Branch | Merged |
|---|---|---:|---|---|
| `F-1` | Solution skeleton + SharedKernel | 3 | `feat/f-1-solution-skeleton` | ✅ PR #1 |
| `F-2` | Aspire AppHost, persistent data | 2 | `feat/f-2-aspire-apphost` | ✅ PR #2 |
| `F-3` | ServiceDefaults, health, OpenAPI | 3 | `feat/f-3-service-defaults` | ✅ PR #2 |
| `F-4` | Migration job + EF conventions | 2 | `feat/f-4-migration-service` | ✅ PR #3 |
| `F-5` | Architecture tests | 2 | `feat/f-5-architecture-tests` | ✅ PR #4 |
| `F-7` | TanStack Start scaffold | 2 | `feat/f-7-web-scaffold` | ✅ PR #5 |
| `F-6` | CI workflow | 1 | `feat/f-6-ci` | ✅ PR #6 |
| `A-1` | Identity: users, roles, register/login | 3 | `feat/a-1-identity-module` | ✅ PR #7 |
| `A-2` | JWT validation, policies, me/refresh/logout | 2 | `feat/a-2-jwt-policies` | ✅ PR #8 |
| `A-3` | BFF session cookie | 4 | `feat/a-3-bff-session` | ✅ PR #11 |
| `IT-1` | Integration test harness | 2 | `feat/a-3-bff-session` | ✅ PR #11 |
| `W-1` | Design system + app shell | 3 | `feat/a-3-bff-session` | ✅ PR #11 |
| `A-6` | Admin grant-instructor | 2 | `feat/a-3-bff-session` | ✅ PR #11 |

**`main` is at `8f4d00c`.** PRs #1–#8 and #11 merged and their branches deleted; all thirteen
delivered cards are on `main`. Sprint 7 branches fresh off it.

### Open risks

| Risk | State |
|---|---|
| **R1** BFF session/refresh pattern | 🟢 **Retired.** Sealing, reading, tamper resistance, transparent refresh and now sign-out are all proven in a browser. The pattern did not fight us; a 204-handling bug in our own HTTP wrapper did. |
| **R2** YouTube IFrame progress tracking | 🟡 Spike `SP-1` scheduled Sprint 7, four months before the real card |
| **R3** Velocity below 5 pts/week | 🔴 **Still unmeasured after six sprints.** All six ran as single sittings. Sprint 6's 8.0 is debt repayment, not throughput, and must not be read as headroom. Re-baseline checkpoint is end of Sprint 9. |
| **R4** Life happens | ⬜ 4 weeks of slack built in (2 holiday + 2 buffer) |
| **R5** Scope creep | 🟢 **Nothing carried.** Both items closed in Sprint 6 — the sign-out fix and the integration suite that had slipped twice. |
| **R8** Design rabbit hole | 🟢 `W-1` came in at estimate. One accent colour, shadcn defaults everywhere else, and no time spent on Studio CSS. Next design work is `W-4`, Sprint 27. |

---

## Velocity

| Sprint | Dates | Planned | Completed | Velocity | Notes |
|---|---|---:|---:|---:|---|
| 1 | Aug 10–16 | 5 | 5 | 5.0 | ✅ Both cards done. Started early (Aug 8) in one sitting, so this is **not** a valid velocity sample — see caveat. |
| 2 | Aug 17–23 | 5 | 5 | 5.0 | ✅ `F-3` + `F-4`. Also completed early, same sitting. Estimates held on both. |
| 3 | Aug 24–30 | 5 | 5 | 5.0 | ✅ `F-5` + `F-7` + `F-6`. Cards reordered within the sprint. |
| 4 | Aug 31–Sep 6 | 5 | 5 | 5.0 | ✅ `A-1` + `A-2`. **Integration-test project carried to Sprint 5** — see the caveat. |
| 5 | Sep 7–13 | 5 | 3 | 3.0 | 🟡 `A-3` partial — sign-out defect. `A-4` was already delivered in `A-2`. Integration tests carried **again**. |
| 6 | Sep 14–20 | 5 | 8 | 8.0 | ✅ `A-3` sign-out fix (1) · `IT-1` harness (2) · `W-1` (3) · `A-6` (2). **8 against a 5-point plan** — two of those points are Sprint 4/5 debt being repaid, so the sprint did ~6 points of new work and cleared the backlog. Still a single sitting. |
| 7 | Sep 21–27 | 5 | — | — | ⬜ `A-5` route guards + login/register pages (3) · `SP-1` YouTube IFrame spike (2) |

> 1 point ≈ 2 focused hours. Record **actual** points as hours ÷ 2, honestly — an inflated
> actual hides a velocity problem until it is expensive to discover.

**Rolling average:** **5.2 pts/sprint** over 6 sprints (31 delivered of 30 planned).

Read that carefully rather than cheerfully. Sprint 6's 8.0 is **debt repayment**, not throughput:
3 of its points were work planned for Sprints 4 and 5 and not done then. Spread honestly across
the sprints that owed them, the picture is closer to a flat 5 with one bad sprint in the middle.
And every sprint so far has still run in a single sitting, so this remains an estimate-accuracy
number rather than a sustained-pace one.

**What Sprint 6 actually showed:** four cards, four estimates, four exact hits. The estimates are
good. What is still unmeasured after six sprints is whether the *cadence* holds across evenings
with context lost in between — the thing risk **R3** is actually about.

> **Sprint 4 caveat.** Scored 5/5, but the integration-test project (`tests/Lms.IntegrationTests`,
> WebApplicationFactory + Testcontainers) was in the plan and was **not built**. Everything was
> verified by hand against a live stack instead — real evidence, but not repeatable and not in CI.
> A truer score is **4 of 5**. Carried into Sprint 5 rather than quietly dropped.

**Re-baseline checkpoints:** ~~end of Sprint 3~~ ✅ done · end of Sprint 9

### Re-baseline — after Sprint 3

[08 §2.3](../design/08-delivery-plan.md) says to divide points completed by three and re-date the plan. The honest reading:

**15 of 15 points delivered. Every estimate held exactly.** No card carried, split, or descoped.

**And it is still not a valid velocity sample.** All three sprints ran as single continuous
sittings on 8 Aug, not across weeks of evenings. The plan's 5 pts/week assumes ~11h spread over
Mon–Thu evenings plus a Saturday morning, with context lost and reloaded between sessions. These
numbers measure **estimate accuracy**, not **sustained pace** — different things. Fatigue,
interruption, and the cost of picking work back up after four days away are precisely what this
sample excludes.

**Recommendation: do not re-date the plan.** Keep the [08 §3.1](../design/08-delivery-plan.md)
milestones as they stand — M1 11 Oct, M5 14 Mar — and treat **Sprints 4–6 as the real
calibration**, being the first worked at the intended cadence. Re-dating on this sample would
swap a stated assumption for false precision.

**What the sample does support:**
- Estimates have been accurate three sprints running, so the 1–5 point scale is well judged.
- The foundation phase hid no work — nothing needed splitting or a follow-up card.
- Four unplanned findings surfaced regardless (a CVE, two non-existent APIs, a licensing trap). Foundation cards attract that; feature cards from Sprint 4 should be less eventful, which is itself a reason not to extrapolate upward.

**Trigger to act:** if Sprint 4 lands below 4 points at real cadence, re-date everything from the
measured number and pull the [§8 descope levers](../design/08-delivery-plan.md) in order.

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
| **PR** | ✅ #1 merged |
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
| **PR** | ✅ #2 merged |
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
| **Branch** | `feat/f-3-service-defaults` |
| **PR** | ✅ #2 merged |
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
| **Branch** | `feat/f-4-migration-service` |
| **PR** | ✅ #3 merged |
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

## Sprint 3 — Aug 24–30, 2026

**Goal:** *Guardrails up, frontend talking to the API.*

> **Cards were reordered `F-5` → `F-7` → `F-6`.** The plan listed `F-6` second, but writing CI
> before the web app existed would have landed a .NET-only workflow that `F-7` immediately
> reopened. Same sprint, same points, one fewer revisit.

### `F-5` Architecture tests

| | |
|---|---|
| **Estimate** | 2 pts · **Actual** 2 pts |
| **Area** | `api` |
| **Branch** | `feat/f-5-architecture-tests` |
| **PR** | ⬜ open — merge after `F-4` |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] `tests/Lms.ArchitectureTests` with the rules from [01 §4](../design/01-architecture.md)
- [x] **31 tests**, all green
- [x] Failures name the offending type *and* its exact dependency
- [x] **Proved the guardrail fails when violated** (see below)

**Verified by breaking it on purpose.** Added an EF Core reference to `Lms.SharedKernel`; rule 1 went red with:
```
Offending types:
  - Lms.SharedKernel.Results.DeliberateViolation  (Has dependency on: Microsoft.EntityFrameworkCore.DbContext)
```
Then reverted. A guardrail nobody has watched fail is not a guardrail.

**Shipped** — 7 NetArchTest rules (SharedKernel purity, Contracts isolation, Domain free of EF/ASP.NET, no cross-module reach-in, no Aspire in modules, handlers `internal sealed`), plus `PagingConventionTests` scanning source for `Skip(`/`Take(` outside `SharedKernel.Persistence`. Assembly markers added to the three previously-empty Contracts projects.

**Decisions**
1. **`NetArchTest.eNhancedEdition`, not `NetArchTest.Rules`.** The original has been dormant since 1.3.2. Both target netstandard2.0 and neither couples to an assertion library.
2. **A source scan for the paging rule.** NetArchTest works on types; `Skip(`/`Take(` is about call sites. Includes a self-check that the scan actually finds files, so a broken path cannot make the rule pass silently.

**Gotchas worth remembering**
- The package's XML docs advertise `Predicate.ResideInNamespaceStartingWith`, but the shipped assembly does not expose it. `ResideInNamespace` is the working equivalent.
- **`nameof(DbContext)` does not trip a dependency rule** — `nameof` is compile-time and emits no IL reference. My first violation probe used it and the test passed, which briefly looked like a broken rule. These rules read IL, not source text.

---

### `F-7` TanStack Start scaffold

| | |
|---|---|
| **Estimate** | 2 pts · **Actual** 2 pts |
| **Area** | `web` |
| **Branch** | `feat/f-7-web-scaffold` |
| **PR** | ⬜ open — merge after `F-5` |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] `web/` scaffolded — React 19, Tailwind v4, ESLint, no example pages
- [x] `AddViteApp` in the AppHost, waiting on the API
- [x] **Server function calls the API server-side and renders the result**
- [x] `npm run build` and `npm run lint` clean
- [x] `node_modules/` and `dist/` ignored; 19 files committed

**Verified under the AppHost.** Page rendered **`Healthy`** with the API base resolved to `https://localhost:7197` — taken from `services__api__https__0`, injected by `WithReference(api)`. The browser made no direct call to the API. That is the BFF path [04](../design/04-adr-authentication.md) depends on, working before auth is built on it.

**Decisions**
1. **Scaffolded into a temp directory, then moved.** `web/CLAUDE.md` already existed and the CLI wants an empty target. Also removed the `.git` the template created.
2. **`src/server/` as the single door to the API.** Never imported from a component; it is where the session cookie lands in Sprint 5.
3. **Vite port from `PORT`**, falling back to 3000, so `npm run dev` still works without an AppHost.
4. **TanStack Query, shadcn/ui and Zod deliberately not added yet** — they arrive with the cards that need them.

**Deviation** — [06 §3.1](../design/06-tech-stack.md) called `.WithNpmPackageInstallation()`, which does not exist in `Aspire.Hosting.JavaScript`. Removed, and the package is now named in the doc (it was unnamed; `Aspire.Hosting.NodeJs` stopped at 9.5.2 and has no 13.x).

---

### `F-6` CI

| | |
|---|---|
| **Estimate** | 1 pt · **Actual** 1 pt |
| **Area** | `infra` |
| **Branch** | `feat/f-6-ci` |
| **PR** | ⬜ open — merge after `F-7`; first run of the workflow |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] Two parallel jobs, on push to `main` and on PRs
- [x] Backend: `global.json` SDK, `dotnet tool restore`, Release build, all 124 tests
- [x] Frontend: `npm ci`, lint, build
- [x] NuGet and npm caches; superseded runs cancelled
- [x] **Every step run locally in Release first**

**Decisions** — `npm ci` over `install` so a stale lock file fails the build; Release build inherits `TreatWarningsAsErrors`, which is also what makes NU1903 advisories fail; architecture tests run in CI, so boundaries are enforced mechanically rather than by review.

**Not yet verified** — the workflow has not run on GitHub. It is verified locally step by step; the first real run happens when the branch is pushed.

---

## Sprint 4 — Aug 31 – Sep 6, 2026

**Goal:** *Users exist and can get a token.*

### `A-1` Identity module: users, roles, register/login

| | |
|---|---|
| **Estimate** | 3 pts · **Actual** 3 pts |
| **Area** | `api` |
| **Branch** | `feat/a-1-identity-module` |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] `AppUser`/`AppRole` on `Guid` keys, EF stores on the `identity` schema
- [x] `InitialIdentity` migration; three roles seeded idempotently
- [x] `POST /api/auth/register` → `201`, always `Student`, `409` duplicate, `400` weak password
- [x] `POST /api/auth/login` → token pair; **one indistinguishable `401`** for every failure
- [x] Refresh tokens stored as SHA-256 hashes, never raw

**Verified against Postgres**
```
register        201 / 409 / 400
login           200, expiresIn=900, token decodes to sub, email, name, role
enumeration     wrong password and unknown email → byte-identical bodies
                (traceId normalised); 72ms vs 60ms
refresh_tokens  64-char hex hashes, no raw values
```

**Decisions**
1. **Custom JWTs, not `MapIdentityApi`.** The ADR contradicted itself; three signals to one favoured JWT (see deviation).
2. **15-minute access token**, not the hour the ADR stated. A JWT cannot be revoked early, so the lifetime *is* the revocation window.
3. **A dummy password hash on the unknown-email path**, so response timing does not leak which addresses exist. Free to do, invisible if skipped, and impossible to retrofit convincingly.
4. **Committed development signing key** so `dotnet run` needs no setup; `JwtOptionsValidator` refuses to start outside Development if it is still in use.
5. **No `AddDefaultTokenProviders()`** — those serve email confirmation and password reset (`H-1`) and would require wiring `AddDataProtection()` for a flow nothing calls.
6. **`AddIdentityPersistence` split from `AddIdentityModule`**, so the migration job does not validate a signing key it never uses. Applied to Notifications too; the pattern is now in `src/CLAUDE.md`.
7. **Identity framework tables lose the `asp_net_` prefix** — already namespaced by the schema, and free to change before the migration was ever applied.

**Deviations from the design docs**
- **`04 §3.1` said `MapIdentityApi`** (opaque tokens) while also specifying JWT claims, while `§5` relies on repointing `JwtBearerOptions.Authority`, and `03 §3` shows `"accessToken": "eyJ..."`. **ADR corrected**, along with the token lifetime.
- **`ErrorType` had no 401.** `03 §1.2` specifies one, but `F-1` shipped only 400/403/404/409/422. Added `Unauthenticated`; the "every error type has an explicit mapping" test would have caught a silent 500 fallthrough.

**Gotcha** — `nameof(DbContext)` does not trip a dependency rule; the first violation probe used it and passed. Architecture rules read IL, not source text.

---

### `A-2` JWT validation, policies, session endpoints

| | |
|---|---|
| **Estimate** | 2 pts · **Actual** 2 pts |
| **Area** | `api` |
| **Branch** | `feat/a-2-jwt-policies` |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] Issuer, audience, lifetime and signature validated; **`ClockSkew = Zero`**
- [x] Three policies wired from the existing `AuthPolicies` constants
- [x] `GET`/`PUT /api/me`; user id taken from the token, never the body
- [x] `POST /api/auth/refresh` rotates single-use
- [x] `POST /api/auth/logout` revokes; succeeds for unknown tokens
- [x] Rate limit 10 / 5 min on `/api/auth/*`
- [x] Build clean, **147 tests**

**Verified against a running stack**
```
GET /api/me   no token → 401 · valid token → 200 · garbage → 401
refresh       rotates; new token differs
replay        old token → 401 AND the replacement → 401  (chain revoked)
logout        204; refresh afterwards → 401; unknown token → 204
rate limit    401×7 then 429×6
```

**Decisions**
1. **`ClockSkew = TimeSpan.Zero`.** The five-minute default would silently turn a deliberately short 15-minute token into a 20-minute one.
2. **`MapInboundClaims = false`** so `sub` and `role` survive as written rather than being rewritten to SOAP-era URIs.
3. **Refresh reuse revokes the whole chain.** A rotated-away token reappearing means replay; the chain is no longer trustworthy. Without it a stolen token is silently useful for fourteen days.
4. **Logout succeeds for unknown tokens** — it must not be an oracle for whether one exists.
5. **Rate limiting is per caller, on top of per-account lockout.** Lockout does nothing against credential stuffing spread across many accounts.

**Two bugs found by using it**
- **`Result<Unit>.ToHttpResult()` returned `200 {}`** — it bound to the generic `ToHttpResult<T>` and serialised `Unit`. Added a `Result<Unit>` overload returning `204`. This would have been wrong for *every* command that returns nothing.
- **The rate-limit policy name first lived in `Lms.Api`**, which the Identity module cannot reference. The compiler caught the inverted dependency the architecture rules exist to prevent; the constant moved to `SharedKernel.Authorization.RateLimitPolicies`.

**Not done — carried**
- **`tests/Lms.IntegrationTests`** (WebApplicationFactory + Testcontainers) was in the plan and is not built. Every criterion above was verified by hand against a live stack, which is not the same as a repeatable test. **Carried into Sprint 5** — see the note below.

---

## Sprint 5 — Sep 7–13, 2026

**Goal:** *No token ever reaches browser JavaScript.*

### `A-3` BFF session cookie and transparent refresh

| | |
|---|---|
| **Estimate** | 4 pts · **Actual** 4 pts (3 in Sprint 5 + 1 in Sprint 6) |
| **Area** | `web` |
| **Branch** | `feat/a-3-bff-session` |
| **Status** | ✅ **Done** — sign-out fixed in Sprint 6, see below |

**Done and verified in a real browser**
- [x] `__Host-session` cookie, AES-256-GCM sealed, `HttpOnly; Secure; SameSite=Lax; Path=/`
- [x] **`document.cookie` empty while signed in**; no JWT in the DOM, no localStorage, no sessionStorage
- [x] Login response body is `{ok:true}` — the token pair never crosses to the browser
- [x] Session survives reload **and a full server restart**
- [x] Tamper resistance — invalid ciphertext, forged plaintext JSON, empty value: all render signed out with `200`, never a 500
- [x] Transparent refresh at 5 minutes before expiry; a failed refresh clears rather than 500s
- [x] **Sign-out clears the cookie** — fixed 2026-08-09, root cause below

**The defect, and what it actually was**

Every hypothesis in Sprint 5 was about `Set-Cookie` — `deleteCookie`'s attribute set, the `__Host-`
rules, empty-value serialisation, stale HMR. All four were wrong, and so was the conclusion drawn
from the probe cookie.

**`clearSession()` was never reached.** `POST /api/auth/logout` answers **204 No Content** — the
`Result<Unit>` overload added in `A-2`. `apiPost` called `response.json()` unconditionally, and
`json()` on an empty body throws `SyntaxError: Unexpected end of JSON input` rather than returning
null. The throw happened on the line above `clearSession()`, so the cookie was never cleared and
the rejected promise surfaced to the browser as a resolved sign-out.

`apiFetch` had carried the correct `status === 204 ? undefined : await response.json()` guard since
`A-3` was written. `apiPost` never got it, because until logout every caller returned a body.

Found by reproducing against a standalone `vite dev` where the server stack trace is visible.
Under the AppHost the same stack goes to the dashboard and had not been read — four hypotheses
were tested against browser-side symptoms when one server-side stack trace named the line.

**Why the probe cookie misled.** It was set inside `clearSession()`, below the throw, so it never
executed. "The probe did not appear either" was read as evidence about the `Set-Cookie` path when
it was evidence that the function did not run at all.

**Also changed, deliberately.** Sign-out is now a document POST to `/sign-out` returning 303 rather
than a client-invoked RPC. It was already the planned next step and it is the better shape
regardless: the browser applies cookie changes on a real navigation, all client router state is
discarded rather than invalidated, and it works with JavaScript disabled — a fair requirement for
the control that ends a session on a shared machine.

**Verified in a real browser:** sign in → sign out → 303 to `/` → a fresh navigation renders signed
out, so the server no longer receives the cookie. `document.cookie` empty throughout. In Postgres,
`identity.refresh_tokens` shows **zero live tokens** — every row has `revoked_at` set.

**Decisions**
1. **AES-256-GCM, not CBC** — authenticated encryption, so a tampered cookie fails to open instead of decrypting into attacker-chosen content. Demonstrated with a forged plaintext payload.
2. **Refresh *before* forwarding**, not after a 401 — one round trip instead of two, and no transient failure surfaces.
3. **`SESSION_SECRET` follows the `A-1` JWT-key pattern** — committed dev value for zero-setup, refused outside Development.
4. **Cookies are written with `setResponseHeader('Set-Cookie', …)`**, not a cookie helper — the
   primitive TanStack Start's own authentication guide uses. This was applied while hunting the
   defect and turned out not to be the fix, but it is kept: one shared attribute string means
   write and clear cannot drift, which is what the `__Host-` prefix requires.
5. **A deliberately unstyled sign-in form** on the index route. Scaffolding so the layer is exercised rather than merely written; `A-5` replaces it. Shipping an unexercised session layer would have been worse — and in fact the browser is what found the defect.

**Not started**
- `tests/Lms.IntegrationTests` — carried from Sprint 4 and **carried again**. Two sprints running now; this needs to be the first thing in Sprint 6, not the last.

**`A-4`** `GET /api/me` — delivered early in `A-2`. Its point is **not** claimed here.

---

## Sprint 6 — Sep 14–20, 2026

**Goal:** *Every future screen inherits a look — and the auth layer is finally covered by tests.*

Carried work first. See the `A-3` card above for the sign-out fix, which completes that card.

### `IT-1` Integration test harness (carried from Sprints 4 and 5)

| | |
|---|---|
| **Estimate** | 2 pts · **Actual** 2 pts |
| **Area** | `api` |
| **Branch** | `feat/a-3-bff-session` |
| **Status** | ✅ Done — **the carry is closed** |

**Acceptance criteria**
- [x] `tests/Lms.IntegrationTests` — xunit v3, `WebApplicationFactory<Program>` + Testcontainers
- [x] One PostgreSQL container per assembly; migrations applied before the API starts
- [x] **20 tests green**, suite total now **167** (112 unit · 35 architecture · 20 integration)
- [x] Every `A-1`/`A-2` security property that previously existed only as a tracker note
- [x] Runs in CI unchanged — `ubuntu-latest` already has a Docker daemon

**What is covered**

Registration (201 / 409 / 400, and that it grants `Student` and nothing else), login and the
**enumeration defence** (wrong password and unknown email compared byte for byte with only
`traceId` normalised), refresh rotation, single use, **replay revoking the whole chain**, logout
revoking, logout succeeding for an unknown token, logout answering 204 with an empty body,
`/api/me` for anonymous / garbage / valid callers, `instructorSlug` null for a student, rename
taking the id from the token, and the rate limiter firing on `/api/auth/*` but not on `/health`.

**Two real bugs found on the first run**

1. **`RoleSeeder` crashes when two instances start together.** `RoleExistsAsync` then
   `CreateAsync` is check-then-act, and the loser gets a `DbUpdateException` on `RoleNameIndex`
   — not a failed `IdentityResult`. The existing comment claimed "a concurrent replica winning
   the race is fine and expected"; the code did not deliver that, so **the first Container Apps
   deploy with two replicas would have crashed both**. Now caught and logged at Debug.
   The comment was true about intent and false about behaviour, which is the worst kind.
2. **Rate limits were unreachable constants.** Every request under `WebApplicationFactory` has
   no `RemoteIpAddress`, so the whole suite shares one partition and the production limit of ten
   was spent within two tests. `PermitLimit` and `WindowMinutes` now bind from configuration
   with the production values as defaults — the suite raises the ceiling and the rate-limit test
   lowers it on its own host.

**Decisions**
1. **One container for the assembly, unique emails instead of truncation between tests.** A
   container per class multiplies a five-second startup for no isolation gain, and truncation
   would force the suite serial. `Guid`-suffixed addresses keep it parallel and order-independent.
2. **`UseSetting`, not `ConfigureAppConfiguration`.** The latter is appended after `Program.cs`
   has run, so eager reads — the signing key, the rate limits — silently keep the app's own
   `appsettings.json`. Only the connection string survived it, and only because `AddDbContext`'s
   options lambda runs lazily. That near-miss is worth remembering: the tests appeared to work
   while running against the **committed development signing key**.
3. **A distinct test signing key, and the host runs as Production.** `JwtOptionsValidator`
   short-circuits in Development, so a Development test host would never execute the validator
   the deployment depends on.
4. **The wire shapes are restated in the test project** rather than reusing the server's records.
   A rename on the server should break a test, not pass because both sides moved together.
5. **A second `WebApplicationFactory` over the same container** for the rate-limit test. Factories
   are cheap; containers are not.

**Deviations from the design docs** — none. `01 §3` specified `WebApplicationFactory` +
Testcontainers and that is what was built.

---

### `W-1` Design system + app shell

| | |
|---|---|
| **Estimate** | 3 pts · **Actual** 3 pts |
| **Area** | `web` |
| **Branch** | `feat/a-3-bff-session` |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] Tailwind v4 tokens — colour, type scale, spacing, radius — as CSS variables
- [x] Light **and** dark, with **no flash** on load
- [x] shadcn/ui installed and generating into `components/ui`
- [x] App shell: header, nav slot, content container, footer
- [x] Markdown renderer with sanitisation
- [x] Works at 375px, keyboard reachable, no console warnings

**Verified in a browser, measured rather than eyeballed**
```
375px light   horizontal overflow 0px · display name hidden · sign-out reachable
375px dark    horizontal overflow 0px · nothing unreachable by keyboard
toggle        class + localStorage + computed bg/fg invert, both directions
reload        stored 'dark' applied before paint; no hydration warning
```

**Shipped**
- One accent hue (`oklch(… 264)`) for `--primary` and `--ring` in both themes; everything else
  stays neutral
- `lib/theme.ts` — the inline no-flash script and the toggle action
- `components/theme-toggle.tsx`, `components/layout/app-shell.tsx`, `components/layout/site-header.tsx`
- `components/markdown.tsx` — `react-markdown` + `remark-gfm` + `rehype-sanitize`
- `web/vitest.config.ts` and **8 sanitiser tests**; `npm test` added to CI
- Root route now puts auth in route context; `__root.tsx` title fixed

**Decisions**
1. **The theme lives in the `dark` class on `<html>`, never in React state.** The server cannot
   know the visitor's choice, so any server-rendered guess is either a flash or a hydration
   mismatch. The toggle's icon is chosen by CSS (`dark:block` / `dark:hidden`), which is why the
   component needs no state, no effect and no client-only guard.
2. **The no-flash script is inlined in `<head>`.** Applying the class at hydration means every
   dark-mode visitor sees a white flash on every load. `<html>` carries
   `suppressHydrationWarning` because the script edits it before React arrives — that is the
   design, not a papered-over warning.
3. **One accent colour, and only two lines reference the hue.** `W-5` (Sprint 28) can replace it
   without touching a component. Rule 2 of [08 §5](../design/08-delivery-plan.md).
4. **Auth moved into root route context** rather than being fetched per route. The header needs
   it everywhere, and `A-5`'s `_authed` / `_instructor` guards read the same context.
5. **Links in rendered markdown name `href` explicitly instead of spreading props.**
   react-markdown also passes a `node` prop, which reaches the DOM as `node="[object Object]"`
   if forwarded — caught in the browser. Naming the one allowed attribute mirrors the schema's
   allow-list at the render layer.
6. **The sanitiser schema narrows rehype-sanitize's default** rather than listing tags from
   scratch. The default already blocks `script`, `iframe`, event handlers and `javascript:`
   URLs and is maintained by people tracking the bypasses; hand-rolled allow-lists acquire holes.

**Deviations from the design docs**
- **A frontend test runner was added (`vitest`), which no card called for.** The markdown
  sanitiser is a security control and there was no way to stop it regressing silently. Kept
  deliberately thin: no jsdom and no testing-library, just `renderToStaticMarkup` and assertions
  on the output. It also unblocks `A-5`'s guard tests.
- One test initially asserted the string `alert(1)` was absent from the output. It is not — raw
  HTML is never parsed, so `<script>alert(1)</script>` becomes the literal text `alert(1)` inside
  a `<p>`, which is inert. The assertion now checks the *element* is absent, because the original
  would also have failed on any page that legitimately discusses XSS.

**Not done here, by design** — skeletons, empty states and error boundaries are `W-4` (Sprint 27).
The shell has no data of its own to be empty about.

**Scaffolding had landed early, on the `A-3` branch.** shadcn/ui was installed and the token
layer set up while the sign-out defect was still open, so it rides along on
`feat/a-3-bff-session` rather than a fresh branch. What exists: `components.json`, the
`radix-vega` style with `radix-ui` / `cva` / `tailwind-merge` / `lucide-react`, Inter Variable,
~125 lines of `@theme inline` tokens and light/dark variables in `styles.css`, one generated
`Button`, `lib/utils.ts` (`cn`), and the health server function moved out of the route into
`features/health/query.ts` behind the `#/` alias.

It is counted inside the 3 points above, not separately.

**Fixed on the way in:** `components.json` was generated with `rsc: true` (TanStack Start is not
RSC — it would prepend `"use client"` to every future component), `css: src/style.css` (the file
is `styles.css`) and a `tailwind.config.js` path that does not exist under Tailwind v4. All three
would have misdirected the next `shadcn add`. The two generated files also tripped
`import/consistent-type-specifier-style` and were `--fix`ed, so CI stays green.

---

### `A-6` Admin grant-instructor

| | |
|---|---|
| **Estimate** | 2 pts · **Actual** 2 pts |
| **Area** | `api` |
| **Branch** | `feat/a-3-bff-session` |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] `InstructorProfile` entity, configuration and the `AddInstructorProfile` migration
- [x] `POST /api/admin/users/{id}/grant-instructor` → `200`, `409` on a taken slug
- [x] `POST /api/admin/users/{id}/revoke-instructor` → `204`, role only
- [x] `GET /api/admin/users?search=` → `PagedResult<AdminUser>`
- [x] Seeded admin from configuration, with no default password
- [x] `instructorSlug` on `/api/me` is real — the `A-1` placeholder is gone
- [x] **206 backend tests green** (133 unit · 35 architecture · 39 integration)

**Verified against a live stack**
```
admin login          200, token carries role Admin
admin /api/me        200, roles ["Student","Admin"]
grant                200 {"roles":["Student","Instructor"],"instructorSlug":"jane-doe-live"}
jane /api/me         200, instructorSlug present
jane grants          403  ← Instructor is not Admin
admin search         200, one row, no credential fields
```

**Shipped**
- `Domain/InstructorProfile.cs` — factory returning `Result<T>`, slug validated at the boundary
- Three slices: `GrantInstructor`, `RevokeInstructor`, `FindUsers`
- `Endpoints/AdminEndpoints.cs` — one group, `Admin` policy applied at the group
- `Infrastructure/AdminSeeder.cs` + a committed Development-only credential
- Three new `IdentityErrors` entries; 8 unit tests and 12 integration tests

**Decisions**
1. **A real FK from `instructor_profiles.user_id` to `users.id`.** The no-FK rule is about
   *cross-module* references; both tables are in the `identity` schema, so a foreign key is
   correct and the primary key doubles as it — one profile per user, enforced by the key.
2. **The unique index on slug is the arbiter, not a pre-check.** Querying first and inserting
   after still loses to a second admin granting the same slug concurrently, so the handler
   catches the `DbUpdateException` and returns `409`.
3. **Both writes are idempotent.** Granting twice is a retry, not a conflict; revoking a
   non-instructor is a no-op. Admin tooling gets retried by hand.
4. **Revoke keeps the profile and the slug.** Course pages still name the author, and freeing
   the slug would let a later instructor inherit someone else's public URL.
5. **A committed Development-only admin credential**, matching the `A-1` signing key and `A-3`
   session secret. The seeder does nothing at all when either setting is absent — there is no
   default password in the code, and it never resets an existing account, so someone who can
   edit configuration cannot use a redeploy to take over an admin.
6. **`ILIKE` rather than `ToUpper().Contains()`** in the user search. Same work in PostgreSQL,
   states the case-insensitivity in SQL, and does not trip CA1862 on an expression tree.
7. **`github_url` / `linkedin_url` named explicitly.** The snake_case convention splits internal
   capitals into `git_hub_url`; pgweb exists so the schema reads well by hand.

**A bug the live check found that the tests did not**

The seeded admin was granted only `Admin`, so `GET /api/me` returned **403** — contradicting the
authorization matrix in [03 §7](../design/03-api-design.md). "Every registered user holds
`Student`" is an invariant `AuthPolicies.Student` depends on (it means "we know who you are" and
is implemented as `RequireRole(Student)`), and the seeder creates a user directly instead of
going through registration, so it has to uphold the invariant itself. Now grants both roles,
with a regression test.

Worth noting **why the test suite missed it**: every other test obtains its identity through
registration, which grants `Student` automatically. The seeded admin is the only account in the
system created by another path, and nothing exercised it end to end until the live check.

**Also fixed:** `AdminSeeder` shipped with the same check-then-act race as `RoleSeeder` —
`FindByEmailAsync` then `CreateAsync`, where the loser gets a `DbUpdateException` rather than a
failed `IdentityResult`. Caught within minutes by the integration suite, which starts three
hosts concurrently. Writing the same bug twice in one sprint is the argument for the harness.

**Deviations from the design docs**
- **`revoke-instructor` returns `204`, not the `200` in `03 §6`.** A command returning nothing is
  204 here — the `Result<Unit>` convention established in `A-2`. **Doc updated**, along with the
  400/404 cases, the idempotency of both writes, the profile surviving revocation, and a note on
  the seeded admin needing `Student`.

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
| 2026-08-08 | `F-5` | `NetArchTest.eNhancedEdition` — the original has been dormant since 1.3.2 |
| 2026-08-08 | `F-5` | Paging rule enforced by source scan; NetArchTest sees types, not call sites |
| 2026-08-08 | `F-5` | Use `ResideInNamespace`; the advertised `ResideInNamespaceStartingWith` is not in the shipped assembly |
| 2026-08-08 | `F-5` | Dependency rules read IL — `nameof(X)` emits no reference and will not trip them |
| 2026-08-08 | `F-7` | `Aspire.Hosting.JavaScript` 13.4.6 for `AddViteApp`; `Aspire.Hosting.NodeJs` has no 13.x |
| 2026-08-08 | `F-7` | `web/src/server/` is the single door to the API and never imported by a component |
| 2026-08-08 | `F-7` | Vite port from `PORT` so the AppHost assigns it, 3000 standalone |
| 2026-08-08 | `F-6` | `npm ci` not `install`, so a stale lock file fails CI |
| 2026-08-08 | `A-1` | Custom JWTs over `MapIdentityApi`; the ADR contradicted itself and was corrected |
| 2026-08-08 | `A-1` | 15-minute access token — the lifetime *is* the revocation window |
| 2026-08-08 | `A-1` | Dummy password hash on unknown-email login so timing does not leak account existence |
| 2026-08-08 | `A-1` | Dev signing key committed for zero-setup `dotnet run`; validator blocks it outside Development |
| 2026-08-08 | `A-1` | `AddXPersistence` split from `AddXModule` so the migration job skips auth wiring |
| 2026-08-08 | `A-1` | `ErrorType.Unauthenticated` → 401; SharedKernel had no 401 despite the contract specifying one |
| 2026-08-08 | `A-2` | `ClockSkew = Zero`; the 5-minute default would extend a 15-minute token to 20 |
| 2026-08-08 | `A-2` | Refresh reuse revokes the entire chain, making a stolen token detectable |
| 2026-08-08 | `A-2` | `Result<Unit>` overload returning 204; the generic one answered `200 {}` |
| 2026-08-08 | `A-3` | AES-256-GCM for the session cookie — a tampered value fails to open, not decrypts |
| 2026-08-08 | `A-3` | Refresh *before* forwarding, not after a 401 — one round trip, no transient failure |
| 2026-08-08 | `A-3` | Unstyled scaffolding form so the session layer is exercised; it is what found the defect |
| 2026-08-09 | `W-1` | shadcn/token scaffolding committed on the `A-3` branch rather than a fresh one — minor, and `A-3` is still open |
| 2026-08-09 | `W-1` | `components.json` corrected: `rsc: false`, `css: src/styles.css`, empty `config` — Tailwind v4 has no JS config and Start is not RSC |
| 2026-08-09 | `A-3` | Sign-out defect was `apiPost` calling `response.json()` on a 204 — not a `Set-Cookie` problem at all |
| 2026-08-09 | `A-3` | Sign-out is a document POST returning 303, not an RPC — cookie applied on a navigation, and it works without JS |
| 2026-08-09 | `A-3` | Session cookie written via `setResponseHeader`, with one shared attribute string so write and clear cannot drift |
| 2026-08-09 | `A-3` | **Read the server stack trace first.** Four browser-side hypotheses cost a sprint; one `vite dev` stack trace named the line. |
| 2026-08-09 | `IT-1` | One Postgres container per assembly; unique emails per test instead of truncation, so the suite stays parallel |
| 2026-08-09 | `IT-1` | `UseSetting`, not `ConfigureAppConfiguration` — the latter lands after `Program.cs`'s eager config reads |
| 2026-08-09 | `IT-1` | Test host runs as Production with its own signing key, so `JwtOptionsValidator` actually executes |
| 2026-08-09 | `IT-1` | Wire shapes restated in the test project — reusing the server's records would hide a rename |
| 2026-08-09 | `IT-1` | `RoleSeeder` now catches `DbUpdateException`: check-then-act would have crashed a two-replica first deploy |
| 2026-08-09 | `IT-1` | Auth rate limits bind from configuration; `TestServer` has no `RemoteIpAddress`, so the suite shares one partition |
| 2026-08-09 | `W-1` | Theme lives in the `dark` class, never React state; CSS picks the toggle icon, so there is no hydration mismatch |
| 2026-08-09 | `W-1` | No-flash theme script inlined in `<head>`; `<html>` carries `suppressHydrationWarning` by design |
| 2026-08-09 | `W-1` | One accent hue, referenced by exactly two lines, so `W-5` can replace it without touching a component |
| 2026-08-09 | `W-1` | Auth lives in root route context — the header needs it everywhere and `A-5`'s guards will read the same thing |
| 2026-08-09 | `W-1` | Markdown links name `href` explicitly; spreading react-markdown's props leaks `node="[object Object]"` |
| 2026-08-09 | `W-1` | Sanitiser narrows rehype-sanitize's default schema rather than listing tags from scratch |
| 2026-08-09 | `W-1` | **Vitest added, unplanned** — a security control with no regression test is a control with a shelf life |
| 2026-08-09 | `A-6` | Real FK from `instructor_profiles.user_id`; the no-FK rule is about cross-module refs, and both tables are in `identity` |
| 2026-08-09 | `A-6` | The unique slug index is the arbiter — a pre-check still loses to a concurrent grant, so `DbUpdateException` becomes the 409 |
| 2026-08-09 | `A-6` | Grant and revoke are both idempotent; admin tooling gets retried by hand |
| 2026-08-09 | `A-6` | Revoke keeps the profile and slug — course pages still name the author, and the URL must not be inheritable |
| 2026-08-09 | `A-6` | Dev-only admin credential committed; the seeder is a no-op unless both settings are present and never resets an existing account |
| 2026-08-09 | `A-6` | **The seeder must grant `Student` too** — "every registered user holds Student" is what makes `AuthPolicies.Student` mean "authenticated" |
| 2026-08-09 | `A-6` | `ILIKE` over `ToUpper().Contains()` — same SQL, states the intent, and CA1862 cannot read expression trees |
| 2026-08-09 | `A-6` | `revoke-instructor` is 204 not 200; design doc corrected to the `Result<Unit>` convention from `A-2` |
