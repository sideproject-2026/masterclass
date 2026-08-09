/**
 * Theme selection, kept out of React on purpose.
 *
 * The chosen theme lives in one place — the `dark` class on `<html>` — and `styles.css`
 * declares `@custom-variant dark (&:is(.dark *))` against it. React never holds it in state,
 * which is what removes the whole class of hydration mismatches: the server cannot know what
 * the visitor picked, so any server-rendered guess is a flash or a mismatch.
 */

const STORAGE_KEY = 'theme'

/**
 * Runs in `<head>`, before the first paint.
 *
 * Without this, a dark-mode visitor gets a white flash on every single page load, because the
 * class is applied only once React hydrates. It is inlined as a string rather than imported
 * because it has to execute before any module loads.
 *
 * Wrapped in try/catch: `localStorage` throws outright in some privacy modes, and a theme
 * preference is not worth a blank page.
 */
export const themeScript = `(function(){try{
var t=localStorage.getItem('${STORAGE_KEY}');
if(t!=='light'&&t!=='dark'){t=window.matchMedia('(prefers-color-scheme: dark)').matches?'dark':'light'}
document.documentElement.classList.toggle('dark',t==='dark');
}catch(e){}})()`

/** Flips the theme and remembers it. Called from the toggle's click handler, nowhere else. */
export function toggleTheme(): void {
  const isDark = document.documentElement.classList.toggle('dark')

  try {
    localStorage.setItem(STORAGE_KEY, isDark ? 'dark' : 'light')
  } catch {
    // Storage unavailable. The theme still applies for this page view, which is the part
    // the visitor asked for; only remembering it fails.
  }
}
