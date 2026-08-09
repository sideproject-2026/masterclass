import { createServerFn } from '@tanstack/react-start'

import { apiFetch, apiPost } from './api'
import type { TokenResponse } from './api'
import { readSession, sessionFromTokens, writeSession } from './session'

/**
 * The authentication surface the browser is allowed to touch.
 *
 * Every function here runs on the server. Credentials go in, a cookie comes back, and the
 * returned shapes deliberately contain **no tokens** — see artifacts/design/04-adr-authentication.md §3.
 */

export type CurrentUser = {
  id: string
  email: string
  displayName: string
  roles: Array<string>
  instructorSlug: string | null
}

export type AuthState =
  | { signedIn: true; user: CurrentUser }
  | { signedIn: false }

export type AuthResult = { ok: true } | { ok: false; error: string }

/**
 * Exchanges credentials for a session cookie.
 *
 * Note what is returned: `{ ok }`, never the token pair. The tokens go straight into the
 * sealed cookie and the browser is told only whether it worked.
 */
export const login = createServerFn({ method: 'POST' })
  .validator((data: { email: string; password: string }) => data)
  .handler(async ({ data }): Promise<AuthResult> => {
    const result = await apiPost<TokenResponse>('/api/auth/login', {
      email: data.email,
      password: data.password,
    })

    if (!result.ok) {
      // The API returns one indistinguishable failure for unknown email, wrong password
      // and lockout. Pass it through unchanged — do not helpfully disambiguate it here.
      return { ok: false, error: result.detail }
    }

    writeSession(sessionFromTokens(result.data))
    return { ok: true }
  })

// Sign-out is not here. It is a document POST to the /sign-out route, so the browser
// applies the cookie removal on a real navigation — see web/src/routes/sign-out.tsx.

/**
 * The signed-in user, or a signed-out state.
 *
 * Never throws for an absent, expired or tampered cookie — those all mean "signed out",
 * which is a normal condition rather than an error.
 */
export const getCurrentUser = createServerFn({ method: 'GET' }).handler(
  async (): Promise<AuthState> => {
    if (!readSession()) {
      return { signedIn: false }
    }

    const result = await apiFetch<CurrentUser>('/api/me')

    return result.ok ? { signedIn: true, user: result.data } : { signedIn: false }
  },
)
