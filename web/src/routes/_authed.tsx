import { createFileRoute, redirect } from '@tanstack/react-router'

/**
 * The authenticated layout.
 *
 * Everything nested under it requires a signed-in visitor. The check runs in `beforeLoad`, so
 * a signed-out request is turned away *before* the child route's loader runs and before any
 * markup is produced — no flash of protected content, no wasted fetch.
 *
 * It reads `context.auth`, which the root route has already resolved (`__root.tsx`), rather
 * than calling the server itself: one lookup per navigation, shared by the header, the guards
 * and every page.
 *
 * **This is UX, not security** (04 §3.1). The API answers 401/403 on its own account; this
 * guard exists so a signed-out visitor gets a login page instead of an empty screen.
 */
export const Route = createFileRoute('/_authed')({
  beforeLoad: ({ context, location }) => {
    if (!context.auth.signedIn) {
      // location.href is the path and search of the request being denied, so the login page
      // can put the visitor back where they were aiming. It is validated on the way out again
      // by redirectSearchSchema — a value that arrives here trusted still leaves untrusted.
      throw redirect({ to: '/login', search: { redirect: location.href } })
    }

    // Narrowed once, here, so children read a `CurrentUser` rather than re-testing the union.
    return { user: context.auth.user }
  },
})
