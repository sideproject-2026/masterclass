import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import viteReact from '@vitejs/plugin-react'

/**
 * Deliberately separate from vite.config.ts.
 *
 * The app config loads the TanStack Start plugin, which rewrites server functions and expects a
 * running server — none of which a unit test wants. This config carries only what compiling JSX
 * needs, so tests stay fast and cannot accidentally depend on the framework's runtime.
 */
export default defineConfig({
  plugins: [viteReact()],
  resolve: {
    // fileURLToPath, not URL.pathname: on Windows the latter yields "/D:/..." and nothing
    // resolves against it.
    alias: { '#': fileURLToPath(new URL('./src', import.meta.url)) },
  },
  test: {
    include: ['src/**/*.test.ts', 'src/**/*.test.tsx'],
  },
})
