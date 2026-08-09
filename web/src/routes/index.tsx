import { createFileRoute, useRouter } from '@tanstack/react-router'
import { useState } from 'react'

import { getCurrentUser, login } from '../server/auth'
import { getApiHealth } from '#/features/health/query'

export const Route = createFileRoute('/')({
  loader: async () => ({
    health: await getApiHealth(),
    auth: await getCurrentUser(),
  }),
  component: Home,
})

function Home() {
  const { health, auth } = Route.useLoaderData()
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
    <main className="mx-auto max-w-2xl p-8">
      <h1 className="text-3xl font-bold tracking-tight">Masterclass</h1>
      <p className="mt-2 text-sm opacity-70">
        Learning Management System — foundation slice
      </p>

      {/*
        Scaffolding. A-5 (Sprint 7) replaces this with the real login and register pages
        behind the design system from W-1. It exists so A-3's session layer is exercised
        rather than merely written — deliberately unstyled so nobody mistakes it for done.
      */}
      <section className="mt-8 rounded-lg border p-5">
        <h2 className="text-sm font-semibold">Session</h2>
        <p className="mt-1 text-xs opacity-70">
          Tokens are held in an HttpOnly cookie by the server. Nothing here is readable
          from browser JavaScript.
        </p>

        {auth.signedIn ? (
          <div className="mt-4 text-sm">
            <p>
              Signed in as <strong>{auth.user.displayName}</strong> ({auth.user.email})
            </p>
            <p className="mt-1 text-xs opacity-70">
              Roles: {auth.user.roles.join(', ') || 'none'}
            </p>
            {/* A real form POST, not an RPC — see routes/sign-out.tsx for why. */}
            <form method="post" action="/sign-out">
              <button type="submit" className="mt-3 rounded border px-3 py-1 text-sm">
                Sign out
              </button>
            </form>
          </div>
        ) : (
          <form onSubmit={onSignIn} className="mt-4 grid max-w-sm gap-2 text-sm">
            <input
              name="email"
              type="email"
              placeholder="email"
              required
              className="rounded border px-2 py-1"
            />
            <input
              name="password"
              type="password"
              placeholder="password"
              required
              className="rounded border px-2 py-1"
            />
            <button
              type="submit"
              disabled={busy}
              className="justify-self-start rounded border px-3 py-1"
            >
              Sign in
            </button>
            {error ? <p className="text-xs text-red-700">{error}</p> : null}
          </form>
        )}
      </section>

      <section className="mt-6 rounded-lg border p-5">
        <h2 className="text-sm font-semibold">API connectivity</h2>
        <p className="mt-1 text-xs opacity-70">
          Fetched server-side from <code>/health/live</code>.
        </p>
        <dl className="mt-4 grid grid-cols-[8rem_1fr] gap-y-2 text-sm">
          <dt className="font-medium">Status</dt>
          <dd className="font-mono text-xs">{health.status}</dd>
          <dt className="font-medium">Resolved from</dt>
          <dd className="font-mono text-xs break-all">{health.baseUrl}</dd>
        </dl>
      </section>
    </main>
  )
}
