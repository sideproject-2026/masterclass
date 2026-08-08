# Sprint Log

Recorded actuals per card. Updated as the **last action of every card**, before the PR is reported.

This exists for one reason: [08 §2.3](../design/08-delivery-plan.md) re-baselines the whole 33-sprint schedule on measured velocity after Sprint 3. The 5 pts/week figure is a hypothesis until this table disagrees with it.

> 1 point ≈ 2 focused hours. Record **actual** points as hours ÷ 2, honestly — an inflated actual hides a velocity problem until it is expensive.

---

## Velocity

| Sprint | Dates | Planned | Completed | Velocity | Notes |
|---|---|---:|---:|---:|---|
| 1 | Aug 10–16 | 5 | — | — | In progress |

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
| **Actual** | — |
| **Area** | `api` |
| **Branch** | `feat/f-1-solution-skeleton` |
| **PR** | — |
| **Started / Finished** | — |
| **Status** | Not started |

**Acceptance criteria**
- [ ] `dotnet build` clean with `TreatWarningsAsErrors`
- [ ] `dotnet test` green
- [ ] Solution contains all projects from [01 §3](../design/01-architecture.md)
- [ ] `*.Contracts` projects have zero project references
- [ ] SharedKernel covers Result, pagination, messaging contracts, events, clock, typed IDs

**Shipped**
—

**Decisions**
—

**Deviations from the design docs**
—

---

### `F-2` Aspire AppHost with persistent data

| | |
|---|---|
| **Estimate** | 2 pts |
| **Actual** | — |
| **Area** | `infra` |
| **Branch** | `feat/f-2-aspire-apphost` |
| **PR** | — |
| **Started / Finished** | — |
| **Status** | Not started |

**Acceptance criteria**
- [ ] Dashboard lists postgres, pgweb, azurite, api — all healthy
- [ ] Blob containers `course-assets` and `lesson-attachments` exist
- [ ] **Insert a row, stop the AppHost completely, restart — the row is still there**
- [ ] `scripts/reset-local-data.ps1` documented and working

**Shipped**
—

**Decisions**
—

**Deviations from the design docs**
—

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
| — | — | — |
