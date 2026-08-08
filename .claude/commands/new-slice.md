---
description: Scaffold a vertical slice (command or query) following the project conventions
argument-hint: <Module> <UseCaseName> <command|query> [--paged]
allowed-tools: Read, Write, Edit, Glob, Grep
---

Scaffold a new vertical slice.

**Arguments:** `$ARGUMENTS`
- `$1` — Module: `Catalog` | `Enrollment` | `Identity` | `Media` | `Notifications`
- `$2` — UseCaseName in PascalCase, verb + noun: `PublishCourse`, `BrowseCourses`
- `$3` — `command` (writes) or `query` (reads)
- `--paged` — query only; returns `QueryResult<T>`

If any of `$1`–`$3` is missing or invalid, stop and ask. Do not guess the kind — a write registered as a query silently loses its transaction decorator.

## Before generating

1. Check `src/Modules/Lms.Modules.$1/Features/$2/` — if it exists, stop and report.
2. Read the endpoint's contract in `artifacts/design/03-api-design.md`. Route, auth policy, request/response shape, and status codes are **already specified**. Use them. If the spec is missing or wrong, say so and stop — we change the spec first, not the code.
3. Skim `src/CLAUDE.md` if the conventions aren't already in context.

## Files to create

All in `src/Modules/Lms.Modules.$1/Features/$2/`, namespace `Lms.Modules.$1.Features.$2`.

### `$2Command.cs` / `$2Query.cs`

```csharp
public sealed record $2Command(/* typed ids and value objects — never raw Guid or string */)
    : ICommand<TResponse>;
```

- Typed ids only: `CourseId`, `UserId`, `LessonId`. Raw `Guid` is converted at the endpoint.
- Response is `Unit` for commands with no return value.
- Queries: `IQuery<TResponse>`; with `--paged`, `IQuery<QueryResult<TDto>>`.

### `$2Handler.cs`

```csharp
internal sealed class $2Handler($1DbContext db, IClock clock)
    : ICommandHandler<$2Command, TResponse>
{
    public async Task<Result<TResponse>> HandleAsync($2Command command, CancellationToken ct)
    {
        // 1. load  2. authorise (ownership / enrolment)  3. mutate via entity methods  4. save
        throw new NotImplementedException();
    }
}
```

- `internal sealed`, primary constructor, `ct` last.
- Leave the body as `NotImplementedException` with the numbered comment. Do not invent business logic — that's the card's actual work.
- Queries: `AsNoTracking()`, project to a DTO **inside** the query, `AsSplitQuery()` if two collection levels.
- Paged: order with a unique tiebreaker, then `.ToQueryResultAsync(query.Page, ct)`. No `Skip`/`Take` here.
- Ownership (`InstructorId == caller`) and enrolment checks are guard clauses in the handler, returning an `Error`.

### `$2Validator.cs` — commands only

```csharp
internal sealed class $2Validator : AbstractValidator<$2Command>
{
    public $2Validator() { /* shape only */ }
}
```

Shape and format only. **Business invariants belong in the entity**, not here.

### `$2Endpoint.cs`

```csharp
internal static class $2Endpoint
{
    public static IEndpointRouteBuilder Map$2(this IEndpointRouteBuilder app)
    {
        app.MapPost("<route from 03-api-design.md>", async (
            /* route/body params */,
            ICommandHandler<$2Command, TResponse> handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new $2Command(/* … */), ct);
            return result.ToHttpResult();
        })
        .WithName("$2")
        .WithTags("<group>")
        .Produces<TResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }
}
```

- Route, verb, tag, and status codes come from the API design doc — do not improvise them.
- Paged endpoints take `[AsParameters] PagingParams paging`, build a `PageRequest`, and convert: `result.ToHttpResult(qr => PagedResult<TDto>.From(qr, page))`.
- Auth is on the route group, not the endpoint — unless this endpoint's rule differs (e.g. preview lessons allow anonymous).

### Register it

Add `.Map$2()` to `src/Modules/Lms.Modules.$1/Endpoints/$1Endpoints.cs`, in the correct route group.

Handlers and validators are picked up by assembly scanning in `$1Module.cs` — **do not add a manual DI registration**. If scanning isn't wired up yet, add it there rather than registering this one type.

### `tests/Lms.IntegrationTests/$1/$2Tests.cs`

One failing test per acceptance criterion on the card. Names state behaviour:

```csharp
[Fact] public async Task Publish_fails_when_a_chapter_has_no_lessons() { }
[Fact] public async Task Publish_returns_403_when_caller_is_not_the_owner() { }
```

Real Postgres via Testcontainers. No mocking of `DbContext`.

## After generating

Report:
1. Files created, as clickable paths.
2. The route and auth policy you took from the API design doc.
3. **A checklist of what's still to implement** — handler body, validator rules, test bodies.
4. Anything in the API spec that was ambiguous or absent.

Do not run the build — the slice is intentionally incomplete.
