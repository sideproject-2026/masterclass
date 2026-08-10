# 06 — Technology Stack

> The brief states a preference for **.NET 10 and TanStack Start**. Both are good fits and both are adopted.
> Every entry below carries one line of justification and a named simpler/cheaper alternative, so nothing here is load-bearing by accident.

---

## 1. Backend — .NET 10

| Concern | Choice | Why | Alternative |
|---|---|---|---|
| **Runtime** | .NET 10 (LTS), C# 14 | Stated preference, and the right one — LTS support window, and the modular-monolith story in ASP.NET Core is mature. | .NET 8 if a hosting constraint forces it. |
| **HTTP** | ASP.NET Core **minimal APIs** with route groups | Route groups map cleanly onto the audience-based grouping in [`03`](03-api-design.md) — one `.RequireAuthorization()` per group instead of an attribute on every action. Less ceremony than MVC controllers for a JSON API. | MVC controllers, if the team strongly prefers them. Nothing in this design depends on the choice. |
| **Data** | **EF Core 10**, one `DbContext` per Module | Migrations, LINQ, and change tracking with no ORM plumbing to own. Per-Module contexts are what make the schema boundaries real ([`02 §8`](02-domain-model.md#8-persistence-strategy)). | Dapper for a specific hot query later — mixing is fine and expected. |
| **Database** | **Azure Database for PostgreSQL Flexible Server** + Npgsql EF Core provider | Built-in full-text search (`tsvector` + GIN) keeps Azure AI Search out of the architecture; `text[]` gives real indexed tag queries; cheapest managed relational tier on Azure; portable off Azure entirely. Full reasoning in [`01 §7.2`](01-architecture.md#72-why-postgresql-over-azure-sql). | **Azure SQL Database** (GP Serverless) — equally capable, more familiar to most .NET teams, and `rowversion` is slightly more idiomatic than `xmin`. Costs more and puts a search service back on the roadmap. |
| **Identity** | ASP.NET Core Identity + JWT bearer | See [`04`](04-adr-authentication.md). | OpenIddict when real OIDC is needed. |
| **API docs** | Built-in `AddOpenApi()` / `MapOpenApi()` + **Scalar** for the viewer | .NET 10 generates the OpenAPI document in-box. **No Swashbuckle dependency** — one fewer package that historically lags each .NET release. `Microsoft.AspNetCore.OpenApi` deliberately ships no UI, so Scalar (free, OSS) reads the generated document at `/scalar/v1` in Development. | Swashbuckle, if you want the classic Swagger UI and accept the extra dependency. |
| **Validation** | **FluentValidation** + an endpoint filter | Complex rules (the lesson content invariant in [`02 §3.3`](02-domain-model.md#33-lesson-content-invariant)) are unreadable as data annotations, and validators are unit-testable in isolation. | Data annotations for the trivial cases; not worth two mechanisms. |
| **Blob access** | `Azure.Storage.Blobs` + `Azure.Identity` | Managed identity and user-delegation SAS ([`05 §5`](05-adr-video-and-storage.md#5-direct-to-blob-upload-flow)) — no storage keys in the app. | None; this is the standard path. |
| **Logging** | **Serilog** → console (structured JSON) | Structured logs enriched with `UserId`/`CourseId`/`TraceId`. Container Apps picks up stdout. | Built-in `ILogger` is adequate; Serilog's enrichers and sinks earn the dependency. |
| **Telemetry** | **OpenTelemetry** → Application Insights | One correlated trace across web → api → SQL. Vendor-neutral, so leaving Azure does not mean re-instrumenting. | App Insights SDK directly — smaller, but locks the instrumentation in. |
| **Background work** | `BackgroundService` (outbox drain, orphan-blob cleanup) | Two jobs, both short. A scheduler is not warranted. | Hangfire/Quartz when jobs need retries with a dashboard and durable scheduling. |
| **Email** | **Azure Communication Services Email**, behind `IEmailSender` | Same cloud, same identity model, same bill. | Resend or SendGrid — the interface makes it a one-class change. |
| **Testing** | xUnit · **Testcontainers** for PostgreSQL · `WebApplicationFactory` · **NetArchTest** | Integration tests against real Postgres — mandatory here, not just preferable: `tsvector`, `text[]`, and `xmin` have no equivalent in any in-memory or SQLite provider, so the queries that matter most would be untested. NetArchTest keeps the Module boundaries honest ([`01 §4.1`](01-architecture.md#41-enforcement)). Testcontainers rather than the AppHost: tests own a disposable database per run, and must not depend on — or pollute — the persistent dev volume. | None worth taking. Reuse one container across the test collection if startup time bites. |
| **Local orchestration** | **Aspire** — committed, not optional | One `dotnet run` brings up API, web, PostgreSQL, and Azurite with wired connection strings and a live dashboard. `ServiceDefaults` also supplies the OpenTelemetry, health-check, and resilience wiring listed below, so those rows are largely free. Full setup in §3.1. | `docker compose` — still viable, but you would hand-write the wiring Aspire generates. |

### 1.1 Backend packages, complete

```
Microsoft.AspNetCore.OpenApi
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.IdentityModel.JsonWebTokens
Npgsql.EntityFrameworkCore.PostgreSQL
Microsoft.EntityFrameworkCore.Design
EFCore.NamingConventions
Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
Aspire.Azure.Storage.Blobs
Scalar.AspNetCore
FluentValidation.AspNetCore
Azure.Storage.Blobs
Azure.Identity
Azure.Communication.Email
Serilog.AspNetCore
OpenTelemetry.Extensions.Hosting
Azure.Monitor.OpenTelemetry.AspNetCore
```

The two `Aspire.*` packages are the client integrations: they register `DbContext` and `BlobServiceClient` from the connection strings the AppHost injects, with health checks, retries, and telemetry already attached. `OpenTelemetry.Extensions.Hosting` moves into `Lms.ServiceDefaults` rather than being configured per project.

`EFCore.NamingConventions` gives snake_case tables and columns. EF Core 10 has no built-in convention, and unquoted lowercase identifiers are what make the database pleasant to query by hand — which matters, because pgweb is in the AppHost precisely so you can.

Version pinning is not only for packages: **`dotnet-ef` is pinned in `.config/dotnet-tools.json`** and must match the EF Core package major. `dotnet tool install --prerelease` will happily fetch an EF 11 preview against EF 10 packages.

Seventeen packages. If this list passes twenty-five, something has been added that was not needed.

---

## 2. Frontend — TanStack Start

| Concern | Choice | Why | Alternative |
|---|---|---|---|
| **Framework** | **TanStack Start** (React 19) | Stated preference, and it earns it: the server runtime doubles as the BFF that holds the session cookie ([`04 §3`](04-adr-authentication.md#3-decision)), and SSR matters for a public course catalog that needs to be indexable. | Next.js — larger ecosystem, but Start's type-safe routing is a real advantage for a route tree this shaped. |
| **Routing** | TanStack Router, file-based | Fully typed params and search params. `beforeLoad` on layout routes is the auth-guard mechanism. | — |
| **Server calls** | `createServerFn` | Runs on the server, so the access token never reaches browser JS. This is the whole BFF. | Route handlers; server functions are the idiomatic form. |
| **Data** | **TanStack Query** | Integrates with router loaders, dehydrates SSR state into the client cache, handles the progress-heartbeat mutations. | Router loaders alone — but you would rebuild caching and invalidation by hand. |
| **Styling** | **Tailwind CSS v4** | Fast to build in, no CSS architecture debate. | CSS Modules if the team dislikes utility classes. |
| **Components** | **shadcn/ui** | Copied into the repo, not a dependency — so it can be modified without fighting a library. Covers dialogs, tables, forms, toasts. | Mantine or Radix directly. |
| **Forms** | **TanStack Form** + **Zod** | Zod schemas are shared between the server-function validator and the client form — one definition, both sides. | React Hook Form; TanStack Form is the coherent choice inside this ecosystem. |
| **Markdown** | `react-markdown` + `remark-gfm` + **`rehype-sanitize`** | Renders Reading lessons and course descriptions. **Sanitization is mandatory** — see §2.2. | MDX if lessons ever need embedded components; heavier and a larger attack surface. |
| **Markdown editing** | A textarea with a live preview pane | Instructors are engineers. They write markdown. A WYSIWYG would be more work and worse. | TipTap if a non-technical instructor persona ever appears. |
| **Video player** | YouTube IFrame Player API, wrapped in one component | Needed for progress events ([`05 §2.4`](05-adr-video-and-storage.md#24-progress-tracking)). | `react-youtube`; the raw API is ~80 lines and one fewer dependency. |
| **Drag & drop** | `@dnd-kit/core` — Studio chapter/lesson reordering only | Accessible, keyboard-operable, actively maintained. | Up/down arrow buttons. Genuinely fine for MVP, and worth considering if the schedule is tight. |
| **Icons** | `lucide-react` | Pairs with shadcn/ui. | — |
| **Testing** | **Vitest** + Testing Library; **Playwright** for the two critical journeys | Playwright covers author→publish and enroll→complete end to end. Two specs, not a suite. | — |

### 2.1 Route map

```
routes/
  __root.tsx                          shell; loads current user into route context
  index.tsx                           landing page
  courses/
    index.tsx                         catalog — R2, SSR + searchable
    $slug.tsx                         course detail, preview player, enroll CTA
  instructors/$slug.tsx               instructor profile
  login.tsx                           ?redirect= support
  register.tsx                        R4

  _authed.tsx                         guard: authenticated
  _authed/
    my-learning.tsx                   R7
    learn/$slug.tsx                   player shell — curriculum sidebar
    learn/$slug/$lessonId.tsx         lesson content — R6, R8
    courses/$slug/complete.tsx        congratulations + suggestions — R5

  _instructor.tsx                     guard: role Instructor
  _instructor/
    studio/index.tsx                  my courses
    studio/courses/$id.tsx            course settings
    studio/courses/$id/curriculum.tsx chapter/lesson tree + reorder — R1
    studio/lessons/$id.tsx            lesson editor (video | reading) — R1
    studio/courses/$id/stats.tsx      the three numbers
```

Guards are UX; the API enforces the same rules ([`03 §7`](03-api-design.md#7-authorization-matrix)).

### 2.2 Frontend security notes

- **Sanitize all rendered markdown.** Lesson content and course descriptions are instructor-authored and rendered into other users' browsers. `rehype-sanitize` with a conservative allow-list, no raw HTML passthrough. Instructors are curated, not trusted — a compromised instructor account must not become stored XSS against every student.
- **The YouTube embed is an iframe with a server-validated 11-character id** ([`05 §2.2`](05-adr-video-and-storage.md#22-url-parsing)). Never interpolate an unvalidated string into an iframe `src`.
- **No access token in client code, ever.** If you find yourself passing a token to a component, the BFF boundary has been breached.
- **CSP** with `frame-src https://www.youtube-nocookie.com`, `img-src` limited to the CDN origin, and no `unsafe-inline` for scripts.
  `script-src` must also allow **`https://www.youtube.com`** — the IFrame API is served from `www.youtube.com/iframe_api` even when the frame itself is nocookie ([`SP-1`](../spikes/sp-1-youtube-iframe-api.md)). Easy to miss, and the symptom is a player that never initialises.

---

## 3. Infrastructure & delivery

> ⚠️ **Reopened 2026-08-10. Everything in this table marked *provisional* is a proposal, not a decision.**
> Azure is no longer settled; `D-0` ([`08 §Phase 3`](08-delivery-plan.md)) chooses the host and writes
> `10-adr-hosting.md`. Until then, **do not build against these rows** — they are kept as the leading
> candidate and as a record of what was previously reasoned, not as instructions.
>
> The three rows that are **not** provisional are requirements rather than products, and any candidate
> host must satisfy them.

| Concern | Choice | Why | Alternative |
|---|---|---|---|
| **Compute** | 🟡 *Provisional:* Azure Container Apps — `api` and `web` | Scale-to-zero on non-prod, revision-based rollouts, no cluster to run. | **Open.** Any host that runs two containers and can gate a rollout on a job. |
| **Migrations** | ✅ **Requirement:** a one-shot job running `Lms.MigrationService`, **gating the API deploy** | Startup migration races across replicas. This is not a style preference, and it is not negotiable by hosting choice. | — |
| **Secrets** | ✅ **Requirement:** secret storage the running app can reach without a key in the repo | Nothing in config files, nothing in the repo. | Key Vault, or the chosen host's equivalent. |
| **Object storage** | ✅ **Requirement:** direct browser upload via a short-lived pre-signed credential | [`05 §5`](05-adr-video-and-storage.md) — the API must never proxy the bytes. Azure calls it user-delegation SAS; S3-compatible stores call it a pre-signed URL. | Any S3-compatible store. |
| **CDN** | 🟡 *Provisional:* Azure Front Door | Fronts both apps and the public asset container; TLS and WAF included. | Cloudflare. |
| **IaC** | 🟡 *Provisional:* Bicep | Native to Azure, no state file to manage. **Bicep only makes sense if the answer is Azure** — this row follows `D-0`, it does not lead it. | Terraform, Pulumi, or the host's own CLI if the estate is small enough not to warrant IaC. |
| **CI/CD** | ✅ GitHub Actions | Build → test → arch-test → image push → migration job → deploy. Already running since `F-6`. | Unaffected by `D-0`. |
| **Local dev** | **Aspire AppHost** orchestrating **PostgreSQL 17** and **Azurite** in Docker, with **persistent data volumes** | One command starts everything with connection strings wired. Data survives restarts. See §3.1. | — |

### 3.1 Local development with Aspire

Aspire is the committed local environment. `Lms.AppHost` is the entry point developers run; it starts the containers, injects connection strings, sequences startup, and serves the dashboard.

```csharp
// src/Lms.AppHost/AppHost.cs
var builder = DistributedApplication.CreateBuilder(args);

// ---- Data: persistent across AppHost restarts -------------------------------
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()                              // named Docker volume
    .WithLifetime(ContainerLifetime.Persistent)    // container outlives the debug session
    .WithPgWeb();                                  // browser SQL client, dev only

var lmsDb = postgres.AddDatabase("lmsdb");

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithDataVolume()
               .WithLifetime(ContainerLifetime.Persistent);
    });

var courseAssets      = storage.AddBlobContainer("course-assets");
var lessonAttachments = storage.AddBlobContainer("lesson-attachments");

// ---- Migrations run to completion before the API starts ---------------------
var migrations = builder.AddProject<Projects.Lms_MigrationService>("migrations")
    .WithReference(lmsDb)
    .WaitFor(postgres);

// ---- Application ------------------------------------------------------------
var api = builder.AddProject<Projects.Lms_Api>("api")
    .WithReference(lmsDb)
    .WithReference(courseAssets)
    .WithReference(lessonAttachments)
    .WaitForCompletion(migrations);

// AddViteApp comes from Aspire.Hosting.JavaScript. (Aspire.Hosting.NodeJs stopped at
// 9.5.2 and has no 13.x release.) WithReference injects services__api__http__0, which the
// Start server reads to reach the API — the browser never receives it.
builder.AddViteApp("web", "../../web")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

> There is no `WithNpmPackageInstallation()` in `Aspire.Hosting.JavaScript` — run `npm install` in `web/` yourself the first time, and let CI run `npm ci`.

**Long-lived data.** This is the part that matters for day-to-day work, and it needs both settings — they do different jobs:

| Setting | Effect | Without it |
|---|---|---|
| `WithDataVolume()` | Binds a **named Docker volume** to the container's data directory. | Data lives in the container's writable layer and dies when the container is removed. |
| `WithLifetime(ContainerLifetime.Persistent)` | The container **keeps running between AppHost sessions** and is reused on the next start. | Aspire tears the container down on shutdown and creates a fresh one — the volume survives, but you pay full container startup on every F5. |

Together: stop debugging, restart tomorrow, and your seeded instructor, draft courses, uploaded thumbnails, and enrollment progress are all still there. Both resources get the treatment — Azurite holding blobs across restarts is what makes the SAS upload flow ([`05 §5`](05-adr-video-and-storage.md#5-direct-to-blob-upload-flow)) practical to iterate on, since re-uploading test assets every session is exactly the friction that leads to that path being skipped and shipped untested.

**Two consequences worth knowing before they surprise you:**

1. **Migrations run against accumulated real data**, not an empty database. This is a feature — a migration that breaks on existing rows fails on your machine instead of in the Container Apps job. It also means a genuinely broken local database needs an explicit `docker volume rm`; keep a documented reset script rather than letting each developer improvise one.
2. **Persistent containers survive `dotnet run` failures.** If the AppHost crashes, the Postgres container is still up. That is usually what you want, but it means "turn it off and on again" does not clear database state — the reset is the volume, not the process.

**What `Lms.ServiceDefaults` covers.** A shared project referenced by `Lms.Api` and `Lms.MigrationService`, calling `builder.AddServiceDefaults()`: OpenTelemetry traces/metrics/logs, `/health/live` and `/health/ready` wiring, HTTP resilience handlers, and service discovery. It is the same code path in Azure — locally the OTLP exporter feeds the Aspire dashboard, in production it feeds Application Insights. One instrumentation setup, two destinations.

**Seed data.** Add a dev-only seeder behind an environment check in `Lms.MigrationService`: the admin account, one instructor with a profile, and one published course with a real YouTube video and both lesson types. Combined with persistent volumes it runs once and stays — and it means a new developer's first `dotnet run` lands on a working catalog rather than an empty one.

---

## 4. What is deliberately absent

The brief says *do not overkill*. These are common additions that would not pay for themselves here:

| Not using | Why |
|---|---|
| **Redis** | Output caching in-process is enough at one or two replicas. Add it when you can measure a need — and you will need it before scaling past a couple of instances. |
| **Azure Service Bus** | In-process events suffice in one process. `IEventBus` makes the later swap a single implementation. |
| **Azure AI Search / Elasticsearch** | `ILIKE` over tens of published courses is honest at this size, and PostgreSQL's built-in `tsvector` full-text search covers the next several orders of magnitude ([`02 §7`](02-domain-model.md#7-data-volume-and-index-plan)). A separate search service would only earn its place with vector/semantic search — a different feature, not a scaling step. |
| **GraphQL** | One client, well-known screens. REST plus TanStack Query covers it without a schema layer. |
| **SignalR** | Nothing is real-time. Progress is a POST. |
| **Feature flags** | Not enough concurrent development to need them. |
| **Monorepo tooling (Nx/Turborepo)** | Two apps in two languages. A `.sln` and a `package.json` are the tooling. |
| **Micro-frontends, module federation** | One frontend, one team. |
| **Kubernetes** | See "compute" above. |

---

## 5. Repository layout

```
LearningManagementSystem/
├─ artifacts/               brief + this design set
├─ src/                     .NET solution — see 01-architecture.md §3
├─ tests/
├─ web/                     TanStack Start app
├─ infra/                   Bicep
├─ .github/workflows/
└─ LearningManagementSystem.sln
```

One repository. Backend and frontend ship together and the API contract changes with the client that consumes it — splitting them would buy version skew and nothing else. It is also what lets the AppHost reference `web/` as a Vite resource and run the whole system from one command.

---

## 6. Version notes

Pin these at project start and record the actual versions here once resolved:

| Component | Target |
|---|---|
| .NET SDK | 10.0.x (LTS) |
| EF Core | 10.0.x |
| Npgsql.EntityFrameworkCore.PostgreSQL | matching major (10.x) |
| PostgreSQL | 17 (local container and Flexible Server) |
| Aspire | latest stable at project start |
| Docker Desktop | required for local dev — the AppHost cannot start containers without it |
| Node | 22 LTS |
| React | 19.x |
| TanStack Start / Router / Query | latest stable at project start |
| Tailwind | 4.x |

TanStack Start reached 1.0 relatively recently; check its release notes at kickoff rather than trusting any version number written in a design document — including this one.
