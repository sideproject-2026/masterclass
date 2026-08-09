import { HeadContent, Outlet, Scripts, createRootRoute } from '@tanstack/react-router'
import { TanStackRouterDevtoolsPanel } from '@tanstack/react-router-devtools'
import { TanStackDevtools } from '@tanstack/react-devtools'

import { AppShell } from '#/components/layout/app-shell'
import { themeScript } from '#/lib/theme'
import { getCurrentUser } from '#/server/auth'
import appCss from '../styles.css?url'

export const Route = createRootRoute({
  // Auth lands in route context, so the header and every guard downstream read one lookup
  // rather than each fetching their own. A-5 adds the _authed / _instructor guards on top.
  beforeLoad: async () => ({ auth: await getCurrentUser() }),

  head: () => ({
    meta: [
      { charSet: 'utf-8' },
      { name: 'viewport', content: 'width=device-width, initial-scale=1' },
      { title: 'Masterclass — engineering courses' },
    ],
    links: [{ rel: 'stylesheet', href: appCss }],
  }),

  shellComponent: RootDocument,
  component: RootLayout,
})

function RootLayout() {
  const { auth } = Route.useRouteContext()

  return (
    <AppShell auth={auth}>
      <Outlet />
    </AppShell>
  )
}

function RootDocument({ children }: { children: React.ReactNode }) {
  return (
    // suppressHydrationWarning: the theme script below edits this element's class list before
    // React hydrates, so the server's markup and the client's DOM differ here by design.
    <html lang="en" suppressHydrationWarning>
      <head>
        {/*
          Before anything else in the document. This runs synchronously in <head> and sets the
          `dark` class, so a dark-mode visitor never sees a white flash. Moving it below the
          stylesheet, or into a component, reintroduces the flash.
        */}
        <script dangerouslySetInnerHTML={{ __html: themeScript }} />
        <HeadContent />
      </head>
      <body>
        {children}
        <TanStackDevtools
          config={{ position: 'bottom-right' }}
          plugins={[{ name: 'Tanstack Router', render: <TanStackRouterDevtoolsPanel /> }]}
        />
        <Scripts />
      </body>
    </html>
  )
}
