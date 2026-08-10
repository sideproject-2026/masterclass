# Sprint Log

Progress board and recorded actuals. Updated as the **last action of every card**.

*Last updated: 2026-08-10 · after Sprint 8, merged as PR #13*

---

## Status at a glance

```
Points   ████████░░░░░░░░░░░░░░░░░░░░░░   40 / 145   (28%)
Sprints  ████████░░░░░░░░░░░░░░░░░░░░░░    8 / 31
Cards    ████████░░░░░░░░░░░░░░░░░░░░░░   16 / 58
```

| | |
|---|---|
| **Phase** | 4 of 8 — Instructor Studio · started early (Phase 3 suspended) |
| **Last completed** | Sprint 8 — *Courses exist, and the app ships in a container* — **4 of 5 points**, `D-2` partial |
| **Up next** | **Sprint 9** — `D-0` sign-off · `S-2` Course CRUD (2) · **plus the `D-2` remainder** |
| **Next milestone** | **M1 Hello, containerised** — Sprint 9, **11 Oct 2026** · ⚠️ **at risk** — see `R10` |
| **Schedule** | **Phase 3 suspended** (revision 4). Dates not re-cut; reconciled at the end-of-Sprint-9 re-baseline. |
| **Tests** | **321 green** (201 unit · 35 architecture · 39 integration · 46 web) — all four suites re-run this sprint |
| **Build** | Clean, warnings-as-errors |
| **Open branches** | **None.** Sprint 8 merged as PR #13. |
| **Carried work** | **`D-2` remainder — 1 pt.** The .NET images do not build; the SDK pin has no public image. Named in `R10`, carried into Sprint 9, **not** silently dropped. |

### Phases

| # | Phase | Sprints | Pts | Status |
|---|---|---|---:|---|
| 1 | Foundation | 1–3 | 15 | ✅ **Done** — `F-1`…`F-7` |
| 2 | Auth & Design System | 4–7 | 20 | ✅ **Done** — `A-1`…`A-6`, `W-1`, `SP-1` |
| 3 | Deploy | — | 9 | ⏸️ **Suspended** — `D-2` moved to Sprint 8; new `D-0` hosting spike in Sprint 9; `D-1`/`D-3` unscheduled pending it |
| 4 | Instructor Studio | 8–15 | 35 | 🔵 **Next** — `S-1`…`S-12`, `W-2` → **M2** |
| 5 | Catalog & Enrollment | 17–19, 22 | 20 | ⬜ `C-1`…`C-9`, `W-3` → **M3** |
| 6 | Player & Completion | 23–26 | 21 | ⬜ `P-1`…`P-8` → **M4** |
| 7 | Design pass | 27–28 | 10 | ⬜ `W-4`, `W-5`, `C-8` |
| 8 | Hardening & Launch | 29–31 | 15 | ⬜ `H-1`…`H-7` → **M5** |

*Sprints 20–21 (21 Dec – 3 Jan) are planned at zero. Sprints 32–33 are buffer.*

### Milestones

| | Milestone | Sprint | Date | Status |
|---|---|---|---|---|
| **M1** | Hello, **containerised** *(was "deployed")* | 9 | 11 Oct 2026 | ⬜ |
| **M1b** | Hello, deployed | TBD | **TBD — after `D-0`** | ⬜ |
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
| `A-5` | Route guards + login/register | 3 | `feat/a-5-route-guards` | ✅ PR #12 |
| `SP-1` | Spike: YouTube IFrame API | 2 | `feat/a-5-route-guards` | ✅ PR #12 |
| `S-1` | Catalog domain + invariants + migration | 3 | `feat/s-1-catalog-domain` | ✅ PR #13 |
| `D-2` | Dockerfiles for api + web | 1 of 2 | `feat/s-1-catalog-domain` | 🟡 PR #13 — **partial, 1 pt carried** |

**The last card merge on `main` is PR #13 (`4c3cb45`)** — named by PR rather than by tip SHA, which
a docs commit invalidates. PRs #1–#8 and #11–#13 merged and every branch deleted. **Every card
entry below is on `main` and there is no open branch** — 16 complete, plus `D-2` merged partial
with 1 point carried. Sprint 9 branches fresh off it.

### Open risks

| Risk | State |
|---|---|
| **R1** BFF session/refresh pattern | 🟢 **Retired.** Sealing, reading, tamper resistance, transparent refresh and now sign-out are all proven in a browser. The pattern did not fight us; a 204-handling bug in our own HTTP wrapper did. |
| **R2** YouTube IFrame progress tracking | 🟢 **Retired by `SP-1`.** Nothing in the API fought us; the manual-mark-complete fallback is not needed and `P-5` stays at 4 points. Three residuals are named rather than closed — mobile Safari, background-tab timer throttling, autoplay — and carried into `P-5`. |
| **R3** Velocity below 5 pts/week | 🔴 **Still unmeasured after seven sprints.** All seven ran as single sittings. Sprint 7 is a clean 5/5 at estimate, which is more evidence about *estimates* than about pace. Re-baseline checkpoint is end of Sprint 9. |
| **R4** Life happens | ⬜ 4 weeks of slack built in (2 holiday + 2 buffer) |
| **R9** First deploy deferred *(new, 2026-08-10)* | 🟡 **Mitigating.** `D-0` research pulled forward and done the same day — [`10-adr-hosting.md`](../design/10-adr-hosting.md) recommends Render + Cloudflare R2, **pending sign-off**. Goes 🟢 when the ADR is accepted and `D-1`/`D-3` have real estimates; goes back 🔴 if it sits unsigned past Sprint 9. Original entry:  Hosting reopened; Phase 3 suspended pending `D-0`. The plan deployed early on purpose — to hit managed-identity, connection-string and CORS problems now rather than alongside the player in February. That risk is now live and **grows with every sprint**: deploy at Sprint 16 and the first attempt must get object storage, pre-signed upload, the migration job and two apps right at once, instead of two nearly-empty apps. Mitigation: `D-0` is scheduled Sprint 9, not parked. |
| **R10** SDK pin has no public image *(new, 2026-08-10)* | 🔴 **Open — blocks `D-2`, `D-3` and M1.** `global.json` pins `10.0.400-preview.0.26322.102`. `mcr.microsoft.com/dotnet/sdk:10.0` ships **10.0.302** (band 3xx) and the nightly preview tag ships **10.0.100-rc.1** (band 1xx). `rollForward` only rolls *up* within a band, so neither satisfies it and `dotnet restore` fails in the image. Locally only `9.0.200` and the preview are installed. **Fix is a toolchain decision, not code** — see the `D-2` card. |
| **R5** Scope creep | 🟢 **Nothing carried** from Sprints 6–7; **`D-2` carries 1 pt** out of Sprint 8, named and counted. `SP-1`'s code was thrown away as the card required — only the notes are committed. |
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
| 7 | Sep 21–27 | 5 | 5 | 5.0 | ✅ `A-5` (3) · `SP-1` (2). Both at estimate. **Phase 2 closes on plan**, nothing carried. Still a single sitting. |
| 8 | Sep 28–Oct 4 | 5 | 4 | 4.0 | 🟡 `S-1` (3) at estimate · `D-2` **1 of 2** — the web image ships and runs; the .NET images cannot build because `global.json` pins an SDK with no public image. **1 point carried.** |

> 1 point ≈ 2 focused hours. Record **actual** points as hours ÷ 2, honestly — an inflated
> actual hides a velocity problem until it is expensive to discover.

> **Test-count note, Sprint 7.** The dashboard read **214** while its own breakdown summed to
> 215; the total was wrong, not the parts. Corrected to **253** = 133 unit + 35 architecture +
> 39 integration + 46 web.
>
> Unit (133), architecture (35) and web (46) were **re-run and verified this sprint**. The
> **39 integration tests were not re-run**: they require rebuilding `Lms.Api`, and a running
> AppHost holds a file lock on its DLLs, so `dotnet test` fails to build the solution. Sprint 7
> touched no backend code, so the figure carries forward from `A-6`'s verified run — but it is
> carried, not re-measured, and that is worth knowing before trusting it.

**Rolling average:** **5.0 pts/sprint** over 8 sprints (40 delivered of 40 planned).

**Sprint 8 is the first sprint to miss.** Four of five, and the miss is not an estimate problem —
`S-1` landed exactly on 3. It is the first card that depended on something outside the codebase
(a public SDK image) and found it missing. Worth noting because `R3` has always been about
whether the *cadence* holds; this is a different failure mode and it should not be read as
velocity decay.

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
| **PR** | ✅ #4 merged |
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
| **PR** | ✅ #5 merged |
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
| **PR** | ✅ #6 merged — first run of the workflow |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] Two parallel jobs, on push to `main` and on PRs
- [x] Backend: `global.json` SDK, `dotnet tool restore`, Release build, all 124 tests
- [x] Frontend: `npm ci`, lint, build
- [x] NuGet and npm caches; superseded runs cancelled
- [x] **Every step run locally in Release first**

**Decisions** — `npm ci` over `install` so a stale lock file fails the build; Release build inherits `TreatWarningsAsErrors`, which is also what makes NU1903 advisories fail; architecture tests run in CI, so boundaries are enforced mechanically rather than by review.

~~**Not yet verified** — the workflow has not run on GitHub.~~ **Resolved.** It has run on every PR since #6; twelve PRs have now gone through it.

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

## Sprint 7 — Sep 21–27, 2026

**Goal:** *Roles gate the UI and the API.*
**Demo:** Register → sign in on a styled page → DevTools shows the session cookie is `HttpOnly`
and no token is reachable from JS. Admin grants Instructor; the Studio link appears.

### `A-5` Route guards, login and register pages

| | |
|---|---|
| **Estimate** | 3 pts · **Actual** 3 pts |
| **Area** | `web` |
| **Branch** | `feat/a-5-route-guards` |
| **PR** | ✅ #12 merged |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] `_authed.tsx` — signed-out visitor redirected to `/login?redirect=<where they were going>`
- [x] `_instructor.tsx` — signed-in non-instructor gets a **403 page**, not a redirect loop
- [x] Styled login and register pages replacing `A-3`'s scaffolding form
- [x] Header shows Sign in / Register signed out, and the Studio link for instructors
- [x] Zod at the boundary — forms, search params, and the `/api/me` response
- [x] 375px, light **and** dark, keyboard reachable, no console warnings from our code
- [x] **46 web tests** (was 8); lint, typecheck and build clean

**Verified in a browser against the AppHost**
```
register empty form   3 field errors, no request sent
register valid        201 + auto sign-in, landed on /
document.cookie       empty while signed in; localStorage empty; no token in sessionStorage
/my-learning signed out  307 → /login?redirect=%2Fmy-learning
/studio signed out       307 → /login?redirect=%2Fstudio
/studio as student       403 page, URL unchanged, no loop
grant instructor      Studio link appears in header; /studio renders
wrong password        role=alert "Email or password is incorrect.", no cookie written
correct password      landed back on /studio — the redirect survived the round trip
signed in → /login?redirect=https://evil.example/steal   → landed on /, not evil.example
375px light + dark    app overflow 0px; error text oklch(0.704) on oklch(0.145)
```

**Shipped**
- `features/auth/` — `schemas.ts` (credentials, registration, `/api/me`, redirect), `access.ts`
  (`hasRole`, `canUseStudio`), `hooks.ts` (`useAuthForm`), four components
- `routes/` — `_authed.tsx`, `_instructor.tsx`, `login.tsx`, `register.tsx`, and two placeholder
  children (`_authed/my-learning.tsx`, `_instructor/studio/index.tsx`)
- `server/auth.ts` — a `register` server function; `/api/me` now parsed, not cast
- shadcn `input` and `label`; Zod added — the first card that needed it
- 38 new tests

**Decisions**
1. **The `redirect` param is an open-redirect defence, and it lives in the schema.** Only a
   same-origin absolute path is accepted; `//host`, `/\host` and control characters are rejected
   because they read as paths and resolve as absolute URLs. Putting it in
   `redirectSearchSchema` rather than at each call site means no future caller can forget it. A
   hostile value falls back to `/` via `.catch` rather than raising — clamp, don't reject.
2. **`.optional()`, not `.default('/')`.** A default made the router *materialise* it: every
   visit to `/login` answered a 307 to `/login?redirect=%2F` before rendering. Found by curling
   the route, not by looking at it. `destinationFrom()` supplies the fallback at the read site.
3. **Sign-in is a document navigation, like sign-out.** `window.location.assign(destination)`
   rather than `router.navigate`. The identity behind every cached loader has just changed, and
   a full navigation rebuilds that rather than trusting an invalidation to catch all of it. It
   also reaches an arbitrary validated path without casting a string into the router's typed
   route union.
4. **A signed-in non-instructor gets 403, not a redirect.** Sending an authenticated student to
   a login form is a loop: they sign in successfully, return, and are bounced again with no
   explanation.
5. **The Studio link keys off the role, not `instructorSlug`.** See the deviation below.
6. **The sign-in form does not enforce the password policy.** Only non-empty. Enforcing the
   10-character minimum would publish the policy to anyone who opens the page and would lock
   out any account whose password predates a policy change. Length is the API's business and
   its answer is one indistinguishable 401.
7. **`/api/me` is parsed with Zod.** The guards branch on `roles`; a payload whose shape drifted
   would otherwise arrive as `undefined` and silently decide access. A parse failure means
   signed out, which fails closed.
8. **Placeholder pages under both guards.** A guard with nothing behind it is a guard nobody has
   watched work — the `A-3` scaffolding-form argument, which is what found the sign-out defect.
   `C-7` and `S-1` delete the bodies.

**Deviations from the design docs**
- **`03 §3` said `instructorSlug` non-null is how the web app decides to show the Studio link.**
  That stopped being true in `A-6`, which deliberately keeps the profile and slug on revoke so
  course pages still name the author and nobody inherits a public URL. A revoked instructor
  therefore still has a slug, and keying on it would offer them a Studio that answers 403.
  **Doc corrected**; `canUseStudio` keys on the `Instructor` role, with a regression test.
- **Registration signs the user in automatically.** The plan's demo line reads "Register → log
  in", which suggests two steps. The credentials are already in hand server-side, and making
  someone who just proved them type them again is friction with nothing behind it. If the
  registration succeeds and the follow-up sign-in does not, the card reports success — the
  account exists, and implying otherwise invites a duplicate attempt.

**Known consequence, not a defect**
A newly granted instructor sees the Studio immediately, because `/api/me` reads roles from the
database — but the access token in their session cookie still carries the old role claims for up
to 15 minutes, so `/api/studio/*` would answer 403 until it refreshes. Self-healing and bounded
by design (`04 §3.1`: the token lifetime *is* the revocation window). Named here so `S-1` treats
a 403 from Studio as a real state rather than a bug.

**Pre-existing gap, left for `W-4`**
Every page load logs *"a notFoundError was encountered on the route with ID `__root__`, but a
notFoundComponent option was not configured"*. Cause: there is no favicon, so `/favicon.ico`
404s on each load. A real 404 currently renders TanStack's bare `<p>Not Found</p>` outside the
app shell. Not introduced here — `notFoundComponent` has never been configured — and error
states are `W-4` (Sprint 27). Not absorbed into this card.

---

### `SP-1` Spike: YouTube IFrame API

| | |
|---|---|
| **Estimate** | 2 pts · **Actual** 2 pts |
| **Area** | `web` |
| **Branch** | `feat/a-5-route-guards` |
| **PR** | ✅ #12 merged |
| **Status** | ✅ Done — **risk R2 retired** |

Full writeup: [`artifacts/spikes/sp-1-youtube-iframe-api.md`](../spikes/sp-1-youtube-iframe-api.md).

**Timeboxed and thrown away as the card required.** The harness was a standalone Node server and
one HTML page in a scratch directory outside the repo, so there is nothing to delete from `web/`
and nothing that can rot into production. Only the notes are committed.

**What it settled**
- `getCurrentTime()` is accurate — 0.99–1.03s per second over 30+ ticks, no drift. The ADR's
  15-second interval is comfortably safe.
- `getDuration()` is available at `onReady`, before playback (634.6s), so the 90% threshold does
  not depend on `Lesson.DurationSeconds`.
- `sendBeacon` delivers on unload with correct final values.

**Five things the design docs had wrong or unsaid**
1. The nocookie embed needs the player's **`host` option**. Building the iframe yourself and
   attaching `YT.Player` to it silently reverts to `www.youtube.com` — the property is lost with
   no error.
2. `sendBeacon(url, string)` sends **`text/plain`**, which a JSON-bound minimal API answers with
   415 — invisibly, because `sendBeacon` returns `true` for *queued*, never *accepted*. Wrap the
   payload in a `Blob` with an explicit type.
3. **One departure fires three events** — `beforeunload`, `pagehide` and `visibilitychange`
   within 15ms, three identical writes. Send on `pagehide` only. `visibilitychange` alone
   produced 26 beacons before playback had even started.
4. **`onReady` does not mean playable.** A video with embedding disabled fires `onReady` and
   *then* `onError` (code 150). Error states must be driven by `onError`.
5. **CSP `script-src` needs `https://www.youtube.com`**, not just `frame-src` for nocookie — the
   API script is served from the main domain.

**The scrub hole is cheaper to close than `05 §2.4` assumed.** Ten lines of forward-delta
clamping credited **nothing** for a 495-second seek (position 539s, watched 52s). `P-5` should
ship it. **It is not a security control** and the writeup says so plainly — the client is
untrusted and can post any `watchedSeconds` it likes. The ADR's acceptance of the hole stands;
only the honest default changes.

**Not tested, and named as such** — mobile Safari (no device; this is where the original worry
lived), background-tab timer throttling (cited, not measured), autoplay without a prior gesture.
Carried into `P-5` as residual risk rather than counted as closed.

**Docs updated:** `05 §2.3`, `05 §2.4`, `06 §2.2`.

---

## Sprint 8 — Sep 28 – Oct 4, 2026

**Goal:** *Courses exist, and the app ships in a container.*

First sprint of Phase 4, pulled forward because Phase 3 is suspended (`08` revision 4).

### `S-1` Catalog domain, invariants and migration

| | |
|---|---|
| **Estimate** | 3 pts · **Actual** 3 pts |
| **Area** | `api` |
| **Branch** | `feat/s-1-catalog-domain` |
| **PR** | ✅ #13 merged |
| **Status** | ✅ Done |

**Acceptance criteria**
- [x] `Course`, `Chapter`, `Lesson` per [02 §3](../design/02-domain-model.md), factory methods returning `Result`
- [x] Publish invariants (§3.2) — all six, reported **as a list**
- [x] Lesson content invariant (§3.3) — switching type clears the other side
- [x] Dense 0-based `SortOrder`, whole-list reorder (§3.4)
- [x] `Tags` as `text[]` with a GIN index (§3.5)
- [x] `InitialCatalog` migration, registered in `Lms.MigrationService`
- [x] **201 unit tests** (was 133); architecture tests still green

**Verified against real PostgreSQL**
```
catalog.__ef_migrations_history_catalog  20260810100240_InitialCatalog
catalog.courses   tags text[] not null · no xmin column (system column, as intended)
  ix_courses_slug                UNIQUE btree (slug)
  ix_courses_status_published_at btree (status, published_at)
  ix_courses_tags                gin (tags)
  ix_courses_instructor_id       btree (instructor_id)
catalog.chapters  ix_chapters_course_id_sort_order  UNIQUE (course_id, sort_order)
catalog.lessons   ix_lessons_chapter_id_sort_order  UNIQUE (chapter_id, sort_order)
fk_chapters_courses_course_id  ON DELETE CASCADE
```

**A real bug the tests caught — and it would have shipped**

`Slugify` used the textbook accent-stripper: `Normalize(FormD)`, drop the `NonSpacingMark`s,
recompose. It produced `r-activit-avanc-e` from `Réactivité Avancée`.

**`Directory.Build.props` sets `InvariantGlobalization=true`, which makes `string.Normalize()`
a silent no-op** — it does not throw, it returns the input unchanged. So the decomposition never
happens, no combining marks are ever found, and the accented characters fall through to the
"replace anything non-alphanumeric with a hyphen" step.

The failure mode is what makes it worth recording: the code compiles, passes every ASCII test,
and mangles the first accented course title anyone writes — into a *public URL*, which is then
frozen on publish. Replaced with an explicit fold table (plus `ß`→`ss`, `æ`→`ae`, `œ`→`oe`),
with a regression test named after the trap. **Noted in `src/CLAUDE.md`** so the next person
reaching for `Normalize` finds out before shipping.

**Decisions**
1. **`PublishViolations()` returns a list; `Publish()` summarises it.** §3.2 requires the 422 to
   name every problem — telling an author about one missing thing at a time turns publishing
   into a guessing game. `Result` carries one `Error`, and a multi-error `Result` is not being
   invented for a single caller (rule of two). `S-6` reads the list method directly.
2. **"No required lesson" is only reported when lessons exist.** On an empty course it is noise
   stacked on the errors that already say so.
3. **Re-publishing keeps the original `PublishedAt`.** A brief unpublish to fix a typo must not
   send a year-old course back to the top of whatever sorts on it.
4. **Retitling does not regenerate the slug.** Silently moving a URL because someone fixed a
   typo is surprising even on a draft.
5. **The unique index on `slug` is the arbiter, not a pre-check** — a query-then-insert still
   loses to a concurrent create. Same call as `A-6`'s instructor slug.
6. **A title that slugifies to nothing is refused** rather than given a generated fallback.
7. **The tag cap is applied after de-duplication**, so `["dotnet","DotNet"]` counts as one.
8. **`EstimatedMinutes` rounds up.** A 90-second lesson reading as "0 minutes" is worse than "1".
9. **`EnrollmentCount` clamps at zero.** It is denormalised from another module's events and so
   eventually consistent; a duplicate unenrol must not produce −1.

**Deviations from the design docs**
- **`LessonAttachment` was not built.** [02 §3](../design/02-domain-model.md) lists it in the
  Catalog schema, but the card is "Course/Chapter/Lesson", and attachments are `S-8` (Sprint 12).
  Adding a table later is a migration, and the migration pipeline is already verifiable without
  it — unlike `F-4`'s outbox, which was pulled forward precisely because nothing else proved it.

---

### `D-2` Dockerfiles for api + web — 🟡 **partial**

| | |
|---|---|
| **Estimate** | 2 pts · **Actual 1 pt · 1 carried** |
| **Area** | `infra` |
| **Branch** | `feat/s-1-catalog-domain` |
| **PR** | ✅ #13 merged |
| **Status** | 🟡 **Partial — the web image ships; the .NET images cannot be built** |

**Done**
- [x] `web/Dockerfile` — multi-stage, `npm ci`, dev dependencies dropped from the runtime layer
- [x] **Built, run, and exercised**: `/`, `/login`, `/register` all 200, and `/my-learning` 307s
      to `/login?redirect=%2Fmy-learning` **from inside the container** — `A-5`'s guard working
      against a real image
- [x] Runs as non-root (`uid=1000(node)`); 390 MB
- [x] `.dockerignore` — keeps host `bin/`, `obj/`, `node_modules/` and `dist/` out, so a host
      build for another RID cannot shadow the one the image just produced
- [x] `web/server.mjs` production entry — see below

**Not done, and why**
- [ ] `src/Lms.Api/Dockerfile` and `src/Lms.MigrationService/Dockerfile` are **written but do
      not build.** `dotnet restore` fails with exit 155 before it reaches any C#.

**The blocker — `R10`**

`global.json` pins `10.0.400-preview.0.26322.102`.

| Image | Ships |
|---|---|
| `mcr.microsoft.com/dotnet/sdk:10.0` | **10.0.302** — feature band 3xx |
| `mcr.microsoft.com/dotnet/nightly/sdk:10.0-preview` | **10.0.100-rc.1** — band 1xx |

`rollForward: latestPatch` only rolls **up within a band**, so neither satisfies a 4xx pin, and
there is no public image that does. Locally, only `9.0.200` and the pinned preview are installed.
**This is a toolchain decision, not a code fix**, so it was not made unilaterally — options are
in the handover note below. It equally blocks `D-3` and therefore M1.

**A second silent failure, found by running the image rather than building it**

The first web image built cleanly, started, and **exited 0 within a second, logging nothing.**
`vite build` emits a *web-standard fetch handler* at `dist/server/server.js` — it exports
`{ fetch(request) }` and has no listener — so `node dist/server/server.js` evaluates the module
and ends. In a container that is indistinguishable from a healthy start followed by a clean
shutdown, and on a host with restart-on-exit it would have looked like a crash loop with an
empty log.

Fixed with `web/server.mjs`: `srvx` (already in the tree, and what the TanStack Start hosting
guide names for this) adapts the fetch handler to Node, with `serveStatic` in front for
`dist/client`. Deliberately **not** a Nitro/Vercel/Netlify preset — a preset bakes a deployment
target into the build, and `D-0` has not chosen one. A plain Node process on `$PORT` is what
every candidate in [10-adr-hosting.md](../design/10-adr-hosting.md) consumes.

**Decisions**
1. **The whole source tree is copied before restore, not a list of fifteen `.csproj` paths.**
   The list is a maintenance burden that breaks silently the first time a project is added, and
   the cache win on a repo this size does not pay for it.
2. **`npm ci`, never `install`** — a drifted lock file must fail the build, same rule as `F-6`.
3. **A `Dockerfile` for `Lms.MigrationService` too**, though the card says "api + web". It is
   five lines different from the API's, and `D-3`'s migration job — the requirement that
   disqualifies a host if it cannot gate a rollout — has no image without it. Small, deliberate
   pull-forward.
4. **`0.0.0.0`, not localhost.** Binding loopback inside a container looks perfectly healthy
   from within and is unreachable from outside.
5. **Both runtime images run as a non-root user**, and application files stay root-owned, so the
   process cannot rewrite its own code.

**Not verified** — the API and migration images, obviously. Also no health-probe wiring or image
scanning; both belong with `D-3` and the chosen host.

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
| 2026-08-10 | `A-5` | The `?redirect=` open-redirect defence lives in the schema, so no call site can forget it; `//host`, `/\host` and control chars all rejected |
| 2026-08-10 | `A-5` | `.optional()` not `.default('/')` — a default made the router 307 `/login` to `/login?redirect=%2F` on every visit |
| 2026-08-10 | `A-5` | Sign-in navigates the document, like sign-out: identity changed, so rebuild client state rather than invalidate it |
| 2026-08-10 | `A-5` | A signed-in non-instructor gets a 403 page; redirecting them to login is a loop they cannot escape |
| 2026-08-10 | `A-5` | The sign-in form checks non-empty only — enforcing the password policy would publish it and lock out pre-policy accounts |
| 2026-08-10 | `A-5` | `/api/me` is parsed, not cast; a drifted shape must fail closed rather than reach a guard as `undefined` |
| 2026-08-10 | `A-5` | **Studio link keys on the `Instructor` role, not `instructorSlug`** — `A-6` keeps the slug on revoke, so the slug would offer a 403 |
| 2026-08-10 | `A-5` | A freshly granted instructor sees Studio before their token carries the role — bounded by the 15-minute lifetime, and `S-1` must expect the 403 |
| 2026-08-10 | `SP-1` | Nocookie embeds need `YT.Player`'s `host` option; hand-building the iframe silently reverts to `www.youtube.com` |
| 2026-08-10 | `SP-1` | `sendBeacon` needs a `Blob` with an explicit type, or it sends `text/plain` and a JSON endpoint 415s it invisibly |
| 2026-08-10 | `SP-1` | Send progress on `pagehide` only — one departure fires three events, and `visibilitychange` fires constantly |
| 2026-08-10 | `SP-1` | `onReady` fires before `onError`, so it is not a signal that the video is playable |
| 2026-08-10 | `SP-1` | A ten-line forward-delta clamp defeats the casual scrub — worth shipping, but it is **not** a security control |
| 2026-08-10 | `S-1` | **`InvariantGlobalization=true` makes `string.Normalize` a silent no-op** — the textbook accent-stripper mangles accented titles into public URLs |
| 2026-08-10 | `S-1` | `PublishViolations()` returns the full list; `Publish()` summarises it. No multi-error `Result` invented for one caller |
| 2026-08-10 | `S-1` | Re-publishing keeps the original `PublishedAt`; a typo fix must not re-sort a year-old course as new |
| 2026-08-10 | `S-1` | Retitling never regenerates the slug — silently moving a URL is surprising even on a draft |
| 2026-08-10 | `S-1` | The tag cap is applied after de-duplication, so case variants count once |
| 2026-08-10 | `S-1` | `EnrollmentCount` clamps at zero — it is eventually consistent from another module's events |
| 2026-08-10 | `D-2` | **The built web output is a fetch handler, not a server.** `node dist/server/server.js` exits 0 silently; `web/server.mjs` + `srvx` is the missing listener |
| 2026-08-10 | `D-2` | No Nitro/Vercel/Netlify preset — a preset bakes a deploy target into the build before `D-0` has chosen one |
| 2026-08-10 | `D-2` | Copy the whole source tree before restore; a list of `.csproj` paths breaks silently when a project is added |
| 2026-08-10 | `D-2` | **The pinned preview SDK exists in no public image** — `R10`, and it blocks `D-3` and M1 |
