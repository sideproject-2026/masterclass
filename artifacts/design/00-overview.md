# 00 — Overview & Scope

> Design document set for an engineering-focused Learning Management System.
> Source brief: [`artifacts/llm.md`](../llm.md).
> Status: **Draft for review** · Last updated: 2026-08-08

---

## 1. Problem statement

Build a subscription-style learning platform for software engineers, comparable in shape to dometrain.com. Two participant roles:

- **Instructor** — authors structured learning content (video + written notes, or pure reading material) inside a private authoring area ("Instructor Studio").
- **Student** — browses the public catalog, enrolls, works through content in order, and is congratulated with follow-on suggestions on completion.

The MVP proves the **author → publish → discover → enroll → learn → complete** loop end to end. Everything else waits.

---

## 2. Vocabulary (read this first)

The brief uses the word *module* for two different concepts. This document set fixes the vocabulary once and uses it consistently everywhere. Where you read "module" in `llm.md`, substitute per this table:

| `llm.md` term | Term used here | Meaning |
|---|---|---|
| "module" (the thing a student enrolls in) | **Course** | The enrollable unit. Has a title, description, instructor, and a curriculum. |
| "chapter" | **Chapter** | An ordered section inside a Course. Grouping only — not separately enrollable. |
| video / readable content | **Lesson** | The leaf content item. Exactly one of two types: `Video` or `Reading`. |
| "module" (architecture) | **Module** (capital M) | A bounded slice of the monolith codebase — `Catalog`, `Enrollment`, etc. |

So the content hierarchy is:

```
Course ──< Chapter ──< Lesson (Video | Reading)
```

A student enrolls in a **Course**. Progress is tracked per **Lesson**. Completion is computed over the Course.

Other terms used throughout:

| Term | Meaning |
|---|---|
| **Studio** | The instructor-only authoring area (UI + `/api/studio/*` endpoints). |
| **Curriculum** | A Course's full Chapter/Lesson tree. |
| **Preview lesson** | A lesson viewable without enrolling — the marketing free sample. |
| **Entitlement** | Whether a given user is permitted to enroll in a given Course. Always `true` in MVP; the seam where Billing plugs in later. |

---

## 3. Requirements (MVF)

The five items from `llm.md` restated as numbered, testable requirements. Every one of these is traced to concrete endpoints in [`03-api-design.md`](03-api-design.md#9-requirement-traceability).

| ID | Requirement | Notes |
|---|---|---|
| **R1** | An instructor can create a Course, structure it into Chapters and Lessons, and for each Lesson either attach a video or write readable content. | Video ingest is via YouTube for MVP — see [`05`](05-adr-video-and-storage.md). |
| **R2** | A student can browse all published Courses without signing in. | Anonymous catalog. Unpublished/draft content is never visible. |
| **R3** | An instructor can log in and reach the Instructor Studio, where they manage Courses, Chapters, and Lessons (video + notes, or reading content). | Requires role-based access — see [`04`](04-adr-authentication.md). |
| **R4** | A student can self-register and enroll in a Course. | Self-registration is student-only. Instructors are provisioned — see §5. |
| **R5** | On completing a Course, the student sees a congratulations screen with suggestions for what to learn next. | **No certificate in MVP** — explicitly out of scope per the brief. |

### 3.1 Supporting requirements (implied, not optional)

These are not in the brief's list but the loop does not work without them:

| ID | Requirement |
|---|---|
| **R6** | The system tracks per-lesson progress so a student can leave and resume where they stopped. |
| **R7** | A student sees their enrolled Courses and each one's progress in one place ("My Learning"). |
| **R8** | Course content is gated: a non-enrolled user cannot fetch lesson bodies or video IDs, except for lessons explicitly marked as previews. |

---

## 4. Non-goals for MVP

Named explicitly so they do not creep in. Each is a deliberate deferral, not an oversight.

| Not building | Why / when |
|---|---|
| **Certificates of completion** | Brief says so directly. Post-MVP; needs a verifiable-credential design. |
| **Payments and subscriptions** | Modeled as a boundary only — see §5 and [`02`](02-domain-model.md#6-billing--modeled-not-built). Adding it must not reshape Catalog or Enrollment. |
| **Quizzes, exercises, assessments** | Large domain in its own right. Would become a seventh Module. |
| **Discussions, Q&A, comments** | Needs moderation tooling to be responsible. Not on the critical path. |
| **Self-hosted / DRM video** | YouTube covers MVP. See the honest limitations in [`05`](05-adr-video-and-storage.md#4-limitations-you-are-accepting). |
| **Live cohorts, scheduling, calendars** | Different product shape entirely. |
| **Mobile apps** | Responsive web only. |
| **Course reviews & ratings** | Meaningless without volume. Add when there is traffic to rate. |
| **Multi-language / i18n** | English only. Keep user-facing strings out of the database so this stays cheap later. |
| **Instructor payouts / revenue share** | Follows Billing, not MVP. |

---

## 5. Confirmed product decisions

Decisions taken with the product owner before design, recorded here because they constrain everything downstream.

| Decision | Choice | Consequence |
|---|---|---|
| **Instructor onboarding** | **Curated / invite-only.** There is no public instructor signup. An admin grants the `Instructor` role to an existing account. | No application form, no approval state machine, no content-moderation queue. Onboarding is one admin endpoint. |
| **Monetization** | **Model the seam, don't build it.** Every Course is free-enroll in MVP. | Enrollment calls `IEntitlementService.CanEnrollAsync(...)`, which MVP implements as always-allow. Billing later replaces that implementation and adds its own tables — touching nothing in Catalog. |
| **Cloud** | **Azure.** | Azure Blob Storage for assets, Azure Database for PostgreSQL for data, Container Apps for compute. See [`01`](01-architecture.md#7-deployment-topology-azure) and [`01 §7.2`](01-architecture.md#72-why-postgresql-over-azure-sql). |
| **Deliverable** | Design documents only. No code, no scaffolding. | This document set. |

---

## 6. Actors and permissions

Three roles. Kept deliberately flat — no per-course permission grants, no organizations, no teams.

| Role | Granted by | Can |
|---|---|---|
| **Student** | Self-registration | Browse catalog, enroll, consume enrolled content, track progress. Every registered user has this. |
| **Instructor** | Admin grant | Everything a Student can, plus: author and publish **their own** Courses in Studio. Cannot see or edit another instructor's Courses. |
| **Admin** | Seeded at deploy | Grant/revoke the `Instructor` role. Nothing else in MVP — no admin console. |

Ownership rule that matters: **a Course belongs to exactly one instructor.** Co-authoring is not supported. Studio endpoints must verify `course.InstructorId == currentUser.Id` on every write, not merely that the caller holds the `Instructor` role.

---

## 7. Core user journeys

**Instructor authoring (R1, R3)**
Log in → Studio → create Course (draft) → add Chapters → add Lessons → for each Lesson paste a YouTube URL and write notes, *or* write markdown reading content → mark one or two Lessons as previews → publish. Published Courses appear in the public catalog immediately.

**Student discovery and enrollment (R2, R4)**
Land on catalog (no account needed) → search/filter → open a Course page → watch a preview lesson → register or log in → enroll → land in the player.

**Learning and completion (R5, R6, R7)**
Player shows curriculum sidebar + current lesson. Video lessons report watch progress; a lesson auto-completes at ≥90% watched, and can also be marked complete manually. Reading lessons complete on an explicit button. When the last required lesson completes, the Enrollment flips to `Completed` and the student gets the congratulations screen with next-course suggestions.

---

## 8. Design principles for this build

Drawn from the brief's instruction — *do not overkill*.

1. **Boring where it doesn't matter.** REST, a relational database, server-rendered-then-hydrated pages. No novelty budget spent on infrastructure.
2. **One database, clear seams.** A modular monolith with enforced boundaries buys most of what microservices promise, at a fraction of the operational cost. See [`01`](01-architecture.md).
3. **Abstract only where a swap is genuinely foreseeable.** Two abstractions earn their place: `IVideoProvider` (YouTube will be replaced) and `IEntitlementService` (billing is coming). Everything else talks to EF Core directly.
4. **Derive, don't duplicate.** Completion and progress percentage are computed from `LessonProgress`. The denormalized copy on `Enrollment` is a read optimization with a single writer, not a second source of truth.
5. **Gate at the API, not the UI.** Hiding the player button is presentation. The entitlement check on `GET /api/learn/lessons/{id}` is the actual control.

---

## 9. Document map

| Document | Answers |
|---|---|
| **00 — Overview** (this) | What are we building, for whom, and what are we deliberately not building? |
| [01 — Architecture](01-architecture.md) | Monolith vs microservices, Module composition, project layout, Azure topology. |
| [02 — Domain model](02-domain-model.md) | Entities, relationships, invariants, persistence strategy. |
| [03 — API design](03-api-design.md) | Every endpoint, its auth policy, shapes, status codes — plus the R1–R5 traceability matrix. |
| [04 — ADR: Authentication](04-adr-authentication.md) | Keycloak vs Duende vs the alternative we actually recommend. |
| [05 — ADR: Video & storage](05-adr-video-and-storage.md) | YouTube for video, Azure Blob for assets, and the limitations you are accepting. |
| [06 — Tech stack](06-tech-stack.md) | .NET 10 and TanStack Start, with the supporting libraries and why each is there. |
| [07 — Roadmap](07-roadmap.md) | Four demoable slices, in build order. |
| [08 — Delivery plan](08-delivery-plan.md) | Sprint-by-sprint schedule, backlog, milestones, risks, descope levers. |
| [09 — Code conventions](09-code-conventions.md) | Patterns, the in-house mediator, naming, and what we deliberately don't do. |
