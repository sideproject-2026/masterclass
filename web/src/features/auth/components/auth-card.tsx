import type { ReactNode } from 'react'

/**
 * The frame the sign-in and register pages share.
 *
 * `max-w-sm` and nothing else clever: at 375px the card is the page, and on a desktop a form
 * of four short fields stretched across five columns is harder to read, not easier.
 */
export function AuthCard({
  title,
  description,
  children,
  footer,
}: {
  title: string
  description: string
  children: ReactNode
  footer: ReactNode
}) {
  return (
    <div className="mx-auto w-full max-w-sm py-8">
      <h1 className="font-heading text-2xl font-semibold tracking-tight">{title}</h1>
      <p className="mt-2 text-sm text-muted-foreground">{description}</p>

      <div className="mt-6">{children}</div>

      <p className="mt-6 text-sm text-muted-foreground">{footer}</p>
    </div>
  )
}

/**
 * Whatever the API said — a rejected sign-in, an address already registered.
 *
 * `role="alert"` because it appears after the visitor has acted: without it the message is
 * painted silently and a screen-reader user is left on a form that looks unchanged.
 */
export function FormError({ children }: { children: string }) {
  return (
    <p
      role="alert"
      className="rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm text-destructive"
    >
      {children}
    </p>
  )
}
