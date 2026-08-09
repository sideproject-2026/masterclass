import { createFileRoute, useRouter } from '@tanstack/react-router'
import { useState } from 'react'

import { Markdown } from '#/components/markdown'
import { Button } from '#/components/ui/button'
import { getApiHealth } from '#/features/health/query'
import { login } from '#/server/auth'

export const Route = createFileRoute('/')({
  loader: async () => ({ health: await getApiHealth() }),
  component: Home,
})

/** Proves the sanitising renderer is wired up. Replaced by real content in the Studio cards. */
const sampleMarkdown = `Rendered through \`rehype-sanitize\` with a conservative allow-list —
**bold**, \`code\`, and [links](https://example.com) survive; raw HTML does not.`

function Home() {
  const { health } = Route.useLoaderData()
  const { auth } = Route.useRouteContext()

  return (
    <>
      <h1 className="font-heading text-3xl font-semibold tracking-tight">Masterclass</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        Learning Management System — foundation slice
      </p>

      <section className="mt-8 rounded-lg border p-5">
        <h2 className="font-heading text-sm font-semibold">Session</h2>
        <p className="mt-1 text-xs text-muted-foreground">
          Tokens are held in an HttpOnly cookie by the server. Nothing here is readable from
          browser JavaScript.
        </p>

        {auth.signedIn ? (
          <div className="mt-4 text-sm">
            <p>
              Signed in as <strong>{auth.user.displayName}</strong> ({auth.user.email})
            </p>
            <p className="mt-1 text-xs text-muted-foreground">
              Roles: {auth.user.roles.join(', ') || 'none'}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">
              Sign out is in the header.
            </p>
          </div>
        ) : (
          <SignInForm />
        )}
      </section>

      <section className="mt-6 rounded-lg border p-5">
        <h2 className="font-heading text-sm font-semibold">Markdown rendering</h2>
        <div className="mt-3 text-sm">
          <Markdown>{sampleMarkdown}</Markdown>
        </div>
      </section>

      <section className="mt-6 rounded-lg border p-5">
        <h2 className="font-heading text-sm font-semibold">API connectivity</h2>
        <p className="mt-1 text-xs text-muted-foreground">
          Fetched server-side from <code>/health/live</code>.
        </p>
        <dl className="mt-4 grid grid-cols-[7rem_1fr] gap-y-2 text-sm">
          <dt className="font-medium">Status</dt>
          <dd className="font-mono text-xs">{health.status}</dd>
          <dt className="font-medium">Resolved from</dt>
          <dd className="font-mono text-xs break-all">{health.baseUrl}</dd>
        </dl>
      </section>
    </>
  )
}

/*
  Scaffolding. A-5 (Sprint 7) replaces this with the real sign-in and register pages. It exists
  so A-3's session layer is exercised rather than merely written — and it is what found the
  sign-out defect.
*/
function SignInForm() {
  const router = useRouter()
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  async function onSignIn(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setBusy(true)
    setError(null)

    const form = new FormData(event.currentTarget)
    const result = await login({
      data: {
        email: String(form.get('email') ?? ''),
        password: String(form.get('password') ?? ''),
      },
    })

    setBusy(false)
    if (result.ok) {
      await router.invalidate()
    } else {
      setError(result.error)
    }
  }

  return (
    <form onSubmit={onSignIn} className="mt-4 grid max-w-sm gap-2 text-sm">
      <input
        name="email"
        type="email"
        placeholder="email"
        required
        className="rounded-md border bg-background px-2 py-1"
      />
      <input
        name="password"
        type="password"
        placeholder="password"
        required
        className="rounded-md border bg-background px-2 py-1"
      />
      <Button type="submit" size="sm" disabled={busy} className="justify-self-start">
        Sign in
      </Button>
      {error ? <p className="text-xs text-destructive">{error}</p> : null}
    </form>
  )
}
