import { z } from 'zod'

/**
 * The auth boundary.
 *
 * Every shape crossing into or out of this feature is parsed here — form input on the way in,
 * the `/api/me` payload on the way back. Types are inferred from the schemas and never written
 * alongside them: a hand-written duplicate diverges silently (09 §10.2).
 */

/** The API's policy, mirrored: length only, no composition rules (04 §3.1). */
export const PASSWORD_MIN_LENGTH = 10

/** `AppUser.DisplayNameMaxLength`. Rejecting here saves a round trip to the same answer. */
const DISPLAY_NAME_MAX_LENGTH = 100

export const ROLES = {
  student: 'Student',
  instructor: 'Instructor',
  admin: 'Admin',
} as const

export type Role = (typeof ROLES)[keyof typeof ROLES]

const emailField = z.email({ error: 'Enter a valid email address.' })

export const credentialsSchema = z.object({
  email: emailField,
  /**
   * Deliberately *not* the registration policy.
   *
   * A sign-in form must never reject a password for being too short. It would publish the
   * policy to anyone who opens the page, and it would lock out any account whose password
   * predates a future policy change. Password length is the API's business, and its answer is
   * one indistinguishable 401 (03 §3) — the property that stops this endpoint being a
   * user-enumeration oracle. Non-empty is all this schema checks.
   */
  password: z.string().min(1, { error: 'Enter your password.' }),
})

export type Credentials = z.infer<typeof credentialsSchema>

export const registrationSchema = z.object({
  email: emailField,
  displayName: z
    .string()
    .trim()
    .min(1, { error: 'Enter a display name.' })
    .max(DISPLAY_NAME_MAX_LENGTH, {
      error: `Use ${DISPLAY_NAME_MAX_LENGTH} characters or fewer.`,
    }),
  password: z.string().min(PASSWORD_MIN_LENGTH, {
    error: `Use at least ${PASSWORD_MIN_LENGTH} characters.`,
  }),
})

export type Registration = z.infer<typeof registrationSchema>

/**
 * The `/api/me` payload (03 §3).
 *
 * Parsed rather than cast. The guards downstream branch on `roles`, so a payload that quietly
 * changed shape would not throw here — it would silently grant or withhold access. A parse
 * failure is treated as signed out, which fails closed.
 */
export const currentUserSchema = z.object({
  id: z.string(),
  email: z.string(),
  displayName: z.string(),
  roles: z.array(z.string()),
  instructorSlug: z.string().nullable(),
})

export type CurrentUser = z.infer<typeof currentUserSchema>

/**
 * What the root route puts in router context.
 *
 * A union rather than `user: CurrentUser | null`, so nothing can read `.user` without having
 * proved `signedIn` first — the same reason the backend returns `Result<T>`.
 */
export type AuthState = { signedIn: true; user: CurrentUser } | { signedIn: false }

/** C0 controls and DEL. The URL parser strips these *before* resolving, so they can hide a host. */
function hasControlCharacter(value: string): boolean {
  return Array.from(value).some((character) => {
    const code = character.codePointAt(0) ?? 0
    return code <= 0x1f || code === 0x7f
  })
}

/**
 * Whether a post-sign-in destination is safe to navigate to.
 *
 * Only a same-origin absolute path is ever accepted. `?redirect=https://evil.example` on an
 * otherwise genuine login link is the classic open redirect: the victim signs in on the real
 * site, is handed straight to the attacker's page, and every signal they were taught to check
 * — the domain, the padlock, the fact that they authenticated a second ago — was true.
 *
 * The rejected forms all *look* like paths, which is the point of them:
 *   `//evil.example`   protocol-relative; the browser reads it as an absolute URL
 *   `/\evil.example`   the same trick with a backslash, which browsers normalise to `/`
 *   `/{CR}{LF}evil…`   control characters, stripped by the URL parser before it resolves
 */
export function isSafeRedirect(target: string): boolean {
  return (
    target.startsWith('/') &&
    !target.startsWith('//') &&
    !target.startsWith('/\\') &&
    !hasControlCharacter(target)
  )
}

/**
 * `?redirect=` on the login and register routes.
 *
 * `.catch` rather than a rejection: a malformed or hostile value renders the home page, it does
 * not raise an error boundary. Same clamp-don't-reject stance as paging (09 §10.4).
 *
 * Optional, not defaulted. A required param would make `<Link to="/login">` a type error in the
 * header, and a `.default('/')` would make the router *materialise* the default: every visit to
 * `/login` answered 307 to `/login?redirect=%2F` before rendering anything. Absent stays absent;
 * `destinationFrom` supplies the fallback at the point of use.
 */
export const redirectSearchSchema = z.object({
  redirect: z.string().refine(isSafeRedirect).catch('/').optional(),
})

export type RedirectSearch = z.infer<typeof redirectSearchSchema>

/** Where to land after signing in. One place, so no caller can forget the fallback. */
export function destinationFrom(search: RedirectSearch): string {
  return search.redirect ?? '/'
}
