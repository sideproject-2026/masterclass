import { Button } from '#/components/ui/button'
import { AuthField } from '#/features/auth/components/auth-field'
import { FormError } from '#/features/auth/components/auth-card'
import { useAuthForm } from '#/features/auth/hooks'
import { credentialsSchema } from '#/features/auth/schemas'
import type { Credentials } from '#/features/auth/schemas'
import { login } from '#/server/auth'

export function SignInForm({ destination }: { destination: string }) {
  const form = useAuthForm<Credentials>({
    schema: credentialsSchema,
    submit: login,
    destination,
  })

  return (
    <form onSubmit={form.onSubmit} noValidate className="grid gap-4">
      {form.formError ? <FormError>{form.formError}</FormError> : null}

      <AuthField
        name="email"
        label="Email"
        type="email"
        autoComplete="username"
        error={form.fieldErrors.email}
      />

      {/*
        `current-password`, not `new-password` — it is what tells a password manager to offer
        the saved credential here and to offer to save a new one on the register form.
      */}
      <AuthField
        name="password"
        label="Password"
        type="password"
        autoComplete="current-password"
        error={form.fieldErrors.password}
      />

      <Button type="submit" disabled={form.pending}>
        {form.pending ? 'Signing in…' : 'Sign in'}
      </Button>
    </form>
  )
}
