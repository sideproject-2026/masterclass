import { apiBaseUrl, fetchApiHealth } from "#/server/api"
import { createServerFn } from "@tanstack/react-start"

export const getApiHealth = createServerFn({ method: 'GET' }).handler(async () => {
  const health = await fetchApiHealth()
  return { ...health, baseUrl: apiBaseUrl() }
})