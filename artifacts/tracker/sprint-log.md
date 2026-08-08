# Sprint Log

Progress board and recorded actuals. Updated as the **last action of every card**.

*Last updated: 2026-08-08 · after Sprint 3*

---

## Status at a glance

```
Points   ███░░░░░░░░░░░░░░░░░░░░░░░░░░░   15 / 145   (10%)
Sprints  ███░░░░░░░░░░░░░░░░░░░░░░░░░░░    3 / 31
Cards    ███░░░░░░░░░░░░░░░░░░░░░░░░░░░    7 / 58
```

| | |
|---|---|
| **Phase** | 1 of 8 — Foundation ✅ complete |
| **Last completed** | Sprint 3 — *Guardrails up, frontend talking to the API* |
| **Up next** | **Sprint 4** — `A-1` Identity module (3) · `A-2` JWT + policies (2) |
| **Next milestone** | **M1 Hello, deployed** — Sprint 9, **11 Oct 2026** |
| **Schedule** | On plan. Not re-dated — see the re-baseline below. |
| **Tests** | 124 green (93 unit · 31 architecture) |
| **Build** | Clean, warnings-as-errors |

### Phases

| # | Phase | Sprints | Pts | Status |
|---|---|---|---:|---|
| 1 | Foundation | 1–3 | 15 | ✅ **Done** — `F-1`…`F-7` |
| 2 | Auth & Design System | 4–7 | 20 | ⬜ Next — `A-1`…`A-6`, `W-1`, `SP-1` |
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
| `F-5` | Architecture tests | 2 | `feat/f-5-architecture-tests` | ⬜ open |
| `F-7` | TanStack Start scaffold | 2 | `feat/f-7-web-scaffold` | ⬜ open |
| `F-6` | CI workflow | 1 | `feat/f-6-ci` | ⬜ open |

> Branches are **stacked** — merge in the order listed above, or the diffs will show unrelated commits.
> `F-6` adds the CI workflow, so its own PR is the first run of that workflow.

### Open risks

| Risk | State |
|---|---|
| **R1** BFF session/refresh pattern | 🟡 Partly retired — `F-7` proved the server-side API call works. The cookie and refresh loop is still Sprint 5. |
| **R2** YouTube IFrame progress tracking | 🟡 Spike `SP-1` scheduled Sprint 7, four months before the real card |
| **R3** Velocity below 5 pts/week | 🔴 **Unmeasured.** Sprints 1–3 ran in single sittings, not at real cadence. See re-baseline. |
| **R4** Life happens | ⬜ 4 weeks of slack built in (2 holiday + 2 buffer) |
| **R5** Scope creep | 🟢 Held — one deliberate pull-forward (`F-4` outbox table), recorded and offset against `P-7` |
| **R8** Design rabbit hole | 🟢 Not yet applicable — no design work before `W-1` in Sprint 6 |

---

## Velocity

| Sprint | Dates | Planned | Completed | Velocity | Notes |
|---|---|---:|---:|---:|---|
| 1 | Aug 10–16 | 5 | 5 | 5.0 | ✅ Both cards done. Started early (Aug 8) in one sitting, so this is **not** a valid velocity sample — see caveat. |
| 2 | Aug 17–23 | 5 | 5 | 5.0 | ✅ `F-3` + `F-4`. Also completed early, same sitting. Estimates held on both. |
| 3 | Aug 24–30 | 5 | 5 | 5.0 | ✅ `F-5` + `F-7` + `F-6`. Cards reordered within the sprint. |
| 4 | Aug 31–Sep 6 | 5 | — | — | ⬜ `A-1` Identity module (3) · `A-2` JWT + policies (2) |

> 1 point ≈ 2 focused hours. Record **actual** points as hours ÷ 2, honestly — an inflated
> actual hides a velocity problem until it is expensive to discover.

**Rolling average:** 5.0 pts/sprint over 3 sprints — **read the re-baseline below before trusting it.**
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
