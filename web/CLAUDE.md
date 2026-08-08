# Frontend — `web/`

Adds to the root [CLAUDE.md](../CLAUDE.md). Full reasoning: [09-code-conventions.md §10](../artifacts/design/09-code-conventions.md).

TanStack Start (React 19) · TanStack Router + Query · Tailwind v4 · shadcn/ui · Zod.

---

## Where things go

```
src/features/<feature>/   api.ts · hooks.ts · schemas.ts · components/
src/components/ui/        shadcn — generated, never hand-edited
src/components/           shared only when 2+ features use it
src/lib/                  result.ts · pagination.ts · http.ts · format.ts
src/routes/               file routes — thin
```

Feature folders mirror backend Modules (`catalog`, `studio`, `learn`, `auth`). If they drift apart, one of them is wrong.

---

## The rules

- **Components render. Hooks decide.** A component holding business logic in a `useEffect` is an unextracted hook. Components should read as a description of markup.
- **Zod is the boundary.** Every API response, form, and search param is parsed. `type Course = z.infer<typeof CourseSchema>` — never hand-write a type that duplicates a schema; they diverge silently.
- **Server functions are the only thing that calls the API.** No `fetch` in a component, ever. This is also what keeps the access token server-side — if a token reaches client code, the BFF is broken.
- **Routes are thin.** A route file declares the loader, the guard, and renders one feature component. No business logic in `routes/`.
- **`Result` on the client too:** `{ ok: true; data } | { ok: false; error }`. TypeScript won't let you read `.data` without narrowing, which is the point.
- **Branded ids:** `type CourseId = string & { readonly __brand: 'CourseId' }`. Same bug class as the backend, same fix.

**Banned:** `any` · `as` (except `as const`) · non-null `!` · default exports · prop drilling past one level · `useEffect` for data fetching.

## Auth & routing

- Guards live on layout routes: `_authed.tsx`, `_instructor.tsx`, via `beforeLoad` returning the user into route context.
- **Guards are UX, not security.** Every one is backed by `.RequireAuthorization(...)` on the API. Never treat a hidden button as a control.
- Session is an `HttpOnly` cookie held by the Start server. Client code never sees a token.

## Data

- TanStack Query for everything. Router loaders dehydrate into the client cache.
- Paged queries: `pagedResult(itemSchema)` from `lib/pagination.ts`, and **`placeholderData: keepPreviousData`** — without it the list unmounts and the layout jumps on every page change.
- **Page state goes in the URL**, validated by `pageSearchSchema`, never `useState`. Page 3 of a filtered catalogue must be a shareable link with a working back button.
- `.catch(...)` on search-param schemas — `?page=banana` renders page 1, not an error boundary.

## Design system

- **Tokens only.** Colour, spacing, type, radius come from CSS variables set in `W-1`. No ad-hoc hex values, no one-off `mt-[13px]`.
- **Light and dark both work, always.** Verify before calling a card done.
- shadcn components are copied in and may be modified — but modify the copy, don't wrap it in another abstraction.
- Composition over props. A component with eight boolean props is three components.
- Radix gives keyboard nav and ARIA for free — don't replace a shadcn primitive with a bare `<div onClick>`.

**Studio UI is never polished.** shadcn defaults, forever. Only the public surface gets a design pass (Sprint 28). Adjusting Studio CSS is procrastination.

## Content safety

- All markdown renders through `rehype-sanitize` with a conservative allow-list. No raw HTML passthrough. Instructors are curated, not trusted.
- The YouTube embed takes a server-validated 11-character id. Never interpolate an unvalidated string into an iframe `src`.

## Definition of done for a UI card

- [ ] Works at 375px
- [ ] Works in light **and** dark
- [ ] Loading, empty, and error states exist — not just the happy path
- [ ] Keyboard reachable
- [ ] No `any`, no `!`, no console warnings
