import { Input } from '#/components/ui/input'
import { Label } from '#/components/ui/label'

/**
 * One labelled field with its validation message.
 *
 * The wiring is the reason this is a component rather than three lines repeated six times:
 * `aria-invalid` puts the input in its error state *and* announces it, and `aria-describedby`
 * is what makes a screen reader read the message out when focus lands. A red border alone
 * communicates nothing to anyone who cannot see it.
 */
export function AuthField({
  name,
  label,
  type = 'text',
  autoComplete,
  hint,
  error,
}: {
  name: string
  label: string
  type?: 'text' | 'email' | 'password'
  autoComplete?: string
  hint?: string
  error?: string
}) {
  const hintId = `${name}-hint`
  const errorId = `${name}-error`

  // Only the ids that exist — a dangling aria-describedby is read as nothing at all in some
  // screen readers, which silently loses the message it was meant to carry.
  const describedBy = [hint ? hintId : null, error ? errorId : null]
    .filter((id) => id !== null)
    .join(' ')

  return (
    <div className="grid gap-1.5">
      <Label htmlFor={name}>{label}</Label>

      <Input
        id={name}
        name={name}
        type={type}
        autoComplete={autoComplete}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy || undefined}
        // No `required`: the browser's own bubble would pre-empt the schema and show a
        // different message in a different place from every other error on the form.
      />

      {hint ? (
        <p id={hintId} className="text-xs text-muted-foreground">
          {hint}
        </p>
      ) : null}

      {error ? (
        <p id={errorId} className="text-xs text-destructive">
          {error}
        </p>
      ) : null}
    </div>
  )
}
