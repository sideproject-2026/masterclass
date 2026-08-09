import {
  createCipheriv,
  createDecipheriv,
  randomBytes,
  createHash,
} from 'node:crypto'
import { getRequestHeader, setResponseHeader } from '@tanstack/react-start/server'

/**
 * The session cookie — the whole point of the BFF.
 *
 * Tokens live here, on the server, and never reach browser JavaScript. An XSS on the
 * frontend cannot exfiltrate a bearer token because there isn't one to steal.
 * See artifacts/design/04-adr-authentication.md §3.
 *
 * This module is the ONLY place that touches the cookie. Nothing else reads or writes it.
 */

/**
 * The `__Host-` prefix is not cosmetic: it forbids a `Domain` attribute, so a compromised
 * subdomain cannot set a session cookie for the apex. It also requires Secure and Path=/.
 */
const COOKIE_NAME = '__Host-session'

/** Refresh this far before expiry, so a browsing user never meets an expired token. */
const REFRESH_THRESHOLD_MS = 5 * 60 * 1000

/**
 * Committed so `dotnet run` needs no setup, exactly as the JWT signing key is in A-1.
 * `sessionKey()` refuses to use it outside Development.
 */
const DEVELOPMENT_SECRET = 'DEVELOPMENT-ONLY-session-secret-do-not-ship-4c1f8a'

export type Session = {
  accessToken: string
  refreshToken: string
  /** Epoch milliseconds at which the access token expires. */
  expiresAt: number
}

function isDevelopment(): boolean {
  return (process.env.NODE_ENV ?? 'development') !== 'production'
}

/**
 * Derives a 32-byte key. SHA-256 of the secret rather than the raw bytes, so the secret
 * does not have to be exactly 32 characters to be usable.
 */
function sessionKey(): Buffer {
  const secret = process.env.SESSION_SECRET ?? DEVELOPMENT_SECRET

  if (!isDevelopment() && secret === DEVELOPMENT_SECRET) {
    throw new Error(
      'SESSION_SECRET is still the committed development value outside Development. ' +
        'Anyone with it can forge a session cookie for any user.',
    )
  }

  return createHash('sha256').update(secret).digest()
}

/**
 * AES-256-GCM: authenticated encryption. A tampered cookie fails to open rather than
 * decrypting into something an attacker chose — which is why this is not plain AES-CBC.
 */
function seal(session: Session): string {
  const iv = randomBytes(12)
  const cipher = createCipheriv('aes-256-gcm', sessionKey(), iv)

  const body = Buffer.concat([
    cipher.update(JSON.stringify(session), 'utf8'),
    cipher.final(),
  ])

  return Buffer.concat([iv, cipher.getAuthTag(), body]).toString('base64url')
}

function open(sealed: string): Session | null {
  try {
    const raw = Buffer.from(sealed, 'base64url')
    if (raw.length < 29) return null

    const decipher = createDecipheriv(
      'aes-256-gcm',
      sessionKey(),
      raw.subarray(0, 12),
    )
    decipher.setAuthTag(raw.subarray(12, 28))

    const json = Buffer.concat([
      decipher.update(raw.subarray(28)),
      decipher.final(),
    ]).toString('utf8')

    const parsed = JSON.parse(json) as Partial<Session>

    return typeof parsed.accessToken === 'string' &&
      typeof parsed.refreshToken === 'string' &&
      typeof parsed.expiresAt === 'number'
      ? (parsed as Session)
      : null
  } catch {
    // Tampered, truncated, or encrypted under a rotated key. Treat as signed out —
    // never surface a 500 for a bad cookie the caller can simply be asked to replace.
    return null
  }
}

export function readSession(): Session | null {
  const header = getRequestHeader('cookie')
  if (!header) return null

  for (const pair of header.split(/;\s*/)) {
    // Split on the FIRST '=' only. The sealed value is base64url and can itself contain
    // '=' padding; splitting on every '=' truncates it into something that will not open.
    const separator = pair.indexOf('=')
    if (separator === -1) continue

    if (pair.slice(0, separator) === COOKIE_NAME) {
      return open(pair.slice(separator + 1))
    }
  }

  return null
}

/**
 * The attribute set, shared by write and clear so the two cannot drift.
 *
 * A `__Host-` cookie can only be replaced or removed by a `Set-Cookie` that itself
 * satisfies the `__Host-` rules — Secure, `Path=/`, no `Domain`. Miss one and the browser
 * silently ignores the header.
 */
const COOKIE_ATTRIBUTES = 'HttpOnly; Secure; SameSite=Lax; Path=/'

/**
 * Written as a raw response header rather than through a cookie helper.
 *
 * This is the primitive TanStack Start's own authentication guide uses, and switching to it
 * is what fixed the A-3 sign-out defect: the helper's `Set-Cookie` was not surviving on the
 * response for every server-side entry point, so sign-out reported success while the browser
 * kept the cookie. A header we build ourselves has no such ambiguity.
 */
export function writeSession(session: Session): void {
  setResponseHeader(
    'Set-Cookie',
    // Max-Age matches the refresh-token lifetime; the access token inside expires far sooner.
    `${COOKIE_NAME}=${seal(session)}; ${COOKIE_ATTRIBUTES}; Max-Age=${14 * 24 * 60 * 60}`,
  )
}

/** Expires the cookie immediately. `Max-Age=0` is the removal instruction. */
export function clearSession(): void {
  setResponseHeader('Set-Cookie', `${COOKIE_NAME}=; ${COOKIE_ATTRIBUTES}; Max-Age=0`)
}

export function needsRefresh(session: Session, now = Date.now()): boolean {
  return session.expiresAt - now <= REFRESH_THRESHOLD_MS
}

/** Builds a session from an `/api/auth` token response. */
export function sessionFromTokens(
  tokens: { accessToken: string; refreshToken: string; expiresIn: number },
  now = Date.now(),
): Session {
  return {
    accessToken: tokens.accessToken,
    refreshToken: tokens.refreshToken,
    expiresAt: now + tokens.expiresIn * 1000,
  }
}
