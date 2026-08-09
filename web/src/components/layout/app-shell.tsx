import type { ReactNode } from 'react'

import { SiteHeader } from '#/components/layout/site-header'
import type { AuthState } from '#/server/auth'

/**
 * Header, content, footer — the frame every page from Sprint 14 onward renders inside.
 *
 * It takes children rather than a set of flags. A shell with `showNav`, `wide`, `noFooter`
 * booleans becomes unreadable by the fourth screen that needs an exception; composition does
 * not.
 */
export function AppShell({ auth, children }: { auth: AuthState; children: ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      <SiteHeader auth={auth} />

      <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-8">{children}</main>

      <footer className="border-t">
        <div className="mx-auto max-w-5xl px-4 py-6 text-sm text-muted-foreground">
          Masterclass — engineering courses.
        </div>
      </footer>
    </div>
  )
}
