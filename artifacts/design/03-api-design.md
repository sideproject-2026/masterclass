# 03 — API Design

> The contract. REST over ASP.NET Core minimal APIs, `/api` prefix, grouped by audience.
> Entities referenced here are defined in [`02-domain-model.md`](02-domain-model.md).
> Auth policies come from [`04-adr-authentication.md`](04-adr-authentication.md).

---

## 1. Conventions

| Aspect | Convention |
|---|---|
| **Base path** | `/api`. Route groups per audience: `/api/courses` (public), `/api/auth`, `/api/studio`, `/api/learn`, `/api/admin`. |
| **Auth** | Bearer token on every non-public request. Applied at the **group** level with `.RequireAuthorization(policy)`; ownership checks happen inside handlers. |
| **Casing** | `camelCase` JSON. Enums serialized as strings (`"Published"`, not `2`) — the wire format should survive reordering an enum. |
| **Ids** | `Guid` (v7, sequential). Courses are additionally addressable by `slug` on public routes. |
| **Errors** | RFC 9457 `application/problem+json` on every 4xx/5xx. Never a bare string. |
| **Validation** | FluentValidation via endpoint filter → `400` with an `errors` extension member. |
| **Paging** | Offset paging: `?page=1&pageSize=20`. `pageSize` is **clamped** to 50, not rejected. Response is `PagedResult<T>`, built from the internal `QueryResult<T>` — see [`09 §8`](09-code-conventions.md#8-pagination). |
| **Dates** | ISO 8601 with offset (`2026-08-08T09:10:00+00:00`). |
| **Versioning** | None in MVP. The path prefix leaves room for `/api/v2` if it is ever needed; do not build the machinery now. |
| **OpenAPI** | `builder.Services.AddOpenApi()` + `app.MapOpenApi()` → `/openapi/v1.json`. .NET 10 generates the document in-box — no Swashbuckle. `Microsoft.AspNetCore.OpenApi` ships **no UI**, so **Scalar** (`MapScalarApiReference()` → `/scalar/v1`) renders it, Development only. |

### 1.1 Standard envelopes

```jsonc
// PagedResult<T>
{
  "items": [ /* T */ ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 137,
  "totalPages": 7
}
```

```jsonc
// ProblemDetails — validation failure
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-8f1c...-01",
  "errors": { "title": ["Title is required."] }
}
```

```jsonc
// ProblemDetails — domain rule violation
{
  "type": "https://lms.example.com/errors/course-not-publishable",
  "title": "Course cannot be published.",
  "status": 422,
  "detail": "Chapter 'Getting Started' has no lessons.",
  "traceId": "00-8f1c...-01"
}
```

### 1.2 Status codes

| Code | Used for |
|---|---|
| `200` | Successful read, or an idempotent write that found existing state. |
| `201` | Resource created. `Location` header set. |
| `204` | Successful delete or fire-and-forget write (progress heartbeat). |
| `400` | Malformed request or validation failure. |
| `401` | Missing/expired token. |
| `403` | Authenticated but not permitted — wrong role, not the course owner, or **not enrolled**. |
| `404` | Not found, **or** exists but the caller must not learn that it does (an unpublished course to an anonymous caller is a 404, not a 403). |
| `409` | Concurrency conflict (`rowVersion` mismatch). |
| `422` | Well-formed request that violates a domain invariant (e.g. publishing an empty course). |
| `429` | Rate limited. |

---

## 2. Public catalog — `/api/courses` · **anonymous**

Serves R2. No authentication. Output-cached.

### `GET /api/courses`

Browse published courses.

| Query param | Type | Notes |
|---|---|---|
| `search` | `string?` | Matches title, subtitle, tags. |
| `level` | `Beginner\|Intermediate\|Advanced`? | |
| `tag` | `string?` | Exact tag match. |
| `instructor` | `string?` | Instructor slug. |
| `sort` | `newest\|popular\|title` | Default `newest`. `popular` = enrollment count. |
| `page`, `pageSize` | `int` | Default `1`, `20`. |

**200** → `PagedResult<CourseSummary>`

```jsonc
{
  "id": "0193f2...",
  "slug": "distributed-systems-in-dotnet",
  "title": "Distributed Systems in .NET",
  "subtitle": "Build systems that survive the network",
  "level": "Advanced",
  "thumbnailUrl": "https://cdn.../course-assets/0193f2/thumb.webp",
  "tags": ["dotnet", "distributed-systems"],
  "lessonCount": 42,
  "estimatedMinutes": 380,
  "enrollmentCount": 1204,
  "publishedAt": "2026-05-02T00:00:00+00:00",
  "instructor": { "slug": "jane-doe", "displayName": "Jane Doe", "avatarUrl": "https://cdn/..." }
}
```

Only `Status = Published` is ever returned. Draft and archived courses are invisible here, full stop.

### `GET /api/courses/{slug}`

Course landing page. Returns the full curriculum **outline** — titles, types, durations — but **no lesson bodies and no video ids** except for preview lessons. This is the marketing view; the content gate lives in §5.

**200** → `CourseDetail`

```jsonc
{
  "id": "0193f2...",
  "slug": "distributed-systems-in-dotnet",
  "title": "Distributed Systems in .NET",
  "subtitle": "Build systems that survive the network",
  "description": "## What you'll learn\n...",       // markdown
  "level": "Advanced",
  "thumbnailUrl": "https://cdn/...",
  "tags": ["dotnet", "distributed-systems"],
  "lessonCount": 42,
  "estimatedMinutes": 380,
  "enrollmentCount": 1204,
  "publishedAt": "2026-05-02T00:00:00+00:00",
  "instructor": { "slug": "jane-doe", "displayName": "Jane Doe", "headline": "...", "avatarUrl": "..." },
  "chapters": [
    {
      "id": "0193f3...",
      "title": "Getting Started",
      "sortOrder": 0,
      "lessons": [
        { "id": "0193f4...", "title": "Why distribution hurts", "type": "Video",
          "durationSeconds": 540, "isPreview": true,  "sortOrder": 0 },
        { "id": "0193f5...", "title": "Failure modes",          "type": "Reading",
          "durationSeconds": 300, "isPreview": false, "sortOrder": 1 }
      ]
    }
  ],
  "viewer": { "isAuthenticated": true, "isEnrolled": false, "enrollmentId": null }
}
```

`viewer` is populated when a token is present and defaults to `isEnrolled: false` otherwise — it drives the "Enroll" vs "Continue" button without a second round trip. Catalog gets it from `Enrollment.Contracts.IEnrollmentLookup`, not from a join ([`01 §2.2`](01-architecture.md#22-dependency-direction)). `enrollmentCount` comes from the denormalized column on `Course`, maintained by the `StudentEnrolled` subscriber ([`02 §3`](02-domain-model.md#3-catalog-module--schema-catalog)).

**404** if the slug does not exist **or** the course is not published.

### `GET /api/instructors/{slug}`

**200** → instructor profile + their published courses (`CourseSummary[]`).
**404** if unknown.

---

## 3. Authentication — `/api/auth`

Detail and rationale in [`04`](04-adr-authentication.md). These are consumed by the TanStack Start BFF, not directly by the browser.

| Endpoint | Auth | Purpose |
|---|---|---|
| `POST /api/auth/register` | anonymous | Create a student account. |
| `POST /api/auth/login` | anonymous | Exchange credentials for tokens. |
| `POST /api/auth/refresh` | anonymous (refresh token in body) | Rotate the access token. |
| `POST /api/auth/logout` | authenticated | Revoke the refresh token. |
| `GET /api/me` | authenticated | Current user + roles. |
| `PUT /api/me` | authenticated | Update `displayName`. |

### `POST /api/auth/register`

```jsonc
// request
{ "email": "sam@example.com", "password": "•••••••••••", "displayName": "Sam" }
```

**201** → `{ "userId": "...", "email": "...", "displayName": "..." }`
**400** validation (password policy, malformed email).
**409** email already registered.

Always assigns the `Student` role. There is no way to self-register as an instructor — see [`00 §5`](00-overview.md#5-confirmed-product-decisions).

### `POST /api/auth/login`

```jsonc
// request
{ "email": "sam@example.com", "password": "•••••••••••" }
// 200
{ "accessToken": "eyJ...", "expiresIn": 3600, "refreshToken": "...", "tokenType": "Bearer" }
```

**401** on bad credentials — deliberately indistinguishable from an unknown email, so the endpoint is not a user-enumeration oracle.
**429** after repeated failures (rate limiter on this group).

### `GET /api/me`

**200** → `{ "id", "email", "displayName", "roles": ["Student","Instructor"], "instructorSlug": "jane-doe" | null }`

`instructorSlug` is the instructor's public URL segment. It is **not** the signal for showing the
Studio link — the `Instructor` **role** is. `A-6` deliberately keeps the profile and the slug when
a grant is revoked, so that course pages still name the author and nobody inherits someone else's
public URL; a revoked instructor therefore still has a slug, and keying the link off it would
offer them a Studio that answers 403. See `canUseStudio` in `web/src/features/auth/access.ts`.

---

## 4. Instructor Studio — `/api/studio` · **policy `Instructor`**

Serves R1 and R3. Every handler additionally verifies `course.InstructorId == caller.Id` and returns **403** otherwise. Holding the `Instructor` role is not authorization to edit *someone else's* course.

### 4.1 Courses

| Endpoint | Purpose | Success |
|---|---|---|
| `GET /api/studio/courses` | The caller's courses, any status. `?status=Draft` filter. | `200 PagedResult<StudioCourseSummary>` |
| `POST /api/studio/courses` | Create a draft. | `201 StudioCourse` + `Location` |
| `GET /api/studio/courses/{id}` | Full course incl. curriculum and unpublished content. | `200 StudioCourse` |
| `PUT /api/studio/courses/{id}` | Update metadata. | `200` |
| `POST /api/studio/courses/{id}/publish` | Draft → Published. | `200` / `422` |
| `POST /api/studio/courses/{id}/unpublish` | Published → Draft. | `200` |
| `POST /api/studio/courses/{id}/archive` | → Archived. | `200` |
| `DELETE /api/studio/courses/{id}` | Hard delete. **409** if enrollments exist. | `204` / `409` |
| `POST /api/studio/courses/{id}/thumbnail-upload-url` | Mint a SAS for the thumbnail. | `200 UploadTicket` |
| `GET /api/studio/courses/{id}/stats` | Enrollment/completion counts. | `200 CourseStats` |

**`POST /api/studio/courses`**

```jsonc
// request
{ "title": "Distributed Systems in .NET", "subtitle": "...", "level": "Advanced" }
// 201
{ "id": "0193f2...", "slug": "distributed-systems-in-dotnet", "status": "Draft", ... }
```

Slug is derived from the title, de-duplicated with a numeric suffix, editable while `Draft`, and frozen on publish.

**`PUT /api/studio/courses/{id}`**

```jsonc
{
  "title": "...", "subtitle": "...", "description": "## markdown",
  "level": "Advanced", "tags": ["dotnet", "distributed-systems"],
  "slug": "distributed-systems-in-dotnet",     // ignored unless status = Draft
  "rowVersion": 4521                           // xmin; 409 on mismatch
}
```

**`POST /api/studio/courses/{id}/publish`** — runs the invariants from [`02 §3.2`](02-domain-model.md#32-publish-invariants). On failure, **422** listing *every* unmet condition, not just the first:

```jsonc
{
  "type": "https://lms.example.com/errors/course-not-publishable",
  "title": "Course cannot be published.", "status": 422,
  "errors": {
    "thumbnail": ["A thumbnail is required."],
    "chapters":  ["Chapter 'Advanced Topics' has no lessons."]
  }
}
```

On success, publishes `CoursePublished` (invalidates the catalog output cache).

**`GET /api/studio/courses/{id}/stats`** — MVP analytics, three numbers:

```jsonc
{ "enrollmentCount": 1204, "completionCount": 311, "activeLast30Days": 480,
  "averageProgressPercent": 47 }
```

Served by `Enrollment.Contracts.ICourseStatsQuery` — Studio does not query `enrollment` tables directly.

### 4.2 Chapters

| Endpoint | Purpose | Success |
|---|---|---|
| `POST /api/studio/courses/{courseId}/chapters` | Append a chapter. | `201 Chapter` |
| `PUT /api/studio/chapters/{id}` | Rename. | `200` |
| `DELETE /api/studio/chapters/{id}` | Delete + cascade lessons. Re-densifies sort order. | `204` |
| `POST /api/studio/courses/{courseId}/chapters/reorder` | Rewrite ordering. | `204` |

**Reorder** takes the complete ordered list — see [`02 §3.4`](02-domain-model.md#34-ordering) for why deltas are rejected:

```jsonc
{ "chapterIds": ["0193f3...", "0193fa...", "0193fb..."] }
```

**400** if the list is not exactly the set of the course's chapter ids.

### 4.3 Lessons

| Endpoint | Purpose | Success |
|---|---|---|
| `POST /api/studio/chapters/{chapterId}/lessons` | Create. | `201 Lesson` |
| `GET /api/studio/lessons/{id}` | Full lesson incl. content. | `200 StudioLesson` |
| `PUT /api/studio/lessons/{id}` | Update title/flags/content. | `200` / `422` |
| `DELETE /api/studio/lessons/{id}` | Delete. | `204` |
| `POST /api/studio/chapters/{chapterId}/lessons/reorder` | Rewrite ordering. | `204` |
| `POST /api/studio/lessons/{id}/move` | Move to another chapter in the same course. | `204` |

**`PUT /api/studio/lessons/{id}`** — one endpoint, discriminated by `type`. This is R1's core write.

```jsonc
// Video lesson
{
  "title": "Why distribution hurts",
  "type": "Video",
  "isPreview": true,
  "isRequired": true,
  "videoUrl": "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
  "notesMarkdown": "## Key points\n- ...",
  "rowVersion": 4521
}
```

```jsonc
// Reading lesson
{
  "title": "Failure modes",
  "type": "Reading",
  "isPreview": false,
  "isRequired": true,
  "contentMarkdown": "# Failure modes\n...",
  "rowVersion": 4521
}
```

Notes on the video path:
- The instructor pastes a **full URL**; the Media Module parses it. All common YouTube forms are accepted (`watch?v=`, `youtu.be/`, `/embed/`, `/shorts/`, with extra query params). The API stores only the extracted `externalVideoId`.
- **400** `invalid-video-url` if it does not parse.
- `durationSeconds` is fetched server-side via the YouTube Data API when a key is configured; otherwise the client must supply it. Either way it is server-validated as `> 0` before publish.
- Switching `type` clears the other type's fields ([`02 §3.3`](02-domain-model.md#33-lesson-content-invariant)). The response reflects the cleared state so the Studio UI cannot resubmit stale content.
- **422** if the content invariant is violated (e.g. `type: "Reading"` with empty `contentMarkdown`).

### 4.4 Attachments

| Endpoint | Purpose | Success |
|---|---|---|
| `POST /api/studio/lessons/{id}/attachments/upload-url` | Mint a write SAS. | `200 UploadTicket` |
| `POST /api/studio/lessons/{id}/attachments` | Confirm upload, persist metadata. | `201 Attachment` |
| `DELETE /api/studio/attachments/{id}` | Delete row + blob. | `204` |

```jsonc
// request  → POST .../attachments/upload-url
{ "fileName": "slides.pdf", "contentType": "application/pdf", "sizeBytes": 2411520 }

// 200 UploadTicket
{
  "uploadUrl": "https://lmsstore.blob.core.windows.net/lesson-attachments/0193f4/slides.pdf?sv=...&sig=...",
  "blobPath": "0193f4/slides.pdf",
  "expiresAt": "2026-08-08T09:25:00+00:00",
  "requiredHeaders": { "x-ms-blob-type": "BlockBlob" }
}
```

The browser `PUT`s bytes straight to Azure, then calls `POST .../attachments` with the returned `blobPath` to create the row. Bytes never pass through the API. Content type and size are validated **when minting the SAS** — that is the only moment the server controls. Full flow: [`05 §5`](05-adr-video-and-storage.md#5-direct-to-blob-upload-flow).

---

## 5. Learning — `/api/learn` and enrollment · **policy `Student`**

Serves R4–R8. This group holds the content gate.

### `POST /api/courses/{courseId}/enroll`

**Idempotent.** Returns `201` on first enroll, `200` with the existing enrollment on repeat — never `409`. Backed by the unique index on `(StudentId, CourseId)`.

```jsonc
// 201 / 200
{ "enrollmentId": "0194aa...", "courseId": "0193f2...", "status": "Active",
  "enrolledAt": "2026-08-08T09:10:00+00:00", "progressPercent": 0,
  "firstLessonId": "0193f4..." }
```

**403** `entitlement-required` when `IEntitlementService.CanEnrollAsync` returns false. Always true in MVP; this is the Billing seam ([`02 §6`](02-domain-model.md#6-billing--modeled-not-built)) and it exists in the contract from day one so adding paid courses is not a breaking change.
**404** if the course is not published.
Re-enrolling after `Dropped` flips back to `Active` and **retains prior progress**.

### `DELETE /api/courses/{courseId}/enroll`

Unenroll → `Dropped`. Progress rows are kept. **204**.

### `GET /api/me/enrollments`

"My Learning" (R7). `?status=Active|Completed|Dropped`, default `Active`. Sorted by `lastAccessedAt DESC`.

```jsonc
// PagedResult<EnrollmentSummary>
{
  "enrollmentId": "0194aa...",
  "course": { "id": "...", "slug": "...", "title": "...", "thumbnailUrl": "...",
              "instructorName": "Jane Doe", "lessonCount": 42 },
  "status": "Active",
  "progressPercent": 47,
  "completedLessonCount": 20,
  "lastAccessedAt": "2026-08-07T21:03:00+00:00",
  "resumeLessonId": "0193f9..."
}
```

Served entirely from the denormalized fields on `Enrollment` — no curriculum lookups, no N+1.

### `GET /api/learn/{courseSlug}`

The player shell: curriculum plus the caller's per-lesson progress. Still **no lesson bodies** — those come one at a time from the next endpoint.

```jsonc
{
  "course": { "id": "...", "slug": "...", "title": "...", "instructorName": "Jane Doe" },
  "enrollment": { "id": "0194aa...", "status": "Active", "progressPercent": 47,
                  "resumeLessonId": "0193f9..." },
  "chapters": [
    { "id": "...", "title": "Getting Started", "sortOrder": 0,
      "lessons": [
        { "id": "0193f4...", "title": "Why distribution hurts", "type": "Video",
          "durationSeconds": 540, "isRequired": true,
          "isCompleted": true,  "lastPositionSeconds": 540 },
        { "id": "0193f5...", "title": "Failure modes", "type": "Reading",
          "durationSeconds": 300, "isRequired": true,
          "isCompleted": false, "lastPositionSeconds": 0 }
      ] }
  ]
}
```

**403** `not-enrolled` if there is no active enrollment.

### `GET /api/learn/lessons/{lessonId}`

**The gated read (R8).** Access requires *either* an active/completed enrollment in the owning course *or* `lesson.IsPreview = true`. Preview lessons are readable anonymously, which is why this endpoint allows anonymous callers and enforces the rule in the handler rather than at the group.

```jsonc
// 200 — Video
{
  "id": "0193f4...", "chapterId": "...", "courseId": "...",
  "title": "Why distribution hurts", "type": "Video",
  "videoProvider": "YouTube", "externalVideoId": "dQw4w9WgXcQ",
  "durationSeconds": 540,
  "notesMarkdown": "## Key points\n- ...",
  "attachments": [
    { "id": "...", "fileName": "slides.pdf", "sizeBytes": 2411520,
      "downloadUrl": "https://lmsstore.blob.../slides.pdf?sv=...&sig=...",
      "downloadUrlExpiresAt": "2026-08-08T10:10:00+00:00" }
  ],
  "progress": { "isCompleted": false, "lastPositionSeconds": 132 },
  "navigation": { "previousLessonId": null, "nextLessonId": "0193f5..." }
}
```

```jsonc
// 200 — Reading
{ "id": "...", "title": "Failure modes", "type": "Reading",
  "contentMarkdown": "# Failure modes\n...", "durationSeconds": 300,
  "attachments": [], "progress": { ... }, "navigation": { ... } }
```

**403** `not-enrolled` for a non-preview lesson without enrollment.

Two things to hold onto: attachment `downloadUrl`s are **short-lived read SAS tokens minted per request** (~15 min), never stored; and `externalVideoId` is only ever emitted from this endpoint, so it is not sitting in the public course-detail payload.

### `POST /api/learn/lessons/{lessonId}/progress`

The heartbeat. Called roughly every 15 seconds while a video plays, and on pause/unload.

```jsonc
// request
{ "positionSeconds": 240, "watchedSeconds": 250 }
// 204, or 200 when this call caused completion:
{ "lessonCompleted": true, "courseCompleted": false, "progressPercent": 52 }
```

Behaviour:
- Upsert on `(EnrollmentId, LessonId)`.
- `watchedSeconds` is **monotonic** — the server keeps `max(existing, incoming)` so rewinding never reduces credit.
- Auto-completes the lesson at `watchedSeconds >= 0.9 × durationSeconds`.
- Updates `Enrollment.LastAccessedAt` and `LastLessonId`.
- Rate-limited per user; this is the chattiest endpoint in the system by a wide margin.
- Last-write-wins. Two tabs playing the same lesson is not a scenario worth locking for.

### `POST /api/learn/lessons/{lessonId}/complete`

Explicit "Mark complete" — the only way a `Reading` lesson completes, and an escape hatch for videos.

```jsonc
// 200
{ "lessonCompleted": true, "courseCompleted": true, "progressPercent": 100 }
```

Idempotent: completing an already-complete lesson returns the same body and does not move `CompletedAt`.

### `POST /api/learn/lessons/{lessonId}/uncomplete`

Undo. Recomputes `ProgressPercent`. Does **not** reverse a course-level `Completed` status — that transition is terminal ([`02 §4.1`](02-domain-model.md#41-enrollment-status-lifecycle)).

### `GET /api/courses/{courseId}/completion`

**R5.** The congratulations payload. **404** unless the caller's enrollment is `Completed`.

```jsonc
{
  "courseTitle": "Distributed Systems in .NET",
  "completedAt": "2026-08-08T09:10:00+00:00",
  "lessonsCompleted": 42,
  "totalWatchTimeMinutes": 380,
  "daysToComplete": 21,
  "instructor": { "slug": "jane-doe", "displayName": "Jane Doe" },
  "message": "Congratulations! You finished Distributed Systems in .NET.",
  "suggestions": [
    { "id": "...", "slug": "event-driven-architecture", "title": "Event-Driven Architecture",
      "subtitle": "...", "level": "Advanced", "thumbnailUrl": "...",
      "lessonCount": 30, "reason": "Shares topics with what you just finished" },
    { "id": "...", "slug": "...", "title": "...", "reason": "More from Jane Doe" }
  ],
  "certificate": null
}
```

`suggestions` follows the deterministic ranking in [`02 §4.4`](02-domain-model.md#44-suggestions-after-completion-r5) — max 3, never a course the student is already enrolled in.

`certificate` is `null` and stays `null`. The field is present so adding certificates later is additive rather than a shape change; MVP explicitly ships no certificate.

---

## 6. Admin — `/api/admin` · **policy `Admin`**

The entirety of curated instructor onboarding ([`00 §5`](00-overview.md#5-confirmed-product-decisions)). No admin console in MVP — these are called with an HTTP client.

| Endpoint | Purpose | Success |
|---|---|---|
| `POST /api/admin/users/{userId}/grant-instructor` | Add the `Instructor` role and create an `InstructorProfile`. | `200` |
| `POST /api/admin/users/{userId}/revoke-instructor` | Remove the role. Published courses stay published. | `204` |
| `GET /api/admin/users?search=` | Find a user id by email. | `200 PagedResult<AdminUser>` |

```jsonc
// request → grant-instructor
{ "slug": "jane-doe", "headline": "Principal engineer, distributed systems" }
// 200
{ "userId": "...", "roles": ["Student","Instructor"], "instructorSlug": "jane-doe" }
```

**409** if the slug already belongs to someone else. **400** if the slug is malformed — it becomes a public URL segment, so it must be lowercase letters, digits and single hyphens. **404** for an unknown user.

Both writes are **idempotent**: granting a user who already holds the role returns `200` with their existing slug rather than `409`, and revoking someone who is not an instructor returns `204`.

Revoking does **not** unpublish or delete existing courses — that would break enrolled students' access, and content removal should be a deliberate, separate act. It also **keeps the `InstructorProfile` and its slug**: course pages still name the author, and releasing the slug would let a later instructor inherit someone else's URL.

Revocation is not instantaneous. The role travels in an access token that cannot be recalled, so it takes effect within the 15-minute token lifetime — that window *is* the revocation window ([`04 §3.1`](04-adr-authentication.md)).

> **The first `Admin` is seeded from configuration** (`Admin:Email`, `Admin:Password`) and there is no self-service path — see [`04 §7`](04-adr-authentication.md#7-open-items-and-hardening-backlog). The seeder grants **both `Student` and `Admin`**, because "every registered user holds `Student`" ([`02 §2`](02-domain-model.md)) is the invariant behind the `Student` policy meaning "authenticated"; an admin without it is refused `GET /api/me`, contradicting the matrix below.

---

## 7. Authorization matrix

| Endpoint group | Anonymous | Student | Instructor | Admin |
|---|:--:|:--:|:--:|:--:|
| `GET /api/courses`, `/api/courses/{slug}`, `/api/instructors/{slug}` | ✅ | ✅ | ✅ | ✅ |
| `POST /api/auth/*` | ✅ | ✅ | ✅ | ✅ |
| `GET /api/me` | ❌ | ✅ | ✅ | ✅ |
| `/api/studio/*` | ❌ | ❌ | ✅ own courses only | ❌ |
| `POST /api/courses/{id}/enroll`, `GET /api/me/enrollments` | ❌ | ✅ | ✅ | ✅ |
| `GET /api/learn/{slug}` | ❌ | ✅ if enrolled | ✅ if enrolled | ✅ if enrolled |
| `GET /api/learn/lessons/{id}` | ✅ preview only | ✅ if enrolled | ✅ if enrolled | ✅ if enrolled |
| `POST /api/learn/lessons/{id}/progress`, `/complete` | ❌ | ✅ if enrolled | ✅ | ✅ |
| `/api/admin/*` | ❌ | ❌ | ❌ | ✅ |

Two rules that are easy to get wrong:

1. **An admin is not implicitly enrolled.** Admin grants role management, not free content access. No back door around the entitlement gate.
2. **An instructor viewing their own course through `/api/learn` still needs an enrollment.** Studio is where they preview their own content; `/api/learn` is the student path and applies student rules. Keeping these separate stops "is it my course?" logic leaking into the enrollment gate.

---

## 8. Cross-cutting API behaviour

| Concern | Behaviour |
|---|---|
| **Caching** | `GET /api/courses*` and `/api/instructors/*`: output cache, 60 s, varied by query string, evicted on `CoursePublished`. Everything authenticated: `Cache-Control: no-store`. |
| **Rate limits** | `/api/auth/*`: 10 requests / 5 min per IP. `/progress`: 20 / min per user. Everything else: 100 / min per user. `429` + `Retry-After`. |
| **CORS** | Only the web origin, credentials allowed. The BFF means most calls are server-to-server and never hit CORS at all. |
| **Payload caps** | 1 MB request body (markdown lessons are the largest). Attachment bytes never touch the API — SAS handles them. |
| **Concurrency** | `rowVersion` on course and lesson updates → `409` with the current server state so the client can diff rather than guess. The value is a **number**, not a base64 string — it is PostgreSQL's `xmin` ([`02 §8.2`](02-domain-model.md#82-query-conventions)). Clients treat it as opaque: echo back whatever the last read returned. |
| **Correlation** | `traceId` in every `ProblemDetails`, matching the W3C trace context in Application Insights. |
| **Health** | `GET /health/live`, `GET /health/ready` (DB + blob reachability). Unauthenticated, excluded from the OpenAPI document. |

---

## 9. Requirement traceability

Every MVF requirement from [`00 §3`](00-overview.md#3-requirements-mvf) maps to concrete endpoints. This table is the acceptance checklist.

| Req | Requirement | Endpoints |
|---|---|---|
| **R1** | Instructor creates a Course with Chapters and Lessons; video or readable content | `POST /api/studio/courses` · `POST /api/studio/courses/{id}/chapters` · `POST /api/studio/chapters/{id}/lessons` · `PUT /api/studio/lessons/{id}` (Video/Reading) · `POST /api/studio/courses/{id}/publish` |
| **R2** | Student browses all published courses | `GET /api/courses` · `GET /api/courses/{slug}` · `GET /api/instructors/{slug}` |
| **R3** | Instructor logs in and accesses Studio | `POST /api/auth/login` · `GET /api/me` (role check) · all of `/api/studio/*` |
| **R4** | Student registers and enrolls | `POST /api/auth/register` · `POST /api/auth/login` · `POST /api/courses/{id}/enroll` |
| **R5** | Congratulations + suggestions on completion; **no certificate** | `POST /api/learn/lessons/{id}/complete` → `courseCompleted: true` · `GET /api/courses/{id}/completion` (`certificate: null`) |
| **R6** | Progress tracked; student can resume | `POST /api/learn/lessons/{id}/progress` · `GET /api/learn/{slug}` (`resumeLessonId`) |
| **R7** | Student sees enrolled courses and progress | `GET /api/me/enrollments` |
| **R8** | Content is gated except previews | `GET /api/learn/lessons/{id}` → `403 not-enrolled`; `GET /api/courses/{slug}` returns outline only |
| — | Curated instructor onboarding | `POST /api/admin/users/{id}/grant-instructor` |

---

## 10. Endpoint index

Forty-four business endpoints for the whole MVP (3 public, 6 auth, 23 studio, 9 learning, 3 admin), plus three ops endpoints. Studio is over half of it — authoring is where the surface area lives, which is expected for an LMS. If the count starts climbing elsewhere, that is the signal to check scope against [`00 §4`](00-overview.md#4-non-goals-for-mvp).

```
PUBLIC
  GET    /api/courses
  GET    /api/courses/{slug}
  GET    /api/instructors/{slug}

AUTH
  POST   /api/auth/register
  POST   /api/auth/login
  POST   /api/auth/refresh
  POST   /api/auth/logout
  GET    /api/me
  PUT    /api/me

STUDIO                                        [Instructor]
  GET    /api/studio/courses
  POST   /api/studio/courses
  GET    /api/studio/courses/{id}
  PUT    /api/studio/courses/{id}
  DELETE /api/studio/courses/{id}
  POST   /api/studio/courses/{id}/publish
  POST   /api/studio/courses/{id}/unpublish
  POST   /api/studio/courses/{id}/archive
  POST   /api/studio/courses/{id}/thumbnail-upload-url
  GET    /api/studio/courses/{id}/stats
  POST   /api/studio/courses/{courseId}/chapters
  POST   /api/studio/courses/{courseId}/chapters/reorder
  PUT    /api/studio/chapters/{id}
  DELETE /api/studio/chapters/{id}
  POST   /api/studio/chapters/{chapterId}/lessons
  POST   /api/studio/chapters/{chapterId}/lessons/reorder
  GET    /api/studio/lessons/{id}
  PUT    /api/studio/lessons/{id}
  DELETE /api/studio/lessons/{id}
  POST   /api/studio/lessons/{id}/move
  POST   /api/studio/lessons/{id}/attachments/upload-url
  POST   /api/studio/lessons/{id}/attachments
  DELETE /api/studio/attachments/{id}

LEARNING                                      [Student]
  POST   /api/courses/{courseId}/enroll
  DELETE /api/courses/{courseId}/enroll
  GET    /api/me/enrollments
  GET    /api/learn/{courseSlug}
  GET    /api/learn/lessons/{lessonId}         (anonymous for previews)
  POST   /api/learn/lessons/{lessonId}/progress
  POST   /api/learn/lessons/{lessonId}/complete
  POST   /api/learn/lessons/{lessonId}/uncomplete
  GET    /api/courses/{courseId}/completion

ADMIN                                         [Admin]
  GET    /api/admin/users
  POST   /api/admin/users/{userId}/grant-instructor
  POST   /api/admin/users/{userId}/revoke-instructor

OPS
  GET    /health/live
  GET    /health/ready
  GET    /openapi/v1.json
```
