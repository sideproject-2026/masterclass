import { Button } from '#/components/ui/button'
import { AuthField } from '#/features/auth/components/auth-field'
import { FormError } from '#/features/auth/components/auth-card'
import { useAuthForm } from '#/features/auth/hooks'
import { PASSWORD_MIN_LENGTH, registrationSchema } from '#/features/auth/schemas'
import type { Registration } from '#/features/auth/schemas'
import { register } from '#/server/auth'

export function RegisterForm({ destination }: { destination: string }) {
  const form = useAuthForm<Registration>({
    schema: registrationSchema,
    submit: register,
    destination,
  })

  return (
    <form onSubmit={form.onSubmit} noValidate className="grid gap-4">
      {form.formError ? <FormError>{form.formError}</FormError> : null}

      <AuthField
        name="displayName"
        label="Display name"
        autoComplete="name"
        hint="Shown on your profile and anywhere you appear to other people."
        error={form.fieldErrors.displayName}
      />

      <AuthField
        name="email"
        label="Email"
        type="email"
        autoComplete="username"
        error={form.fieldErrors.email}
      />

      {/*
        The policy is stated up front rather than sprung as an error after submitting. It is
        length only — the API has no composition rules (04 §3.1) and inventing some here would
        reject passwords the server would have accepted.
      */}
      <AuthField
        name="password"
        label="Password"
        type="password"
        autoComplete="new-password"
        hint={`At least ${PASSWORD_MIN_LENGTH} characters. Length is all that is required.`}
        error={form.fieldErrors.password}
      />

      <Button type="submit" disabled={form.pending}>
        {form.pending ? 'Creating account…' : 'Create account'}
      </Button>
    </form>
  )
}
