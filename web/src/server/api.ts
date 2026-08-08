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
