import { renderToStaticMarkup } from 'react-dom/server'
import { describe, expect, it } from 'vitest'

import { Markdown } from '#/components/markdown'

/**
 * The sanitiser is a security control, not a formatting nicety.
 *
 * Course content is authored by instructors and rendered on pages where students are signed in.
 * An instructor account is one reused password away from being an attacker's account, so
 * "instructors are curated" is a reason to keep the allow-list tight, not a reason to trust the
 * input. Every case below is a real payload class, and each one must come out inert.
 */

const render = (markdown: string) => renderToStaticMarkup(<Markdown>{markdown}</Markdown>)

describe('Markdown', () => {
  it('renders ordinary markdown', () => {
    const html = render('**bold** and `code`')

    expect(html).toContain('<strong>bold</strong>')
    expect(html).toContain('<code>code</code>')
  })

  /**
   * The element is what matters, not the characters.
   *
   * raw HTML is never parsed, so the tag is dropped and its contents fall through as literal
   * text: <p>Before alert(1) after</p>. That text is inert — asserting the string "alert(1)" is
   * absent would be testing the payload rather than the property, and would fail on a page that
   * legitimately discusses XSS.
   */
  it('strips a script tag', () => {
    const html = render('Before <script>alert(1)</script> after')

    expect(html).not.toContain('<script')
    expect(html).toContain('<p>Before alert(1) after</p>')
  })

  it('strips a javascript: href', () => {
    const html = render('[click me](javascript:alert(1))')

    expect(html).not.toContain('javascript:')
    expect(html).toContain('click me')
  })

  it('strips an inline event handler', () => {
    const html = render('<img src=x onerror="alert(1)">')

    expect(html).not.toContain('onerror')
    expect(html).not.toContain('alert(1)')
  })

  it('strips an iframe', () => {
    const html = render('<iframe src="https://evil.example"></iframe>')

    expect(html).not.toContain('<iframe')
  })

  it('strips a data: URL', () => {
    const html = render('[x](data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==)')

    expect(html).not.toContain('data:text/html')
  })

  /**
   * Without noopener, the opened page can reach back through window.opener and navigate this
   * tab — a phishing primitive that costs the attacker nothing.
   */
  it('makes external links unable to reach back through window.opener', () => {
    const html = render('[example](https://example.com)')

    expect(html).toContain('rel="noopener noreferrer nofollow"')
    expect(html).toContain('target="_blank"')
  })

  it('does not leak react-markdown internals onto the element', () => {
    const html = render('[example](https://example.com)')

    expect(html).not.toContain('node=')
    expect(html).not.toContain('[object Object]')
  })
})
