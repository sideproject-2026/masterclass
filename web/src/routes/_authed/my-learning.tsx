import { createFileRoute } from '@tanstack/react-router'

/**
 * Placeholder. `C-7` (Sprint 22) builds the real My Learning page.
 *
 * It exists now because a guard with nothing behind it is a guard nobody has watched work —
 * the same reasoning that put an unstyled sign-in form on the index route in `A-3`, and that
 * is what found the sign-out defect. Deleting the body of this file is the whole of `C-7`'s
 * interaction with `A-5`.
 */
export const Route = createFileRoute('/_authed/my-learning')({
  component: MyLearning,
})

function MyLearning() {
  const { user } = Route.useRouteContext()

  return (
    <>
      <h1 className="font-heading text-2xl font-semibold tracking-tight">My Learning</h1>

      <p className="mt-2 text-sm text-muted-foreground">
        Signed in as {user.displayName}. Enrolled courses appear here from Sprint 22 (
        <code className="font-mono text-xs">C-7</code>).
      </p>
    </>
  )
}
