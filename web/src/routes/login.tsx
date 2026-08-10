import { Link, createFileRoute, redirect } from '@tanstack/react-router'

import { AuthCard } from '#/features/auth/components/auth-card'
import { SignInForm } from '#/features/auth/components/sign-in-form'
import { destinationFrom, redirectSearchSchema } from '#/features/auth/schemas'

/**
 * Sign in.
 *
 * Thin, as a route should be: validate the search params, bounce anyone already signed in, and
 * render one feature component. The form's logic lives in `features/auth`.
 */
export const Route = createFileRoute('/login')({
  // `?redirect=` arrives from the guards, and from anywhere else a visitor cares to type it.
  // The schema is where the open-redirect defence lives, so no caller has to remember it.
  validateSearch: redirectSearchSchema,

  beforeLoad: ({ context, search }) => {
    if (context.auth.signedIn) {
      throw redirect({ to: destinationFrom(search) })
    }
  },

  component: LoginPage,
})

function LoginPage() {
  const search = Route.useSearch()

  return (
    <AuthCard
      title="Sign in"
      description="Your session is held by the server in an HttpOnly cookie. No token ever reaches this page."
      footer={
        <>
          No account yet?{' '}
          {/* Carries the destination across, and stays a bare /register when there isn't one. */}
          <Link
            to="/register"
            search={search}
            className="text-primary underline underline-offset-4"
          >
            Create one
          </Link>
          .
        </>
      }
    >
      <SignInForm destination={destinationFrom(search)} />
    </AuthCard>
  )
}
