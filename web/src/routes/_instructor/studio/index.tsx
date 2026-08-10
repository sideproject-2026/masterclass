import { createFileRoute } from '@tanstack/react-router'

/**
 * Placeholder. `S-1` (Sprint 10) builds the real Studio course list.
 *
 * Here for the same reason as `_authed/my-learning.tsx`: it is what makes the `_instructor`
 * guard something that has been seen to work, in both directions, rather than asserted.
 *
 * Studio UI is never polished — shadcn defaults, forever.
 */
export const Route = createFileRoute('/_instructor/studio/')({
  component: StudioHome,
})

function StudioHome() {
  const { user } = Route.useRouteContext()

  return (
    <>
      <h1 className="font-heading text-2xl font-semibold tracking-tight">Studio</h1>

      <p className="mt-2 text-sm text-muted-foreground">
        {user.displayName} — instructor
        {user.instructorSlug ? ` (${user.instructorSlug})` : null}. Your courses appear here
        from Sprint 10 (<code className="font-mono text-xs">S-1</code>).
      </p>
    </>
  )
}
