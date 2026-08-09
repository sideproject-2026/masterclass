import ReactMarkdown from 'react-markdown'
import rehypeSanitize, { defaultSchema } from 'rehype-sanitize'
import remarkGfm from 'remark-gfm'

/**
 * The only way markdown reaches the page.
 *
 * Instructors are curated, not trusted — an account is one compromised password away from
 * being an attacker, and course content renders on a page where students are signed in. So the
 * allow-list is conservative and there is no raw-HTML passthrough at all.
 */

/**
 * What an instructor is allowed to produce.
 *
 * Built by narrowing rehype-sanitize's default rather than listing tags from scratch: the
 * default already excludes `script`, `style`, `iframe`, event-handler attributes and
 * `javascript:` URLs, and it is maintained by people who track the bypasses. Starting from an
 * empty list and adding what looks safe is how allow-lists acquire holes.
 */
const schema = {
  ...defaultSchema,
  attributes: {
    ...defaultSchema.attributes,
    // Anchors keep href (protocol-filtered by the default schema) and nothing else. No
    // target, no rel — the renderer decides those below, so a link cannot opt itself out.
    a: ['href'],
    // Only the language hint that syntax highlighting will later read.
    code: [['className', /^language-./]],
  },
  // Belt and braces: react-markdown does not render raw HTML unless rehype-raw is added, and
  // it is not. This makes the intent explicit so nobody adds it casually later.
  protocols: {
    ...defaultSchema.protocols,
    href: ['http', 'https', 'mailto'],
  },
} as const

export function Markdown({ children }: { children: string }) {
  return (
    <div className="prose-sm max-w-none space-y-4 [&_a]:text-primary [&_a]:underline [&_code]:rounded [&_code]:bg-muted [&_code]:px-1 [&_h2]:font-heading [&_h2]:text-xl [&_h2]:font-semibold [&_h3]:font-heading [&_h3]:text-lg [&_h3]:font-semibold [&_li]:ml-4 [&_ol]:list-decimal [&_pre]:overflow-x-auto [&_pre]:rounded [&_pre]:bg-muted [&_pre]:p-3 [&_ul]:list-disc">
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        rehypePlugins={[[rehypeSanitize, schema]]}
        components={{
          // href is named explicitly rather than spread: react-markdown also passes a `node`
          // prop, which lands in the DOM as node="[object Object]" if forwarded. Picking the
          // one attribute the schema allows mirrors the allow-list here too, so nothing can
          // arrive on the element that was not deliberately chosen.
          //
          // Every link leaves to somewhere we do not control. noopener is the one that
          // matters: without it the opened page can navigate this tab via window.opener.
          a: ({ href, children: text }) => (
            <a href={href} target="_blank" rel="noopener noreferrer nofollow">
              {text}
            </a>
          ),
        }}
      >
        {children}
      </ReactMarkdown>
    </div>
  )
}
