import { describe, expect, it } from 'vitest'

import { canUseStudio, hasRole } from './access'
import { ROLES } from './schemas'
import type { AuthState, CurrentUser } from './schemas'

function signedIn(overrides: Partial<CurrentUser> = {}): AuthState {
  return {
    signedIn: true,
    user: {
      id: 'user-1',
      email: 'sam@example.com',
      displayName: 'Sam',
      roles: [ROLES.student],
      instructorSlug: null,
      ...overrides,
    },
  }
}

const signedOut: AuthState = { signedIn: false }

describe('hasRole', () => {
  it('is false for a signed-out visitor', () => {
    expect(hasRole(signedOut, ROLES.student)).toBe(false)
  })

  it('is true only for a role the user actually holds', () => {
    const admin = signedIn({ roles: [ROLES.student, ROLES.admin] })

    expect(hasRole(admin, ROLES.admin)).toBe(true)
    expect(hasRole(admin, ROLES.student)).toBe(true)
    expect(hasRole(admin, ROLES.instructor)).toBe(false)
  })
})

describe('canUseStudio', () => {
  it('offers the Studio to an instructor', () => {
    expect(canUseStudio(signedIn({ roles: [ROLES.student, ROLES.instructor] }))).toBe(true)
  })

  it('withholds it from a student', () => {
    expect(canUseStudio(signedIn())).toBe(false)
  })

  it('withholds it from an admin who is not also an instructor', () => {
    // Admin grants the role; it does not confer it. A-6's own live check found the seeded
    // admin missing Student for a related reason — role sets are not hierarchical here.
    expect(canUseStudio(signedIn({ roles: [ROLES.student, ROLES.admin] }))).toBe(false)
  })

  it('withholds it from a revoked instructor, who keeps their slug by design', () => {
    // The case that makes this a function rather than a truthiness check on instructorSlug.
    // A-6 keeps the profile and slug on revoke so course pages still name the author; 03 §3
    // still says the slug is the signal, and following it here would offer a Studio link
    // that answers 403.
    expect(
      canUseStudio(signedIn({ roles: [ROLES.student], instructorSlug: 'jane-doe' })),
    ).toBe(false)
  })

  it('withholds it from a signed-out visitor', () => {
    expect(canUseStudio(signedOut)).toBe(false)
  })
})
