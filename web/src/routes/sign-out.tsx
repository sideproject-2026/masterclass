import { createFileRoute } from '@tanstack/react-router'

import { apiPost } from '#/server/api'
import { clearSession, readSession } from '#/server/session'

/**
 * Sign-out, as a document request rather than an RPC.
 *
 * A form POST followed by a 303 is an ordinary navigation, so the browser applies
 * `Set-Cookie` exactly as it does on any page load — and the whole client is rebuilt from
 * scratch, which is the correct outcome for sign-out. An RPC would leave the router's cached
 * loader data in memory and rely on an invalidation to clear it.
 *
 * It also works with JavaScript disabled, which is a reasonable property for the control that
 * ends a session on a shared machine.
 */
export const Route = createFileRoute('/sign-out')({
  server: {
    handlers: {
      POST: async () => {
        const session = readSession()

        if (session) {
          // Revoke server-side first. Clearing only the cookie would leave a usable refresh
          // token in the database for fourteen days.
          await apiPost('/api/auth/logout', { refreshToken: session.refreshToken })
        }

        clearSession()

        // 303, not 302: the redirect must be followed with GET, never by replaying the POST.
        return new Response(null, { status: 303, headers: { location: '/' } })
      },
    },
  },
})
