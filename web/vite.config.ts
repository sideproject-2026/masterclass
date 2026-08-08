import { defineConfig } from 'vite'
import { devtools } from '@tanstack/devtools-vite'

import { tanstackStart } from '@tanstack/react-start/plugin/vite'

import viteReact from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// The Aspire AppHost assigns the port and passes it as PORT. Falling back to 3000 keeps
// `npm run dev` working on its own, without an AppHost.
const port = Number(process.env.PORT ?? 3000)

const config = defineConfig({
  server: { port },
  preview: { port },
  resolve: { tsconfigPaths: true },
  plugins: [devtools(), tailwindcss(), tanstackStart(), viteReact()],
})

export default config
