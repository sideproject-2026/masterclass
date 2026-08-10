import { useState } from 'react'
import { z } from 'zod'

import type { AuthResult } from '#/server/auth'

/**
 * The decision half of the sign-in and register forms.
 *
 * Components render; hooks decide. Both forms do the same four things — parse the fields,
 * call a server function, show whatever came back, and leave on success — so they share one
 * hook rather than two nearly-identical `onSubmit` handlers. That is the rule of two: the
 * abstraction arrives with the second case, not the first.
 */

export type FieldErrors<TValues> = Partial<Record<keyof TValues & string, string>>

export type AuthFormState<TValues> = {
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void
  pending: boolean
  /** The API's answer — a rejected login, a taken email. Not a per-field problem. */
  formError: string | null
  fieldErrors: FieldErrors<TValues>
}

export function useAuthForm<TValues extends Record<string, unknown>>({
  schema,
  submit,
  destination,
}: {
  schema: z.ZodType<TValues>
  submit: (input: { data: TValues }) => Promise<AuthResult>
  destination: string
}): AuthFormState<TValues> {
  const [pending, setPending] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors<TValues>>({})

  function onSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const fields = Object.fromEntries(new FormData(event.currentTarget))
    const parsed = schema.safeParse(fields)

    if (!parsed.success) {
      // First message per field. A list under one input is noise — fix the first problem and
      // the next one, if there still is one, appears on the next submit.
      const flattened = z.flattenError(parsed.error).fieldErrors
      const next: FieldErrors<TValues> = {}

      for (const [field, messages] of Object.entries(flattened)) {
        const first = messages?.[0]
        if (first !== undefined) {
          Object.assign(next, { [field]: first })
        }
      }

      setFieldErrors(next)
      setFormError(null)
      return
    }

    setFieldErrors({})
    setFormError(null)
    setPending(true)

    void submit({ data: parsed.data }).then((result) => {
      if (result.ok) {
        // A document navigation, not router.navigate — the same choice as sign-out, for the
        // same two reasons. The identity behind every cached loader has just changed, and a
        // full navigation rebuilds that state instead of relying on an invalidation to catch
        // all of it. It is also the honest way to reach `destination`: that is a validated
        // path this app produced, not one of the router's known route literals, and the
        // alternative is casting a string into a typed union.
        window.location.assign(destination)
        return
      }

      setPending(false)
      setFormError(result.error)
    })
  }

  return { onSubmit, pending, formError, fieldErrors }
}
