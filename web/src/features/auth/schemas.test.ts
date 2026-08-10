import { describe, expect, it } from 'vitest'

import {
  credentialsSchema,
  currentUserSchema,
  destinationFrom,
  isSafeRedirect,
  redirectSearchSchema,
  registrationSchema,
} from './schemas'

/**
 * The open-redirect defence is the reason this file exists.
 *
 * It is a security control with no visible symptom when it breaks: the login page keeps
 * working, and the only sign of failure is that somebody's session ends up on a page they did
 * not mean to be on. Exactly the class of thing that regresses silently, which is why `W-1`
 * put a test runner here in the first place.
 */
describe('isSafeRedirect', () => {
  it.each(['/', '/my-learning', '/studio/courses/7?tab=curriculum', '/a/b/c#anchor'])(
    'accepts the same-origin path %j',
    (target) => {
      expect(isSafeRedirect(target)).toBe(true)
    },
  )

  it.each([
    ['an absolute http URL', 'http://evil.example/'],
    ['an absolute https URL', 'https://evil.example/'],
    ['a protocol-relative URL', '//evil.example/'],
    ['a backslash protocol-relative URL', '/\\evil.example/'],
    ['a backslash-only path', '\\evil.example'],
    ['a javascript: URL', 'javascript:alert(1)'],
    ['a data: URL', 'data:text/html,<script>alert(1)</script>'],
    ['a bare relative path', 'my-learning'],
    ['the empty string', ''],
  ])('rejects %s', (_, target) => {
    expect(isSafeRedirect(target)).toBe(false)
  })

  it('rejects a path carrying a CR or LF, which the URL parser strips before resolving', () => {
    expect(isSafeRedirect('/\r\n/evil.example')).toBe(false)
    expect(isSafeRedirect('/\tevil')).toBe(false)
  })
})

describe('redirectSearchSchema', () => {
  it('keeps a safe destination', () => {
    expect(redirectSearchSchema.parse({ redirect: '/my-learning' })).toEqual({
      redirect: '/my-learning',
    })
  })

  it('falls back to the home page rather than raising, for a hostile destination', () => {
    expect(redirectSearchSchema.parse({ redirect: 'https://evil.example' })).toEqual({
      redirect: '/',
    })
  })

  it('falls back for a non-string, which is what ?redirect=a&redirect=b produces', () => {
    expect(redirectSearchSchema.parse({ redirect: ['/a', '/b'] })).toEqual({ redirect: '/' })
  })

  it('leaves an absent parameter absent, so /login is not rewritten to /login?redirect=%2F', () => {
    // A `.default('/')` here made the router materialise the value: every visit to the login
    // page answered a 307 to itself before rendering. The fallback belongs at the read site.
    expect(redirectSearchSchema.parse({})).toEqual({})
  })
})

describe('destinationFrom', () => {
  it('passes a destination through', () => {
    expect(destinationFrom({ redirect: '/studio' })).toBe('/studio')
  })

  it('falls back to the home page when there is none', () => {
    expect(destinationFrom({})).toBe('/')
  })
})

describe('credentialsSchema', () => {
  it('accepts a short password — length is the API\'s business, not the form\'s', () => {
    // Enforcing the registration policy here would publish it, and would lock out any account
    // whose password predates a policy change. The API answers one indistinguishable 401.
    expect(credentialsSchema.safeParse({ email: 'a@b.co', password: 'short' }).success).toBe(
      true,
    )
  })

  it('rejects an empty password before spending a request on it', () => {
    expect(credentialsSchema.safeParse({ email: 'a@b.co', password: '' }).success).toBe(false)
  })

  it('rejects a malformed email', () => {
    expect(credentialsSchema.safeParse({ email: 'not-an-email', password: 'x' }).success).toBe(
      false,
    )
  })
})

describe('registrationSchema', () => {
  const valid = {
    email: 'sam@example.com',
    displayName: 'Sam',
    password: 'a-long-enough-password',
  }

  it('accepts a complete registration', () => {
    expect(registrationSchema.safeParse(valid).success).toBe(true)
  })

  it('enforces the 10-character minimum the API enforces', () => {
    expect(registrationSchema.safeParse({ ...valid, password: '123456789' }).success).toBe(
      false,
    )
    expect(registrationSchema.safeParse({ ...valid, password: '1234567890' }).success).toBe(
      true,
    )
  })

  it('imposes no composition rules, because the API imposes none', () => {
    expect(
      registrationSchema.safeParse({ ...valid, password: 'aaaaaaaaaaaaaaa' }).success,
    ).toBe(true)
  })

  it('trims the display name and rejects one that is only whitespace', () => {
    const parsed = registrationSchema.safeParse({ ...valid, displayName: '  Sam  ' })
    expect(parsed.success && parsed.data.displayName).toBe('Sam')

    expect(registrationSchema.safeParse({ ...valid, displayName: '   ' }).success).toBe(false)
  })

  it('rejects a display name over the entity maximum', () => {
    expect(
      registrationSchema.safeParse({ ...valid, displayName: 'x'.repeat(101) }).success,
    ).toBe(false)
  })
})

describe('currentUserSchema', () => {
  const user = {
    id: '018f-…',
    email: 'sam@example.com',
    displayName: 'Sam',
    roles: ['Student'],
    instructorSlug: null,
  }

  it('parses the documented /api/me payload', () => {
    expect(currentUserSchema.safeParse(user).success).toBe(true)
  })

  it('fails closed when roles are missing, rather than reaching a guard as undefined', () => {
    const { roles: _roles, ...withoutRoles } = user
    expect(currentUserSchema.safeParse(withoutRoles).success).toBe(false)
  })

  it('accepts a populated instructorSlug', () => {
    expect(
      currentUserSchema.safeParse({ ...user, instructorSlug: 'jane-doe' }).success,
    ).toBe(true)
  })
})
