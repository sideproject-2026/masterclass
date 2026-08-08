import {
  clearSession,
  needsRefresh,
  readSession,
  sessionFromTokens,
  writeSession
  
} from './session'
import type {Session} from './session';

/**
 * Server-only API access.
 *
 * Everything that talks to Lms.Api goes through here, and this module must never be
 * imported from a component — that is what keeps the access token server-side once auth
 * lands (artifacts/design/04-adr-authentication.md §3). No `fetch` in a component, ever.
 */

/**
 * Resolves the API base URL from the environment the Aspire AppHost injects.
 *
 * `WithReference(api)` in AppHost.cs produces `services__api__http__0` (and `__https__0`).
 * Falling back to a local default keeps `npm run dev` usable without an AppHost.
 */
export function apiBaseUrl(): string {
  const fromAspire =
    process.env.services__api__https__0 ?? process.env.services__api__http__0

  const url = fromAspire ?? process.env.API_BASE_URL ?? 'http://localhost:5000'

  return url.replace(/\/+$/, '')
}

export type ApiHealth = {
  reachable: boolean
  status: string
  detail?: string
}

export type TokenResponse = {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

/** Unauthenticated call to the API. Used by login and register. */
export async function apiPost<T>(
  path: string,
  body: unknown,
): Promise<{ ok: true; data: T } | { ok: false; status: number; detail: string }> {
  
  const response = await fetch(`${apiBaseUrl()}${path}`, {
    method: 'POST',
    headers: { 'content-type': 'application/json', accept: 'application/json' },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    return {
      ok: false,
      status: response.status,
      detail: await problemDetail(response),
    }
  }

  return { ok: true, data: (await response.json()) as T }
}

/**
 * Calls the API with the caller's bearer token, refreshing first when the access token is
 * nearly expired.
 *
 * The token is read from the session cookie and attached here, on the server. It is never
 * returned to the browser — that is the entire reason this function exists rather than the
 * client calling the API directly.
 */
export async function apiFetch<T>(
  path: string,
  init: RequestInit = {},
): Promise<{ ok: true; data: T } | { ok: false; status: number; detail: string }> {
  
  let session = readSession()

  if (!session) {
    return { ok: false, status: 401, detail: 'Not signed in.' }
  }

  // Refresh before forwarding rather than after a 401: one round trip instead of two,
  // and the caller never sees a transient failure.
  if (needsRefresh(session)) {
    const refreshed = await refreshSession(session)
    if (!refreshed) {
      clearSession()
      return { ok: false, status: 401, detail: 'Session expired.' }
    }
    session = refreshed
  }

  const response = await fetch(`${apiBaseUrl()}${path}`, {
    ...init,
    headers: {
      ...init.headers,
      accept: 'application/json',
      authorization: `Bearer ${session.accessToken}`,
    },
  })

  if (response.status === 401) {
    // The token was rejected despite looking current — a rotated signing key, or a
    // revoked chain. Either way this session is finished.
    clearSession()
    return { ok: false, status: 401, detail: 'Session is no longer valid.' }
  }

  if (!response.ok) {
    return {
      ok: false,
      status: response.status,
      detail: await problemDetail(response),
    }
  }

  return {
    ok: true,
    data: (response.status === 204 ? undefined : await response.json()) as T,
  }
}

/** Exchanges the refresh token for a new pair and rewrites the cookie. */
async function refreshSession(session: Session): Promise<Session | null> {
  const response = await fetch(`${apiBaseUrl()}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify({ refreshToken: session.refreshToken }),
  })

  if (!response.ok) return null

  const next = sessionFromTokens((await response.json()) as TokenResponse)
  writeSession(next)
  return next
}

/** Pulls the human-readable part out of an RFC 9457 ProblemDetails body. */
async function problemDetail(response: Response): Promise<string> {
  try {
    const body = (await response.json()) as { detail?: string; title?: string }
    return body.detail ?? body.title ?? `${response.status} ${response.statusText}`
  } catch {
    return `${response.status} ${response.statusText}`
  }
}

/**
 * Calls the API's liveness probe from the server.
 *
 * This is the round trip that proves the BFF path works: the browser never contacts the
 * API directly, and never sees whatever credential this call will eventually carry.
 */
export async function fetchApiHealth(): Promise<ApiHealth> {
  const url = `${apiBaseUrl()}/health/live`

  try {
    const response = await fetch(url, {
      signal: AbortSignal.timeout(5000),
      headers: { accept: 'application/json' },
    })

    if (!response.ok) {
      return {
        reachable: false,
        status: 'Unhealthy',
        detail: `${response.status} ${response.statusText} from ${url}`,
      }
    }

    const body = (await response.json()) as { status?: string }

    return { reachable: true, status: body.status ?? 'Unknown' }
  } catch (error) {
    return {
      reachable: false,
      status: 'Unreachable',
      detail: error instanceof Error ? error.message : String(error),
    }
  }
}
