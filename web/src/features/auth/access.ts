import { ROLES } from './schemas'
import type { AuthState, Role } from './schemas'

/**
 * Who may see what.
 *
 * Plain predicates over the auth state, deliberately free of React and of the router, so the
 * guards in `routes/_authed.tsx` and `routes/_instructor.tsx` stay one line each and the
 * decisions themselves are unit-testable without rendering anything.
 *
 * **These are UX, not security.** Every one of them is backed by `.RequireAuthorization(...)`
 * on the API (03 §7). A hidden link is a convenience; the 403 is the control.
 */

export function hasRole(auth: AuthState, role: Role): boolean {
  return auth.signedIn && auth.user.roles.includes(role)
}

/**
 * Whether to offer the Studio.
 *
 * Keyed on the **role**, not on `instructorSlug`. 03 §3 says the slug is the signal, and that
 * stopped being true in `A-6`: revoking an instructor deliberately keeps the profile and the
 * slug, so that course pages still name the author and nobody inherits a public URL. A revoked
 * instructor therefore still has a slug, and keying on it would show them a Studio link that
 * answers 403. Design doc corrected.
 */
export function canUseStudio(auth: AuthState): boolean {
  return hasRole(auth, ROLES.instructor)
}
