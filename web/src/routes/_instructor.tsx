import { Link, Outlet, createFileRoute, redirect } from '@tanstack/react-router'

import { Button } from '#/components/ui/button'
import { canUseStudio } from '#/features/auth/access'

/**
 * The instructor layout — everything under `/studio`.
 *
 * Two different failures, two different answers:
 *
 * - **Not signed in** → redirect to the login page, exactly as `_authed` does. There is
 *   something the visitor can do about it.
 * - **Signed in, not an instructor** → a 403 page, not a redirect. Sending an authenticated
 *   student to a login form is a loop: they would sign in successfully, come back, and be
 *   bounced again with no explanation.
 *
 * **UX, not security** (04 §3.1) — `/api/studio/*` carries the `Instructor` policy, and every
 * write inside it also checks `course.InstructorId == caller.Id`. Holding the role is not
 * authorisation to edit someone else's course, and no route guard could know the difference.
 */
export const Route = createFileRoute('/_instructor')({
  beforeLoad: ({ context, location }) => {
    if (!context.auth.signedIn) {
      throw redirect({ to: '/login', search: { redirect: location.href } })
    }

    return { user: context.auth.user }
  },

  component: InstructorLayout,
})

function InstructorLayout() {
  const { auth } = Route.useRouteContext()

  return canUseStudio(auth) ? <Outlet /> : <NotAnInstructor />
}

function NotAnInstructor() {
  return (
    <div className="mx-auto max-w-md py-12 text-center">
      <p className="font-mono text-xs text-muted-foreground">403</p>

      <h1 className="mt-2 font-heading text-2xl font-semibold tracking-tight">
        The Studio is for instructors
      </h1>

      <p className="mt-3 text-sm text-muted-foreground">
        Your account does not hold the instructor role. Accounts are granted it by an
        administrator — there is no way to self-register as an instructor.
      </p>

      <Button asChild variant="outline" size="sm" className="mt-6">
        <Link to="/">Back to courses</Link>
      </Button>
    </div>
  )
}
