import { serve } from 'srvx'
import { serveStatic } from 'srvx/static'

import handler from './dist/server/server.js'

/**
 * The production entry point.
 *
 * `vite build` emits a **web-standard fetch handler** at `dist/server/server.js`, not a Node
 * HTTP server — it exports `{ fetch(request) }` and has no `listen`. Running it directly with
 * `node` therefore starts nothing and exits 0, which looks exactly like a container that
 * crashed silently. This file is the missing half.
 *
 * `srvx` is what the TanStack Start hosting guide names for this: it adapts a fetch handler to
 * Node, Bun and Deno, and `serveStatic` puts the built client assets in front of it.
 *
 * Deliberately not a host-specific preset. A Nitro/Vercel/Netlify preset would bake a
 * deployment target into the build, and `D-0` has not chosen one yet — a plain Node process
 * listening on `$PORT` is what every candidate host in 10-adr-hosting.md consumes.
 */

const port = Number(process.env.PORT ?? 3000)

// 0.0.0.0, not localhost: inside a container, binding the loopback interface makes the server
// unreachable from outside it while looking perfectly healthy from within.
const hostname = process.env.HOST ?? '0.0.0.0'

serve({
  port,
  hostname,
  // Static first, so hashed assets never pay the cost of entering the router.
  middleware: [serveStatic({ dir: './dist/client' })],
  fetch: (request) => handler.fetch(request),
})

console.log(`web listening on http://${hostname}:${port}`)
