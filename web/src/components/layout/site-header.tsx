import { Link } from '@tanstack/react-router'

import { ThemeToggle } from '#/components/theme-toggle'
import { Button } from '#/components/ui/button'
import type { AuthState } from '#/server/auth'

/**
 * The application header.
 *
 * Takes the auth state rather than fetching it — components render, hooks decide, and this one
 * only renders. The state comes from the root route's context, so every page shares one lookup.
 */
export function SiteHeader({ auth }: { auth: AuthState }) {
  return (
    <header className="border-b">
      <div className="mx-auto flex h-14 max-w-5xl items-center gap-3 px-4">
        <Link to="/" className="font-heading font-semibold tracking-tight">
          Masterclass
        </Link>

        {/* Pushes everything after it to the right, and collapses first on a narrow screen. */}
        <div className="flex-1" />

        <ThemeToggle />

        {auth.signedIn ? <SignedIn auth={auth} /> : null}
      </div>
    </header>
  )
}

function SignedIn({ auth }: { auth: Extract<AuthState, { signedIn: true }> }) {
  return (
    <div className="flex items-center gap-2">
      {/*
        The name is the first thing to go at 375px: it is confirmation, not navigation, and
        sign-out has to stay reachable on a phone.
      */}
      <span className="hidden text-sm text-muted-foreground sm:inline">
        {auth.user.displayName}
      </span>

      {/* A real form POST — see routes/sign-out.tsx for why this is not an RPC. */}
      <form method="post" action="/sign-out">
        <Button type="submit" variant="outline" size="sm">
          Sign out
        </Button>
      </form>
    </div>
  )
}
