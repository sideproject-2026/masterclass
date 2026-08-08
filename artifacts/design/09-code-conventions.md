# 09 — Code Conventions & Patterns

> The written standard we build against, both stacks. Referenced by [`CLAUDE.md`](../../CLAUDE.md).
> Architecture context: [`01`](01-architecture.md) · Domain: [`02`](02-domain-model.md) · API: [`03`](03-api-design.md)

---

## 1. Principles

Six rules. When two approaches look equally good, these decide.

1. **Rule of two.** Introduce an abstraction on the *second* instance, not the first. An interface with one implementation is indirection; with two it is a seam. If we reach for a pattern and cannot name the second case, we do not add it yet.
2. **Make illegal states unrepresentable.** A `Lesson` should not be able to exist with `Type = Video` and no video id. Enforce in the constructor and factory methods, not in a validator that runs later and hopes.
3. **Errors are values.** Expected failures return `Result<T>`. Exceptions are for programmer errors and infrastructure faults only — things where the correct response is a stack trace, not a 404.
4. **Boundaries are typed; interiors trust.** Parse and validate once, at the edge (HTTP, database, YouTube URL, form input). Past that boundary, types carry the guarantee and nothing re-checks.
5. **Depend on the narrowest thing.** A handler that needs one query takes that query's interface, not the whole `DbContext`, not a "service".
6. **One standard, two stacks.** Everything below applies to TypeScript as much as C#. Different syntax, same discipline.

---

## 2. The mediator — ours, and simpler than MediatR

### 2.1 Why write it

MediatR moved to commercial licensing, but that is not the main reason. The *value* MediatR delivers is **pipeline behaviours** — validation, logging, transactions applied uniformly across ~44 endpoints. The *cost* is a runtime service locator, reflection-based dispatch, and losing compile-time knowledge of which handler serves which request.

We can keep the value and drop the cost.

### 2.2 The design: handler interfaces + open-generic decorators

**No dispatcher. No `ISender`. No reflection at call time.** The endpoint injects the handler it needs; behaviours are DI decorators applied to all handlers at once.

```csharp
// Lms.SharedKernel/Messaging/Contracts.cs
public interface ICommand<TResponse>;
public interface IQuery<TResponse>;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken ct);
}

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken ct);
}
```

Commands and queries are **separate interfaces** — not because we are doing CQRS, but because the transaction behaviour must wrap commands and must not wrap queries. One decorator registration, correct by construction.

A handler:

```csharp
// Modules/Catalog/Features/PublishCourse/PublishCourseCommand.cs
public sealed record PublishCourseCommand(CourseId CourseId, UserId InstructorId)
    : ICommand<Unit>;

internal sealed class PublishCourseHandler(CatalogDbContext db, IClock clock)
    : ICommandHandler<PublishCourseCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(PublishCourseCommand cmd, CancellationToken ct)
    {
        var course = await db.Courses
            .Include(c => c.Chapters).ThenInclude(ch => ch.Lessons)
            .FirstOrDefaultAsync(c => c.Id == cmd.CourseId, ct);

        if (course is null)
            return CatalogErrors.CourseNotFound(cmd.CourseId);

        if (course.InstructorId != cmd.InstructorId)
            return CatalogErrors.NotCourseOwner;

        return await course.Publish(clock.UtcNow)          // returns Result — invariants live in the entity
            .TapAsync(_ => db.SaveChangesAsync(ct));
    }
}
```

The endpoint:

```csharp
group.MapPost("/courses/{id:guid}/publish",
    async (Guid id, ICommandHandler<PublishCourseCommand, Unit> handler,
           ClaimsPrincipal user, CancellationToken ct) =>
    {
        var result = await handler.HandleAsync(
            new PublishCourseCommand(new CourseId(id), user.GetUserId()), ct);
        return result.ToHttpResult();
    });
```

Injecting `ICommandHandler<PublishCourseCommand, Unit>` is no more typing than `ISender`, and the compiler now knows the handler exists. A missing registration is a startup failure, not a runtime one.

### 2.3 Pipeline behaviours as decorators

Behaviours are ordinary decorators, registered once as open generics:

```csharp
// Lms.SharedKernel/Messaging/ValidationBehavior.cs
internal sealed class ValidationBehavior<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken ct)
    {
        var failures = validators
            .Select(v => v.Validate(command))
            .SelectMany(r => r.Errors)
            .ToList();

        return failures.Count > 0
            ? Error.Validation(failures)
            : await inner.HandleAsync(command, ct);
    }
}
```

Registration — one line each, applying to every handler in the assembly:

```csharp
services.Scan(s => s.FromAssemblyOf<CatalogModule>()
    .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
    .AsImplementedInterfaces().WithScopedLifetime());

services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationBehavior<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(TransactionBehavior<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingBehavior<,>));
```

Decorators apply **outermost-last**: logging wraps transaction wraps validation wraps the handler. Order is explicit and readable, unlike a behaviour list ordered by registration accident.

Scrutor provides `Scan` and `Decorate`. If you would rather own that too, open-generic decoration is ~40 lines against `IServiceCollection` — a reasonable thing to write once in `SharedKernel`.

### 2.4 The four behaviours, and no more

| Behaviour | Applies to | Does |
|---|---|---|
| `ValidationBehavior` | commands | Runs FluentValidation validators; short-circuits to `Error.Validation`. |
| `TransactionBehavior` | commands | Opens a transaction on the Module's `DbContext`, commits on `Result.IsSuccess`, rolls back otherwise. **This is why commands and queries have separate interfaces.** |
| `LoggingBehavior` | both | Structured log + Activity span per request. Handler name, duration, outcome. |
| `IdempotencyBehavior` | commands marked `IIdempotent` | Only enrolment needs it today. Add when the second case appears (rule of two). |

**Not going through the mediator:** domain events. Those are `IEventBus` ([`01 §4`](01-architecture.md#4-module-isolation-rules)) — fire-and-forget, multiple subscribers, no return value. Conflating request/response with publish/subscribe is the most common way mediator libraries become unreadable. Two concepts, two abstractions.

---

## 3. Result and errors

```csharp
public readonly record struct Error(string Code, string Message, ErrorType Type)
{
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
    public static Error Invariant(string code, string message) => new(code, message, ErrorType.Invariant);
}

public enum ErrorType { Validation, NotFound, Conflict, Forbidden, Invariant }
```

Rules:

- **One error catalogue per Module**, as a static class: `CatalogErrors.CourseNotFound(id)`, `EnrollmentErrors.NotEnrolled`. Never an inline error string at a call site — you will want to find every use later.
- **`ErrorType` → HTTP status maps in exactly one place**, the `ToHttpResult()` extension. `Validation`→400, `Forbidden`→403, `NotFound`→404, `Conflict`→409, `Invariant`→422. Matches [`03 §1.2`](03-api-design.md#12-status-codes).
- **Implicit conversion `Error` → `Result<T>`** so handlers read as `return CatalogErrors.CourseNotFound(id);` with no ceremony.
- **No exceptions for expected failures.** A missing course is not exceptional. A `DbUpdateException` is.
- **`Result` composition:** `Map`, `Bind`, `Tap`, `Ensure`. Keep the set small — four combinators cover everything here. Resist building a monad library.

---

## 4. Value objects & strongly-typed IDs

This codebase passes a great many `Guid`s around, and `EnrollStudent(courseId, studentId)` compiles just as happily with the arguments swapped. That is a real bug class, and it is worth closing.

```csharp
public readonly record struct CourseId(Guid Value)
{
    public static CourseId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}
```

Domain value objects use private construction plus a factory returning `Result`:

```csharp
public readonly record struct Slug
{
    private Slug(string value) => Value = value;
    public string Value { get; }

    public static Result<Slug> Create(string input) =>
        SlugRegex().IsMatch(input)
            ? new Slug(input)
            : Error.Invariant("slug.invalid", $"'{input}' is not a valid slug.");

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex SlugRegex();
}
```

Conventions:

- `Create` returns `Result<T>` — never throws, never returns null.
- Implicit conversion **out** to the primitive; **never in**. Widening is safe; narrowing must go through `Create`.
- `[GeneratedRegex]`, never `new Regex(...)` at runtime.
- Boilerplate for EF converters, JSON converters, and route binding lives **once** in `SharedKernel` as generic base converters. Write it in Sprint 1 and never think about it again — this is the one place upfront investment clearly pays, because the alternative is per-type converters forever.
- Value objects to build: `CourseId`, `ChapterId`, `LessonId`, `UserId`, `EnrollmentId`, `Slug`, `VideoId`, `Email`.

---

## 5. Strategy

Where it earns its place — each has a real or imminent second implementation:

| Interface | Implementations | Second case |
|---|---|---|
| `IVideoProvider` | `YouTubeVideoProvider` | `CloudflareStreamVideoProvider` ([`05 §6`](05-adr-video-and-storage.md#6-consequences)) |
| `IEntitlementService` | `AlwaysAllowEntitlementService` | `SubscriptionEntitlementService` ([`02 §6`](02-domain-model.md#6-billing--modeled-not-built)) |
| `ISuggestionStrategy` | `SharedTagStrategy`, `SameInstructorStrategy`, `NextLevelStrategy` | Three from day one — a genuine chain |
| `IEmailSender` | `AcsEmailSender`, `ConsoleEmailSender` (dev) | Two from day one |

Selection by key where the strategy is chosen at runtime from data:

```csharp
services.AddKeyedScoped<IVideoProvider, YouTubeVideoProvider>(VideoProviderKind.YouTube);

// resolve by the value stored on the lesson
var provider = serviceProvider.GetRequiredKeyedService<IVideoProvider>(lesson.VideoProvider);
```

Keyed DI is built in — no factory class, no switch statement, no registry dictionary.

**Suggestions are a chain, not a switch.** Ordered strategies each contribute candidates until three are found ([`02 §4.4`](02-domain-model.md#44-suggestions-after-completion-r5)). Adding a fourth ranking rule is a new class and a registration.

**Not a strategy:** `Lesson.Type` (Video vs Reading). Two cases, both known, both stable, differing only in which fields are set. A discriminated shape with guard clauses in the entity is clearer than two subclasses and an EF inheritance mapping. Rule of two applies to *variation you expect to grow*, not to every branch.

---

## 6. Parsing

A parser is a function `string → Result<T>`. Never throws. Never returns null. Never has an out-parameter unless mirroring a framework idiom.

```csharp
public static class YouTubeUrlParser
{
    public static Result<VideoId> Parse(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return MediaErrors.VideoUrlEmpty;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return MediaErrors.VideoUrlMalformed(url);

        var id = uri.Host.TrimStart("www.") switch
        {
            "youtu.be"                  => uri.AbsolutePath.Trim('/'),
            "youtube.com" or
            "m.youtube.com" or
            "youtube-nocookie.com"      => ExtractFromYouTubeCom(uri),
            _                           => null
        };

        return VideoId.Create(id);   // final shape check lives in the value object
    }
}
```

Rules:

- **Parse at the boundary, once.** The endpoint parses; the handler receives a `VideoId`. No handler ever sees a raw URL string.
- **The parser identifies the shape; the value object validates it.** Two responsibilities, two places. `VideoId.Create` enforces `^[A-Za-z0-9_-]{11}$` regardless of which parser produced the candidate.
- **Every parser gets a table-driven test** listing all accepted forms from [`05 §2.2`](05-adr-video-and-storage.md#22-url-parsing) plus the rejections. Parsers are where cheap tests catch expensive bugs.

---

## 7. Mapping

**Mapperly** — source-generated, compile-time, no reflection, and you can read the output.

```csharp
[Mapper]
internal static partial class CourseMapper
{
    public static partial CourseSummaryDto ToSummary(Course course);
    public static partial CourseDetailDto ToDetail(Course course);
}
```

Rules:

- **One mapper per Module**, `internal static partial`. Mappers never cross a Module boundary.
- **Entity → DTO only. Never DTO → entity.** Entities are constructed through their own factory methods so invariants hold; a mapper that populates an entity from a request bypasses every guarantee the domain makes. This is the single most important rule in this section.
- **List projections do not use the mapper.** `Select(c => new CourseSummaryDto { ... })` inside the query, so EF generates a narrow `SELECT`. Mapping a materialised entity to a DTO fetches columns you then discard ([`02 §8.2`](02-domain-model.md#82-query-conventions)).
- If a mapping needs a condition, it is not a mapping — write a method.

---

## 8. Pagination

Three types, each with one job. Paging is implemented **once** in `SharedKernel` and never re-derived at a call site.

| Type | Layer | Holds |
|---|---|---|
| `PageRequest` | inbound | Validated, clamped `Page` + `PageSize` |
| `QueryResult<T>` | internal | `Data` + `TotalCount` — nothing else |
| `PagedResult<T>` | wire | The client-facing DTO from [`03 §1.1`](03-api-design.md#11-standard-envelopes) |

### 8.1 `PageRequest` — clamp, don't reject

```csharp
// Lms.SharedKernel/Pagination/PageRequest.cs
public readonly record struct PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize     = 50;

    private PageRequest(int page, int pageSize) => (Page, PageSize) = (page, pageSize);

    public int Page     { get; }
    public int PageSize { get; }
    public int Skip     => (Page - 1) * PageSize;

    public static PageRequest Of(int? page, int? pageSize) =>
        new(Math.Max(1, page ?? 1),
            Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize));
}
```

`?pageSize=5000` silently becomes 50 rather than returning a 400. For a public catalogue that is the right call — a nonsense query parameter should not be an error page, and the cap is enforced in the type so no endpoint can forget it.

### 8.2 `QueryResult<T>` — data and count, nothing more

```csharp
// Lms.SharedKernel/Pagination/QueryResult.cs
public sealed record QueryResult<T>(IReadOnlyList<T> Data, int TotalCount)
{
    public static QueryResult<T> Empty { get; } = new([], 0);

    public QueryResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new([.. Data.Select(map)], TotalCount);
}
```

It deliberately knows nothing about page numbers or page sizes — **the caller already knows what it asked for**. Keeping `Page`/`PageSize` out of it means a handler can be reused by something that pages differently (an export job, an internal Contracts query) without carrying meaningless fields.

> **Naming:** `Result<T>` signals success-or-error; `QueryResult<T>` is a successful page. They compose — a handler returns `Result<QueryResult<CourseSummaryDto>>`. Slightly awkward to read the first time, unambiguous thereafter.

### 8.3 One paging implementation

```csharp
// Lms.SharedKernel/Pagination/QueryableExtensions.cs
public static async Task<QueryResult<T>> ToQueryResultAsync<T>(
    this IQueryable<T> query, PageRequest page, CancellationToken ct)
{
    var total = await query.CountAsync(ct);
    if (total == 0) return QueryResult<T>.Empty;

    var data = await query.Skip(page.Skip).Take(page.PageSize).ToListAsync(ct);
    return new QueryResult<T>(data, total);
}
```

Two round trips, short-circuited when the count is zero. **No `Skip`/`Take` appears anywhere else in the codebase.**

### 8.4 The wire type

```csharp
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages)
{
    public static PagedResult<T> From(QueryResult<T> result, PageRequest page) =>
        new(result.Data,
            page.Page,
            page.PageSize,
            result.TotalCount,
            (int)Math.Ceiling(result.TotalCount / (double)page.PageSize));
}
```

`QueryResult.Data` becomes `PagedResult.Items` — the rename is deliberate, not an inconsistency. `Data` is the internal payload; `items` is the published contract, and the contract does not change because we renamed a field.

### 8.5 Using it

```csharp
public readonly record struct PagingParams(int? Page, int? PageSize)
{
    public PageRequest ToPageRequest() => PageRequest.Of(Page, PageSize);
}

group.MapGet("/courses", async (
    [AsParameters] PagingParams paging,
    [AsParameters] CourseFilter filter,
    IQueryHandler<BrowseCoursesQuery, QueryResult<CourseSummaryDto>> handler,
    CancellationToken ct) =>
{
    var page   = paging.ToPageRequest();
    var result = await handler.HandleAsync(new BrowseCoursesQuery(filter, page), ct);
    return result.ToHttpResult(qr => PagedResult<CourseSummaryDto>.From(qr, page));
});
```

```csharp
internal sealed class BrowseCoursesHandler(CatalogDbContext db)
    : IQueryHandler<BrowseCoursesQuery, QueryResult<CourseSummaryDto>>
{
    public async Task<Result<QueryResult<CourseSummaryDto>>> HandleAsync(
        BrowseCoursesQuery q, CancellationToken ct) =>
        await db.Courses
            .AsNoTracking()
            .Where(c => c.Status == CourseStatus.Published)
            .ApplyFilter(q.Filter)
            .ApplySort(q.Sort)
            .Select(c => new CourseSummaryDto { /* … */ })   // project BEFORE paging
            .ToQueryResultAsync(q.Page, ct);
}
```

### 8.6 Rules

1. **Always order before paging, and always with a tiebreaker.** `Skip`/`Take` over an unordered — or non-uniquely ordered — query is non-deterministic in PostgreSQL: rows can repeat across pages or vanish entirely. Sorting by `PublishedAt` alone is *not* unique. Every sort ends `.ThenBy(c => c.Id)`.
2. **Project before paging.** `Select` into the DTO first so both the count and the page run against the narrow shape.
3. **Sorting is an extension method, not a strategy.** `ApplySort` is a switch over three stable cases (`newest`, `popular`, `title`). Rule of two — three known branches that will not grow is a switch, not three classes.
4. **`ToQueryResultAsync` is the only paging code.** If you find `Skip(` outside `SharedKernel`, that is a bug.
5. **Empty pages are `200`, not `404`.** An empty result set is a successful query with no matches.

### 8.7 When this stops being enough

- **Two round trips per page.** Fine at this scale. PostgreSQL's `COUNT(*) OVER()` window function returns count and rows in one query — available if the count query ever shows up in traces, but it complicates the projection, so not the default.
- **Offset paging degrades at high offsets** — `OFFSET 10000` makes PostgreSQL walk 10,000 rows. Irrelevant for a catalogue of tens of courses; it would matter for `LessonProgress` if it were ever paged for a user. The escape hatch is keyset pagination, and it is a new overload of `ToQueryResultAsync` plus a cursor parameter — a change in one file, not a redesign.

---

## 9. Vertical slices

One folder per use case. Everything for that operation is in it.

```
Modules/Catalog/Features/PublishCourse/
├─ PublishCourseCommand.cs      request record
├─ PublishCourseHandler.cs      ICommandHandler implementation
├─ PublishCourseValidator.cs    FluentValidation
└─ PublishCourseEndpoint.cs     route registration
```

Rules:

- **Handlers are `internal sealed`.** Nothing outside the Module resolves them.
- **A handler does one thing.** If it needs a second `DbContext` or a second Module's data, it is two use cases or it needs a `*.Contracts` query.
- **No `Services/` folder.** A class named `CourseService` accumulates every operation touching courses and becomes the thing vertical slices exist to prevent. If shared logic appears in three handlers, extract it to the *entity* or to a named domain service with a specific job — never to a catch-all.
- **No `Helpers/`, no `Utils/`, no `Common/`.** These are folders named after not having decided. Name the thing after what it does.

---

## 10. Frontend conventions

Same principles. The vocabulary differs; the standard does not.

### 10.1 Structure — mirrors the backend

```
web/src/
├─ features/
│  ├─ catalog/       api.ts · hooks.ts · components/ · schemas.ts
│  ├─ studio/
│  ├─ learn/
│  └─ auth/
├─ components/ui/    shadcn — generated, do not hand-edit
├─ components/       shared app components (2+ features use it, else keep it local)
├─ lib/              result.ts · http.ts · format.ts
└─ routes/           TanStack Router file routes — thin
```

A feature folder mirrors a backend Module. If `features/` and `Modules/` drift apart, one of them is wrong.

### 10.2 The rules

- **Components render. Hooks decide.** A component with a `useEffect` containing business logic is a hook that has not been extracted yet. Components should read as a description of the markup.
- **Zod is the boundary; infer everything.** `type Course = z.infer<typeof CourseSchema>`. Never hand-write a type that duplicates a schema — they diverge silently, always.
- **Routes are thin.** A route file declares the loader, the guard, and renders one feature component. Business logic never lives in `routes/`.
- **Server functions are the only thing that talks to the API.** No `fetch` in a component, ever. This is also what keeps the access token server-side ([`04 §3`](04-adr-authentication.md#3-decision)).
- **`Result` on the client too.** A discriminated union — `{ ok: true, data } | { ok: false, error }` — mirrors the backend and makes error paths impossible to forget, because TypeScript will not let you read `.data` without narrowing.
- **Banned:** `any`, `as` (except `as const`), non-null `!`, default exports, prop drilling past one level, `useEffect` for data fetching (that is TanStack Query's job).
- **Strongly-typed ids here too:** `type CourseId = string & { readonly __brand: 'CourseId' }`. Same bug class, same fix, roughly two lines.

### 10.3 Frontend equivalents of the backend patterns

| Backend | Frontend |
|---|---|
| Strategy | A component registry: `const LESSON_VIEWS = { Video: VideoLesson, Reading: ReadingLesson }` — not an if-chain in the player |
| Parsing | Zod schemas at every boundary: API responses, form input, search params |
| Mapping | DTO → view model in `features/*/api.ts`. Components never receive a raw DTO. |
| Pipeline behaviour | Server-function middleware (auth, logging) — the same decorator idea |
| Value object | Branded types |
| `PagedResult<T>` | A generic Zod schema factory + one `usePagedQuery` hook — §10.4 |

### 10.4 Pagination on the client

One schema factory, one hook. Written once, used by the catalogue, My Learning, and Studio's course list.

```ts
// lib/pagination.ts
export const pagedResult = <T extends z.ZodTypeAny>(item: T) =>
  z.object({
    items:      z.array(item),
    page:       z.number().int().positive(),
    pageSize:   z.number().int().positive(),
    totalCount: z.number().int().nonnegative(),
    totalPages: z.number().int().nonnegative(),
  })

export type PagedResult<T> = { items: T[]; page: number; pageSize: number
                               totalCount: number; totalPages: number }

// the search-param contract, shared by every paged route
export const pageSearchSchema = z.object({
  page:     z.number().int().positive().catch(1),
  pageSize: z.number().int().min(1).max(50).catch(20),
})
```

Two rules that matter more than they look:

1. **Page state lives in the URL, not in `useState`.** TanStack Router validates search params with `pageSearchSchema`, so page 3 of a filtered catalogue is a shareable link and the back button behaves. Component state would break both, and it is the default thing people reach for.
2. **`placeholderData: keepPreviousData` on every paged query.** Without it the list unmounts and the layout jumps on each page change — the single most common way pagination feels broken while being technically correct.

```ts
export function useCourses(search: CourseSearch) {
  return useQuery({
    queryKey: ['courses', search],
    queryFn: () => fetchCourses(search),
    placeholderData: keepPreviousData,
  })
}
```

`.catch(1)` on the schema mirrors the backend's clamp-don't-reject stance (§8.1): `?page=banana` renders page 1 rather than an error boundary.

---

## 11. Testing

| Kind | Scope | Rule |
|---|---|---|
| Unit | Domain entities, value objects, parsers | No I/O, no mocks. If it needs a mock, it is an integration test. |
| Integration | Endpoint → real Postgres via Testcontainers | One per endpoint minimum, covering the success path and the primary failure. |
| Architecture | NetArchTest | Module boundaries ([`01 §4.1`](01-architecture.md#41-enforcement)). |
| E2E | Playwright | Two specs only: author→publish, enroll→complete. |

**Test names state behaviour:** `Publish_fails_when_a_chapter_has_no_lessons`. Not `PublishCourseTest2`.

**Do not mock what you own.** Mocking `CatalogDbContext` tests EF's fluent API, not your code. Use a real database.

---

## 12. Naming

| Thing | Convention | Example |
|---|---|---|
| Command / query | Verb + noun + suffix | `PublishCourseCommand`, `GetCourseBySlugQuery` |
| Handler | Request name + `Handler` | `PublishCourseHandler` |
| DTO | Noun + `Dto` | `CourseSummaryDto` |
| Domain event | Past tense | `CoursePublished`, `StudentEnrolled` |
| Error factory | Module + `Errors` | `CatalogErrors.CourseNotFound` |
| Interface | No `I` prefix on... nothing. Keep `I`. | `IVideoProvider` |
| React component | PascalCase, named export | `CourseCard` |
| Hook | `use` + noun | `useEnrollment` |
| Boolean | `is` / `has` / `can` | `isEnrolled`, `canPublish` |

Async methods end in `Async`. Cancellation tokens are named `ct` and are the last parameter, always.

---

## 13. What we are not doing

Named so their absence is a decision rather than an oversight.

| Not doing | Why |
|---|---|
| **Generic `IRepository<T>`** | `DbContext` is already a repository and a unit of work. Wrapping it removes LINQ composition and buys a test benefit that Testcontainers already provides. |
| **`CourseService` / anemic service layer** | Handlers *are* the service layer, scoped to one operation. A per-entity service is where cohesion goes to die. |
| **AutoMapper-style runtime mapping** | Reflection, silent misconfiguration, and a debugging experience with no code to step through. Mapperly generates code you can read. |
| **A `Common`/`Shared`/`Utils` project** | `SharedKernel` is deliberately narrow: `Result`, `Error`, ids, `IClock`, paging, messaging contracts. Anything else needs a real name. |
| **Base controller / base handler classes** | Inheritance for code reuse. Use composition — decorators for cross-cutting, extension methods for helpers. |
| **Reflection-based mediator dispatch** | §2.2 — decorators give the same benefit with compile-time safety. |
| **Interfaces on everything for testability** | Concrete classes are testable. Add an interface when there is a second implementation (rule of two). |
| **`dynamic`, `object`, or reflection in hot paths** | If it needs reflection, source generation probably does it better. |

---

## 14. When to break these rules

When following one produces worse code, and you can say why in a sentence. Write that sentence in a comment. A convention that cannot be broken with justification is not a convention; it is a cargo cult.

The exceptions to the exception — never broken without changing this document first:

- Entities are never constructed by a mapper (§7)
- Module boundaries (§13, [`01 §4`](01-architecture.md#4-module-isolation-rules))
- No access token reachable from browser JavaScript ([`04`](04-adr-authentication.md))
- The enrolment gate is enforced in the API, not the UI ([`03 §5`](03-api-design.md#5-learning--apilearn-and-enrollment--policy-student))
