# Sprint Log

Recorded actuals per card. Updated as the **last action of every card**, before the PR is reported.

This exists for one reason: [08 §2.3](../design/08-delivery-plan.md) re-baselines the whole 33-sprint schedule on measured velocity after Sprint 3. The 5 pts/week figure is a hypothesis until this table disagrees with it.

> 1 point ≈ 2 focused hours. Record **actual** points as hours ÷ 2, honestly — an inflated actual hides a velocity problem until it is expensive.

---

## Velocity

| Sprint | Dates | Planned | Completed | Velocity | Notes |
|---|---|---:|---:|---:|---|
| 1 | Aug 10–16 | 5 | 5 | 5.0 | ✅ Both cards done. Started early (Aug 8) in one sitting, so this is **not** a valid velocity sample — see caveat. |

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
