# Masterclass — Learning Management System

An engineering-focused learning platform. Instructors author courses; students browse, enrol, and work through them.

**Modular monolith** — .NET 10 minimal APIs + TanStack Start, PostgreSQL, Azure. Local development runs on Aspire.

---

## Getting started

```bash
dotnet run --project src/Lms.AppHost
```

Starts the API, PostgreSQL, and Azurite with the Aspire dashboard. Requires Docker. Data persists across restarts — to reset it, run `scripts/reset-local-data.ps1`.

```bash
dotnet test
```

---

## Documentation

The design is written down before it is built. Start with the overview.

| Doc | Covers |
|---|---|
| [00 — Overview](artifacts/design/00-overview.md) | Scope, requirements, non-goals, vocabulary |
| [01 — Architecture](artifacts/design/01-architecture.md) | Modular monolith, module boundaries, Azure topology |
| [02 — Domain model](artifacts/design/02-domain-model.md) | Entities, invariants, persistence |
| [03 — API design](artifacts/design/03-api-design.md) | Every endpoint and its contract |
| [04 — ADR: Authentication](artifacts/design/04-adr-authentication.md) | Why not Keycloak or Duende |
| [05 — ADR: Video & storage](artifacts/design/05-adr-video-and-storage.md) | YouTube, Azure Blob, and the trade-offs |
| [06 — Tech stack](artifacts/design/06-tech-stack.md) | Libraries, and why each is there |
| [07 — Roadmap](artifacts/design/07-roadmap.md) | Build order |
| [08 — Delivery plan](artifacts/design/08-delivery-plan.md) | Sprints, milestones, risks |
| [09 — Code conventions](artifacts/design/09-code-conventions.md) | Patterns, the in-house mediator, naming |

Progress is tracked in [artifacts/tracker/sprint-log.md](artifacts/tracker/sprint-log.md).

## Contributing

One card per branch (`feat/<card-id>-<slug>`), one pull request per card. Conventions are enforced by `Lms.ArchitectureTests` and the checklists in [CLAUDE.md](CLAUDE.md).
