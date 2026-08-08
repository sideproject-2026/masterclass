# 07 — Build Roadmap

> Four vertical slices. Each one is independently demoable and leaves the system in a working state.
> Slices are ordered by dependency, not by layer — there is no "build all the entities" phase.

---

## Slice 0 — Walking skeleton

**Goal:** one endpoint, end to end, deployed. Prove the plumbing before writing features into it.

- Solution and project layout per [`01 §3`](01-architecture.md#3-project-layout). Empty Modules with their `AddXModule`/`MapXEndpoints` pairs wired into `Program.cs`.
- **`Lms.AppHost` and `Lms.ServiceDefaults` first** ([`06 §3.1`](06-tech-stack.md#31-local-development-with-aspire)): Postgres and Azurite with `WithDataVolume()` + `ContainerLifetime.Persistent`, migrations sequenced by `WaitForCompletion`, the Vite app as a resource. Get this working before writing a single entity — it is the loop every later slice is built in.
- A documented **volume reset script** (`docker volume rm`), so a broken local database has one known fix rather than five improvised ones.
- `Lms.SharedKernel`: `Result`, `Error`, `PagedResult`, `IClock`, `AuthPolicies`, `IEventBus`.
- `GET /health/ready` hitting PostgreSQL, `AddOpenApi()` + Swagger UI in Development.
- TanStack Start app with the root route and one server function calling the API.
- Bicep for the resource group, PostgreSQL Flexible Server, storage, two container apps; GitHub Actions build → migration job → deploy.
- **Settle the UUIDv7 and `xmin` conventions here**, in `SharedKernel`, before the first entity exists — retrofitting key generation or a concurrency token across five Modules is avoidable pain.
- **`Lms.ArchitectureTests` from day one** ([`01 §4.1`](01-architecture.md#41-enforcement)) — retrofitting boundary rules onto code that already violates them is how modular monoliths quietly stop being modular.

**Done when:** `dotnet run` on the AppHost brings up the full stack locally with data that survives a restart, **and** a commit to `main` deploys with `/health/ready` returning 200 from Azure.

---

## Slice 1 — Identity and roles

**Serves:** R3 (login), R4 (registration), and the foundation for everything gated.

- Identity Module: `AppUser` (Guid keys), roles seeded (`Student`, `Instructor`, `Admin`), `InstructorProfile`.
- `/api/auth/register`, `/login`, `/refresh`, `/logout`, `/api/me` ([`03 §3`](03-api-design.md#3-authentication--apiauth)).
- JWT bearer validation on the API; the three named policies.
- BFF in the web app: encrypted `__Host-session` cookie, transparent server-side refresh, `_authed.tsx` and `_instructor.tsx` guards ([`04 §3.1`](04-adr-authentication.md#31-implementation-notes)).
- Login and register pages.
- `/api/admin/users/{id}/grant-instructor` — the whole of curated onboarding.
- Admin seeded from configuration at deploy; one instructor granted manually.
- Rate limiting on `/api/auth/*`.

**Demo:** register as a student, log in, see your name in the nav. An admin grants you `Instructor`; the Studio link appears.

**Watch for:** the session cookie must be `HttpOnly` — verify in DevTools that no token is reachable from JavaScript. If it is, the BFF is not actually doing its job.

---

## Slice 2 — Instructor Studio

**Serves:** R1, R3. The largest slice — authoring is where the surface area is.

- Catalog Module: `Course`, `Chapter`, `Lesson`, `LessonAttachment` with the invariants from [`02 §3`](02-domain-model.md#3-catalog-module--schema-catalog).
- All of `/api/studio/*` ([`03 §4`](03-api-design.md#4-instructor-studio--apistudio--policy-instructor)) — course CRUD, publish with the full 422 invariant report, chapter/lesson CRUD, reorder, move.
- Media Module: YouTube URL parsing and id validation; Azure Blob SAS minting.
- Direct-to-blob upload for thumbnails and attachments ([`05 §5`](05-adr-video-and-storage.md#5-direct-to-blob-upload-flow)), exercised locally against Azurite.
- Studio UI: course list, settings form, curriculum tree with drag-reorder, lesson editor with the Video/Reading toggle and a markdown preview pane.
- **Ownership checks on every write.** Test with two instructor accounts explicitly — this is the easiest authorization bug to ship.

**Demo:** create a course, add two chapters and four lessons (two YouTube videos with notes, two markdown readings), upload a thumbnail and a PDF, attempt to publish with an empty chapter and see the 422, fix it, publish.

**Watch for:** reorder must send the complete ordered list ([`02 §3.4`](02-domain-model.md#34-ordering)). Do not let per-item index deltas creep in.

---

## Slice 3 — Public catalog and enrollment

**Serves:** R2, R4.

- `GET /api/courses`, `/api/courses/{slug}`, `/api/instructors/{slug}` — published content only.
- Output caching, invalidated on `CoursePublished`.
- `POST /api/courses/{courseId}/enroll` — idempotent, behind `IEntitlementService` (`AlwaysAllow` in MVP).
- Enrollment Module: `Enrollment` entity, unique `(StudentId, CourseId)` index.
- `GET /api/me/enrollments`.
- Catalog page with search and filters (SSR), course detail page with curriculum outline, preview player, and the Enroll/Continue CTA.
- My Learning page.

**Demo:** browse the catalog signed out, open a course, watch a preview lesson, register, enroll, land on My Learning at 0%.

**Watch for:** the course-detail payload must carry **no** `externalVideoId` for non-preview lessons ([`03 §2`](03-api-design.md#get-apicoursesslug)). Check the network tab, not the UI.

---

## Slice 4 — Player, progress, completion

**Serves:** R5, R6, R7, R8. This is the slice that closes the loop.

- `GET /api/learn/{courseSlug}` — curriculum plus the caller's progress.
- `GET /api/learn/lessons/{lessonId}` — **the gate**. Enrollment required; previews excepted. Attachment read-SAS minted per request.
- `POST .../progress` (monotonic `watchedSeconds`, 15s heartbeat, `sendBeacon` on unload), `/complete`, `/uncomplete`.
- Completion calculation via `Catalog.Contracts.ICourseCurriculumQuery`; `CourseCompleted` event.
- Notifications Module: outbox table, background drain, congratulations email.
- `GET /api/courses/{courseId}/completion` — congratulations payload with deterministic suggestions ([`02 §4.4`](02-domain-model.md#44-suggestions-after-completion-r5)), `certificate: null`.
- Player UI: curriculum sidebar with completion ticks, YouTube IFrame player wired to heartbeats, markdown reading view, prev/next navigation, resume-where-you-left-off.
- Completion page with suggestion cards.
- `GET /api/studio/courses/{id}/stats`.

**Demo:** enroll, watch a video to 90% and see it auto-tick, mark a reading complete, close the tab mid-lesson and reopen to resume at the right position, finish the last lesson, get the congratulations screen with three suggestions and a congratulations email.

**Watch for:** hit `GET /api/learn/lessons/{id}` for a non-preview lesson **without** an enrollment, using a raw HTTP client. It must return 403. If it returns content, R8 is not implemented — the UI hiding the link is not the control.

---

## Slice 5 — Hardening (before real users)

Not a feature slice, and not optional before opening registration.

- Email confirmation enforced; password reset working end to end.
- 2FA available, encouraged for `Admin`.
- Audit log for role grants and revocations.
- Orphan-blob cleanup job.
- Playwright specs for the two critical journeys (author→publish, enroll→complete).
- Load-check the catalog and progress endpoints; progress is the chattiest by design.
- CSP, security headers, dependency scan.
- Application Insights dashboard and alerts on 5xx rate and outbox backlog.
- Backup/restore rehearsed once, on a real restore — not just verified as configured.

---

## Sequencing notes

**Slices 1 → 2 → 3 → 4 are strictly ordered.** Studio cannot be built without roles; the catalog has nothing to show without published courses; the player needs enrollments.

**What can run in parallel:** frontend and backend within a slice (the API contract in [`03`](03-api-design.md) is the interface — build against it with a mock while the endpoints land); Bicep and CI alongside Slice 0; the Notifications outbox alongside Slice 4.

**Slice 2 is roughly the size of 3 and 4 combined.** If the schedule needs cutting, the honest reductions inside Slice 2 are: replace drag-reorder with up/down buttons, and skip lesson `move`-between-chapters. Both are additive later.

---

## Explicitly after MVP

In the order they are most likely to be wanted:

1. **Migrate video off YouTube** (Cloudflare Stream) — a hard prerequisite for anything paid ([`05 §4`](05-adr-video-and-storage.md#4-limitations-you-are-accepting)).
2. **Billing** — the `IEntitlementService` seam plus a Billing Module ([`02 §6`](02-domain-model.md#6-billing--modeled-not-built)).
3. **Certificates** — deferred by the brief.
4. **Quizzes and exercises** — a new Module.
5. **Course versioning** — publishing edits currently goes live immediately.
6. **Search upgrade** — a generated `tsvector` column + GIN index ([`02 §7`](02-domain-model.md#7-data-volume-and-index-plan)). One migration, no new service. An external search service is only warranted if semantic/vector search becomes a product requirement.
7. **Reviews and ratings** — once there is enough traffic for them to mean something.

---

## Definition of done for the MVP

The full loop, on deployed infrastructure, with no manual database edits:

1. An admin grants a user the `Instructor` role.
2. That instructor authors a course with video and reading lessons and publishes it.
3. A new visitor browses the catalog signed out and watches a preview.
4. They register, enroll, and work through the course across two sessions with progress preserved.
5. On finishing, they see the congratulations screen with suggestions and receive the email.
6. Every one of R1–R8 in [`00 §3`](00-overview.md#3-requirements-mvf) is exercised by that walkthrough, and `GET /api/learn/lessons/{id}` returns 403 to a non-enrolled caller.
