import { Link, createFileRoute, redirect } from '@tanstack/react-router'

import { AuthCard } from '#/features/auth/components/auth-card'
import { RegisterForm } from '#/features/auth/components/register-form'
import { destinationFrom, redirectSearchSchema } from '#/features/auth/schemas'

/**
 * Create an account.
 *
 * Always a student. There is no instructor option here and there is no field that could become
 * one — the role is granted by an administrator (00 §5, and `A-6`), which is what stops
 * self-service from being the way someone gets publishing rights.
 */
export const Route = createFileRoute('/register')({
  validateSearch: redirectSearchSchema,

  beforeLoad: ({ context, search }) => {
    if (context.auth.signedIn) {
      throw redirect({ to: destinationFrom(search) })
    }
  },

  component: RegisterPage,
})

function RegisterPage() {
  const search = Route.useSearch()

  return (
    <AuthCard
      title="Create your account"
      description="Free, and it takes one form. You will be signed in as soon as it is created."
      footer={
        <>
          Already have an account?{' '}
          {/* Carries the destination across, and stays a bare /login when there isn't one. */}
          <Link
            to="/login"
            search={search}
            className="text-primary underline underline-offset-4"
          >
            Sign in
          </Link>
          .
        </>
      }
    >
      <RegisterForm destination={destinationFrom(search)} />
    </AuthCard>
  )
}
