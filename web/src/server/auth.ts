import { createServerFn } from '@tanstack/react-start'

import {
  credentialsSchema,
  currentUserSchema,
  registrationSchema,
} from '#/features/auth/schemas'
import type {
  AuthState,
  Credentials,
  Registration,
} from '#/features/auth/schemas'
import { apiFetch, apiPost } from './api'
import type { TokenResponse } from './api'
import { readSession, sessionFromTokens, writeSession } from './session'

/**
 * The authentication surface the browser is allowed to touch.
 *
 * Every function here runs on the server. Credentials go in, a cookie comes back, and the
 * returned shapes deliberately contain **no tokens** — see artifacts/design/04-adr-authentication.md §3.
 */

export type AuthResult = { ok: true } | { ok: false; error: string }

/**
 * Exchanges credentials for a session cookie.
 *
 * Note what is returned: `{ ok }`, never the token pair. The tokens go straight into the
 * sealed cookie and the browser is told only whether it worked.
 */
export const login = createServerFn({ method: 'POST' })
  .validator((data: Credentials) => credentialsSchema.parse(data))
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

/**
 * Creates a student account and signs it in.
 *
 * The sign-in is a second server-side call, invisible to the browser: the credentials are
 * already in hand, and sending someone who just proved them to a login form to type them again
 * is friction with nothing behind it. If registration succeeds and the sign-in somehow does
 * not, the account still exists — so that path reports success and lets the login page handle
 * it, rather than implying the registration failed and inviting a duplicate attempt.
 */
export const register = createServerFn({ method: 'POST' })
  .validator((data: Registration) => registrationSchema.parse(data))
  .handler(async ({ data }): Promise<AuthResult> => {
    const created = await apiPost('/api/auth/register', {
      email: data.email,
      password: data.password,
      displayName: data.displayName,
    })

    if (!created.ok) {
      // 409 for a taken address, 400 for the password policy. Both are safe to show: the
      // caller supplied the address, so this tells them nothing they did not already know.
      return { ok: false, error: created.detail }
    }

    const tokens = await apiPost<TokenResponse>('/api/auth/login', {
      email: data.email,
      password: data.password,
    })

    if (tokens.ok) {
      writeSession(sessionFromTokens(tokens.data))
    }

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

    const result = await apiFetch<unknown>('/api/me')

    if (!result.ok) {
      return { signedIn: false }
    }

    // Parsed, not cast. The guards branch on `roles`; a payload whose shape drifted would
    // otherwise reach them as `undefined` and quietly decide access. Failing to parse means
    // signed out, which fails closed.
    const user = currentUserSchema.safeParse(result.data)

    return user.success ? { signedIn: true, user: user.data } : { signedIn: false }
  },
)
