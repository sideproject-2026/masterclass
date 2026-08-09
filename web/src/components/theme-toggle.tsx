import { Moon, Sun } from 'lucide-react'

import { Button } from '#/components/ui/button'
import { toggleTheme } from '#/lib/theme'

/**
 * Light/dark switch.
 *
 * Which icon shows is decided by CSS, not by React state. That is deliberate: the server
 * cannot know the visitor's theme, so a state-driven icon either flashes the wrong one or
 * throws a hydration warning. Letting the `dark` class pick the icon means the correct one is
 * painted on the first frame and this component needs no state, no effect, and no client-only
 * guard.
 */
export function ThemeToggle() {
  return (
    <Button
      type="button"
      variant="ghost"
      size="icon-sm"
      onClick={toggleTheme}
      // The control is icon-only, so the accessible name has to come from here.
      aria-label="Switch between light and dark theme"
      title="Switch theme"
    >
      <Sun className="hidden dark:block" aria-hidden="true" />
      <Moon className="block dark:hidden" aria-hidden="true" />
    </Button>
  )
}
