import { Link, createFileRoute } from '@tanstack/react-router'

import { Markdown } from '#/components/markdown'
import { Button } from '#/components/ui/button'
import { getApiHealth } from '#/features/health/query'

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
            <Button asChild variant="outline" size="sm" className="mt-3">
              <Link to="/my-learning">My Learning</Link>
            </Button>
          </div>
        ) : (
          <div className="mt-4 flex flex-wrap gap-2">
            <Button asChild size="sm">
              <Link to="/register">Create an account</Link>
            </Button>
            <Button asChild variant="outline" size="sm">
              <Link to="/login">Sign in</Link>
            </Button>
          </div>
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
