# `SP-1` — YouTube IFrame API

**Sprint 7 · 2026-08-10 · 2 pts · retires risk R2**

A timeboxed spike, run four months before [`P-5`](../design/08-delivery-plan.md) needs it. **The code
was thrown away; this is what it taught us.** The harness was a standalone Node server and one HTML
page outside the repo — deliberately not a route in `web/`, so there is nothing to delete later and
nothing that can rot into production.

Measured in Chromium against `https://www.youtube-nocookie.com`, video `aqz-KE-bpKQ`
(Big Buck Bunny, 634.6s). Everything below marked **measured** was observed; everything marked
**cited** is documented platform behaviour that this spike did not exercise.

---

## Verdict

**R2 is retired.** Nothing in the IFrame API fought us. Progress tracking is straightforward and
the fallback ("manual mark-complete only for video") is not needed.

`P-5` remains a 4-point card. The spike did not make it smaller — it removed the risk that it was
secretly an 8-point card.

---

## 1. The nocookie embed and the API coexist — `host`, not a hand-built iframe

[`05 §2.3`](../design/05-adr-video-and-storage.md) requires `youtube-nocookie.com`. The way to get
that *and* the JS API is the player's `host` option — not building the iframe yourself:

```js
new YT.Player('player', {
  videoId: id,                                  // the 11-char id, validated server-side
  host: 'https://www.youtube-nocookie.com',
  playerVars: { rel: 0, modestbranding: 1, enablejsapi: 1, origin: location.origin },
})
```

**Measured** — the constructed iframe's `src` was exactly:

```
https://www.youtube-nocookie.com/embed/aqz-KE-bpKQ?rel=0&modestbranding=1&enablejsapi=1
  &origin=http%3A%2F%2Flocalhost%3A5599&widgetid=1&forigin=…&aoriginsup=1&vf=1
```

The API appends `widgetid`, `forigin`, `aoriginsup` and `vf` itself. **The design doc does not
mention the `host` option**, and the obvious alternative — writing the `<iframe>` in JSX and
attaching `new YT.Player(existingIframe)` — is the version that quietly reverts to
`www.youtube.com`. `05 §2.3` updated.

**Timings (measured):** `iframe_api` script → `onYouTubeIframeAPIReady` 0.29s; `onReady` 0.99s;
first frame after `playVideo()` 1.34s (BUFFERING → PLAYING).

## 2. `getDuration()` is available at `onReady`, before playback

**Measured** — `onReady` reported `635`, and `634.601` once playing. So the 90% completion
threshold can be computed client-side without trusting `Lesson.DurationSeconds`.

Keep storing `DurationSeconds` anyway: the catalogue and curriculum sidebar need a duration for
lessons nobody has opened, and asking YouTube for it would mean instantiating a player per row.

## 3. `getCurrentTime()` is accurate, and a 1s poll is not needed

**Measured** — over 30+ consecutive one-second ticks, position advanced 0.99–1.03s per tick with
no drift or dropped reads. The ADR's 15-second interval is comfortably safe; the API was never the
limiting factor.

## 4. The scrub hole is cheaper to close than the ADR assumed

[`05 §2.4`](../design/05-adr-video-and-storage.md) accepts that "a student can drag the scrubber to
the end and be credited with completion".

**Measured** — with ~10 lines accumulating only forward movement under 2× realtime:

```js
const delta = t - lastTick
if (delta > 0 && delta < 2) watchedSeconds += delta
```

seeking from 13s to 509s (a 495-second jump) **credited nothing**. Watched time stayed at ~24s and
resumed accruing at 1s per second. Total for the session: position 539s, watched 52s.

**Recommendation for `P-5`: ship the clamp.** It is ten lines and it defeats the casual scrub.

**It is not a security control and must not be described as one.** The client is untrusted — a
determined student can POST any `watchedSeconds` they like. Real integrity means server-side
interval tracking, which remains out of scope for MVP exactly as the ADR says. The clamp changes
the honest default; it does not close the hole. `05 §2.4` updated to say so.

## 5. `sendBeacon` works — but the payload needs a `Blob`, and three events fire, not one

**Measured** — beacons arrived server-side on every unload path, with correct final values.

**The content-type trap.** `navigator.sendBeacon(url, string)` sends
`text/plain;charset=UTF-8`. A minimal-API endpoint bound to a JSON body answers **415** to that,
and you will not see it: `sendBeacon` returns `true` for "queued", never "accepted". Wrap the
payload:

```js
navigator.sendBeacon(url, new Blob([json], { type: 'application/json' }))
```

**Measured** — with the Blob, the server logged `content-type=application/json`.

**Three events fire for one departure.** Navigating away produced, in order, 15ms apart:

```
beforeunload            positionSeconds=539 watchedSeconds=52
pagehide                positionSeconds=539 watchedSeconds=52
visibilitychange hidden positionSeconds=539 watchedSeconds=52
```

Three identical writes for one event. `P-5` should send on **`pagehide` only** — `beforeunload` is
unreliable on mobile and blocks bfcache, and `visibilitychange` fires far too often for something
else (see below). Dedupe by not sending when the position has not moved since the last write.

**`visibilitychange` is noisy.** Merely focusing another window produced a hidden→visible→hidden
cycle roughly every two seconds during automation — **26 beacons** before any video was playing,
all with `positionSeconds=0`. Beaconing unconditionally on `hidden` would hammer
`POST /api/learn/lessons/{id}/progress`. Either drop `visibilitychange` entirely or gate it behind
"position changed since last send".

## 6. `onReady` does not mean the video is playable

**Measured** — constructing a player with a non-existent id fired **`onReady` first, then
`onError` with code `150`** (owner disallows embedded playback / unavailable).

`P-5`'s error state must be driven by `onError`, not by the absence of `onReady`. An instructor
pasting a URL for a video with embedding disabled is the ordinary case here, and `S-*` cannot
detect it at paste time without instantiating a player.

Codes worth handling: `2` malformed id, `5` HTML5 player error, `100` removed or private,
`101`/`150` embedding disabled by the owner.

---

## What this spike did not test

Named so nobody reads the verdict as broader than the evidence.

- **Mobile Safari.** Not tested — no device. This is where the ADR's original worry lives: iOS
  historically forced inline-vs-fullscreen quirks and is the platform where `beforeunload` is least
  reliable. Sending on `pagehide` is the mitigation, and `pagehide` is well supported there
  (**cited**). Residual risk, carried into `P-5` rather than closed.
- **Background-tab timer throttling.** **Cited, not measured:** Chrome clamps `setInterval` in
  hidden tabs to roughly once per minute, and may freeze timers entirely after ~5 minutes. This is
  precisely why the beacon on `pagehide` matters — the interval is a convenience, and the unload
  write is what actually preserves progress. Worth a real measurement during `P-5`.
- **Autoplay policy.** Playback was started from a click on our own button, which is the real
  user journey. Whether `playVideo()` succeeds with no prior gesture was not isolated — assume it
  does not, and never autoplay.
- **CSP.** No CSP is deployed yet. [`06 §2.2`](../design/06-tech-stack.md) specifies
  `frame-src https://www.youtube-nocookie.com`; note the API script itself is served from
  `https://www.youtube.com/iframe_api`, so **`script-src` needs `https://www.youtube.com` even
  though the frame is nocookie**. Easy to miss when `H-*` adds the header.

---

## Changes this spike caused

| Doc | Change |
|---|---|
| [`05 §2.3`](../design/05-adr-video-and-storage.md) | Player construction uses the `host` option; hand-building the iframe silently reverts to `www.youtube.com` |
| [`05 §2.4`](../design/05-adr-video-and-storage.md) | Beacon on `pagehide` only, with a `Blob` payload; ship the forward-delta clamp, and state plainly that it is not a security control |
| [`06 §2.2`](../design/06-tech-stack.md) | CSP `script-src` must allow `https://www.youtube.com`, not only `frame-src` for nocookie |
