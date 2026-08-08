import { createFileRoute } from '@tanstack/react-router'
import { createServerFn } from '@tanstack/react-start'

import { apiBaseUrl, fetchApiHealth } from '../server/api'

/**
 * Runs on the server only. The browser calls this route, the Start server calls the API,
 * and the API URL never reaches the client — the shape every later data fetch will take
 * (artifacts/design/04-adr-authentication.md §3).
 */
const getApiHealth = createServerFn({ method: 'GET' }).handler(async () => {
  const health = await fetchApiHealth()
  return { ...health, baseUrl: apiBaseUrl() }
})

export const Route = createFileRoute('/')({
  loader: () => getApiHealth(),
  component: Home,
})

function Home() {
  const health = Route.useLoaderData()

  return (
    <main className="mx-auto max-w-2xl p-8">
      <h1 className="text-3xl font-bold tracking-tight">Masterclass</h1>
      <p className="text-muted-foreground mt-2 text-sm">
        Learning Management System — foundation slice
      </p>

      <section
        className="mt-8 rounded-lg border p-5"
        aria-labelledby="api-status-heading"
      >
        <h2 id="api-status-heading" className="text-sm font-semibold">
          API connectivity
        </h2>
        <p className="mt-1 text-xs opacity-70">
          Fetched server-side from <code>/health/live</code>. The browser never
          contacts the API directly.
        </p>

        <dl className="mt-4 grid grid-cols-[8rem_1fr] gap-y-2 text-sm">
          <dt className="font-medium">Status</dt>
          <dd>
            <span
              className={
                health.reachable
                  ? 'rounded bg-green-100 px-2 py-0.5 font-mono text-green-900'
                  : 'rounded bg-red-100 px-2 py-0.5 font-mono text-red-900'
              }
            >
              {health.status}
            </span>
          </dd>

          <dt className="font-medium">Resolved from</dt>
          <dd className="font-mono text-xs break-all">{health.baseUrl}</dd>

          {health.detail ? (
            <>
              <dt className="font-medium">Detail</dt>
              <dd className="font-mono text-xs break-all">{health.detail}</dd>
            </>
          ) : null}
        </dl>
      </section>
    </main>
  )
}
