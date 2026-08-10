# syntax=docker/dockerfile:1

# The web (BFF) image. Built from the repository root:
#   docker build -f web/Dockerfile -t lms-web .
#
# This container is not a static file server. It is the BFF: it holds the session cookie and
# calls the API server-side, so it must run Node, not nginx
# (04-adr-authentication.md §3). A "just serve dist/client" deployment would break
# authentication entirely and would do it quietly — the pages would still render.

# ---- dependencies ---------------------------------------------------------
FROM node:22-alpine AS deps
WORKDIR /app

# npm ci, never install: a lock file that has drifted from package.json must fail the build
# rather than silently resolve to something nobody tested. Same rule as CI (`F-6`).
COPY web/package.json web/package-lock.json ./
RUN npm ci

# ---- build ----------------------------------------------------------------
FROM node:22-alpine AS build
WORKDIR /app

COPY --from=deps /app/node_modules ./node_modules
COPY web/ ./

RUN npm run build

# Re-resolve with dev dependencies dropped. The build needed vite and the TanStack plugins;
# the running server does not, and they are the bulk of the tree.
RUN npm ci --omit=dev

# ---- runtime --------------------------------------------------------------
FROM node:22-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production
ENV PORT=3000
EXPOSE 3000

COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist
COPY --from=build /app/package.json ./
COPY --from=build /app/server.mjs ./

# The node images ship a 'node' user. Files are owned by root and only read at runtime,
# which is what we want — the process cannot rewrite its own code.
USER node

# SESSION_SECRET has no default outside development: src/server/session.ts refuses to start
# with the committed development value when NODE_ENV=production. That is deliberate — a
# missing secret should stop a deploy, not silently make every session forgeable.
# server.mjs, not dist/server/server.js. The build emits a fetch handler with no listener, so
# running it directly starts nothing and exits 0 — a silent no-op that looks like a healthy
# deploy right up until the first request. See web/server.mjs.
CMD ["node", "server.mjs"]
