# 05 — ADR: Video Hosting & Blob Storage

> **Status:** Accepted · **Date:** 2026-08-08
> **Question asked in the brief:** *"determine all the considerations like storage blob for video — can be used Azure or Firebase"*, with the note that *"the upload initially thru YouTube."*
> **Decision:** **YouTube (unlisted) for video in MVP** behind an `IVideoProvider` abstraction; **Azure Blob Storage** for every other asset. Not Firebase.

---

## 1. Context

Two different storage problems get conflated under "blob storage for video". They have different answers.

| Asset | Size | Access pattern | Needs |
|---|---|---|---|
| **Lesson video** | 100 MB – 2 GB per lesson | Streamed, adaptive bitrate, global | Transcoding, HLS/DASH packaging, CDN, a player |
| **Thumbnails, avatars** | < 500 KB | Public, high read volume | CDN, cheap |
| **Lesson attachments** | 1 – 50 MB | Private, low volume, downloaded | Authenticated access |

Raw blob storage solves the second and third outright. It does **not** solve the first: putting an MP4 in a container and pointing a `<video>` tag at it gives you no adaptive bitrate, no bandwidth-aware quality switching, and full egress billing on every viewer-second. Video hosting is a different product from object storage, and the brief's instinct to start with YouTube is correct.

---

## 2. Decision: video

**MVP uses YouTube unlisted videos.** The instructor uploads to their own YouTube channel, pastes the URL into Studio, and the system stores only the extracted video id.

### 2.1 What is stored

On `Lesson` ([`02 §3`](02-domain-model.md#3-catalog-module--schema-catalog)):

| Field | Value |
|---|---|
| `VideoProvider` | `YouTube` |
| `ExternalVideoId` | `dQw4w9WgXcQ` |
| `DurationSeconds` | `540` |

**The id, never the URL.** Storing `https://www.youtube.com/watch?v=…` bakes today's provider into your data; storing the id plus a provider enum means the URL is built at render time and a provider swap is a data migration of one column.

### 2.2 URL parsing

The Media Module accepts every common YouTube form and normalizes to the id:

```
https://www.youtube.com/watch?v=ID          https://youtu.be/ID
https://www.youtube.com/embed/ID            https://www.youtube.com/shorts/ID
https://m.youtube.com/watch?v=ID&t=42s      …with arbitrary extra query params
```

Anything else → `400 invalid-video-url`. Ids are validated against `^[A-Za-z0-9_-]{11}$` before storage; the id is interpolated into an iframe URL, so treating it as untrusted input is not optional.

### 2.3 Playback

Embed via **`youtube-nocookie.com`**:

```
https://www.youtube-nocookie.com/embed/{id}?rel=0&modestbranding=1&enablejsapi=1
```

- `youtube-nocookie.com` defers tracking cookies until playback — materially better for GDPR posture and for not needing a consent banner on the player.
- `rel=0` limits end-of-video recommendations to the same channel. It **cannot** disable them entirely; YouTube removed that capability.
- `enablejsapi=1` is required for the progress tracking below.

**Build that URL with the player's `host` option, not by hand** ([`SP-1`](../spikes/sp-1-youtube-iframe-api.md)):

```js
new YT.Player('player', {
  videoId: id,
  host: 'https://www.youtube-nocookie.com',
  playerVars: { rel: 0, modestbranding: 1, enablejsapi: 1, origin: location.origin },
})
```

Writing the `<iframe>` in JSX and attaching `new YT.Player(existingIframe)` is the version that
silently reverts to `www.youtube.com` — the nocookie property is lost without any error.

### 2.4 Progress tracking

R6 needs watch progress, and YouTube does not push it to you. Use the **YouTube IFrame Player API**:

1. Load the iframe API, construct a player bound to the embed.
2. On `onStateChange → PLAYING`, start a 15-second interval that reads `player.getCurrentTime()`.
3. Post `{ positionSeconds, watchedSeconds }` to `POST /api/learn/lessons/{id}/progress` ([`03 §5`](03-api-design.md#post-apilearnlessonslessonidprogress)).
4. Also post on `PAUSED`, on `ENDED`, and on **`pagehide`** — via `navigator.sendBeacon`, because a normal fetch is cancelled.
5. Server keeps `watchedSeconds` monotonic, so seeking backwards never reduces credit.
6. Auto-complete at ≥90% watched.

Three things [`SP-1`](../spikes/sp-1-youtube-iframe-api.md) measured that change how step 4 is written:

- **`pagehide` only.** One departure fires `beforeunload`, `pagehide` *and* `visibilitychange` within 15ms of each other — three identical writes. `beforeunload` is also unreliable on mobile and blocks bfcache, and `visibilitychange` fires every time the visitor glances at another window (26 beacons before playback even started). Dedupe further by skipping the send when the position has not moved.
- **Wrap the payload in a `Blob`.** `sendBeacon(url, string)` sends `text/plain`, which a JSON-bound minimal API answers with **415** — invisibly, since `sendBeacon` returns `true` for *queued*, never for *accepted*. Use `new Blob([json], { type: 'application/json' })`.
- **`onReady` does not mean playable.** A video with embedding disabled fires `onReady` and *then* `onError` (code `150`). Drive the error state from `onError`.

Known rough edge: `getCurrentTime()` reports position, not *watched* time, so a student can drag the scrubber to the end and be credited with completion. Solving it properly means server-side interval tracking — real work, and disproportionate for a platform with no certificate and no compliance requirement. **Accepted for MVP.**

`SP-1` did find that ~10 lines of client-side forward-delta clamping (`if (delta > 0 && delta < 2)`) credits nothing for a 495-second scrub, and `P-5` should ship it. **It is not a security control**: the client is untrusted and can post any `watchedSeconds` it likes. It fixes the casual case and changes the honest default; the hole stays open until interval tracking exists.

### 2.5 Alternatives considered

| Option | Cost | Why not now |
|---|---|---|
| **YouTube unlisted** ✅ | Free | Chosen. Zero cost, zero encoding pipeline, global CDN, a player everyone already knows how to use. |
| **Cloudflare Stream** | ~$5 / 1000 min stored + $1 / 1000 min delivered | The best paid upgrade. Signed URLs, real access control, no branding, per-video analytics. **This is the first thing to buy when content becomes paid.** |
| **Mux** | Usage-based, pricier | Excellent player and analytics. Warranted at scale, overkill at launch. |
| **Azure Media Services** | — | **Retired by Microsoft (June 2024).** Do not design against it. Azure's guidance is to move to a partner platform. |
| **Blob Storage + custom HLS** | Storage + egress | You would be building transcoding, packaging, and player infrastructure. This is the "roll your own video platform" trap. |
| **Vimeo** | ~$20/mo+ | Domain-restricted embeds are decent. Priced per-seat in a way that fits creators, not platforms. |

### 2.6 The abstraction

One interface in the Media Module — the only speculative abstraction in this design, and it earns its place because the swap is *planned*, not hypothetical:

```
IVideoProvider
    VideoProviderKind Kind { get; }
    bool TryParseId(string url, out string videoId)
    string BuildEmbedUrl(string videoId)
    Task<int?> GetDurationSecondsAsync(string videoId, CancellationToken ct)
```

`YouTubeVideoProvider` is the MVP implementation. Adding `CloudflareStreamVideoProvider` later means a new class, a registration, and backfilling `VideoProvider`/`ExternalVideoId` per lesson as content is re-uploaded. Both providers can coexist during migration — `VideoProvider` is per-lesson, so old lessons keep playing from YouTube while new ones go to Cloudflare.

---

## 3. Decision: blob storage — **Azure, not Firebase**

Two containers in one Azure Storage account:

| Container | Access | Holds | Delivery |
|---|---|---|---|
| `course-assets` | Public read (via CDN only) | Course thumbnails, instructor avatars | Azure Front Door / CDN, long `Cache-Control`, content-hashed paths |
| `lesson-attachments` | **Private** | Slides, source archives, cheat sheets | Short-lived read SAS, minted per request |

Path convention — content-addressed enough to be cache-safe:

```
course-assets/courses/{courseId}/thumb-{contentHash}.webp
course-assets/instructors/{userId}/avatar-{contentHash}.webp
lesson-attachments/{courseId}/{lessonId}/{attachmentId}/{fileName}
```

Including `courseId` in the attachment path makes "delete everything for this course" a prefix delete rather than a row-by-row walk.

### 3.1 Why not Firebase Storage

Not a quality judgement — it is a coherence judgement:

- The app is .NET 10 on Azure with Azure Database for PostgreSQL and Key Vault. Firebase Storage means a **second cloud account, a second billing relationship, a second IAM model, and a second set of credentials in Key Vault** to serve a handful of thumbnails and PDFs.
- Azure Blob integrates with **managed identity**, so the API holds no storage keys at all. Firebase would reintroduce a long-lived service-account JSON — a credential to store, rotate, and leak.
- Firebase's real draw is its client SDK bundle (auth + Firestore + storage rules). This design uses none of it: auth is ASP.NET Core Identity ([`04`](04-adr-authentication.md)) and data is SQL.

Firebase would be the right answer for a Firebase-native app. This is not one.

### 3.2 Cost sanity check

At MVP scale — ~50 courses, thumbnails plus a few attachments each — total blob storage is comfortably under a gigabyte. Hot-tier storage at that volume costs pennies per month; CDN egress for thumbnails is the larger line and still trivial. **Video, the one genuinely expensive asset, costs nothing because YouTube is serving it.** That is the entire financial argument for the YouTube decision, and it is why replacing YouTube is a business decision (paid content justifies paid hosting), not a technical one.

---

## 4. Limitations you are accepting

Stated plainly, because the YouTube decision is only sound if these are understood and consciously accepted.

| Limitation | Reality |
|---|---|
| **Unlisted ≠ private** | Anyone with the video id can watch it on youtube.com, signed out, without an account on your platform. Your enrollment gate protects the *id* ([`03 §5`](03-api-design.md#get-apilearnlessonslessonid)), not the *video*. One leaked id, or one browser extension reading the embed, and that lesson is public. |
| **No DRM, trivially downloadable** | `yt-dlp` exists. This is true of most non-DRM video, but with YouTube there is a mature, one-command tool everyone already has. |
| **YouTube branding is unavoidable** | The logo, the title overlay linking to youtube.com, and end-screen recommendations from the same channel. `rel=0` and `modestbranding=1` reduce this; they do not remove it. |
| **Content lives on the instructor's channel** | Ownership question worth settling in writing before onboarding external instructors. If an instructor deletes their channel or leaves, your lessons 404. Mitigation: use a **platform-owned channel** and have instructors submit source files. |
| **Third-party availability** | A YouTube outage, a regional block, or a copyright strike (a false positive on background music is common) takes lessons offline with no recourse on your side. |
| **Analytics live elsewhere** | Retention curves and drop-off are in YouTube Studio, tied to the channel owner, not in your database. You only have what your own heartbeats capture. |
| **Scrubbing defeats completion** | See §2.4. |

**Bottom line:** YouTube is the correct MVP call — it removes the single largest cost and complexity item for free. It is **not** viable for paid content. Migrating video is therefore an explicit prerequisite for launching Billing, and should be planned alongside it rather than discovered then.

---

## 5. Direct-to-blob upload flow

Bytes never transit the API. The API's only job is to authorize the upload and mint a scoped, short-lived credential.

```mermaid
sequenceDiagram
    participant B as Browser (Studio)
    participant S as TanStack Start (BFF)
    participant A as Lms.Api
    participant Z as Azure Blob

    B->>S: choose file (name, type, size)
    S->>A: POST /api/studio/lessons/{id}/attachments/upload-url
    A->>A: verify ownership; validate type + size
    A->>Z: get user delegation key (managed identity)
    A-->>S: { uploadUrl (SAS, write-only, 15 min), blobPath }
    S-->>B: upload ticket
    B->>Z: PUT bytes directly (x-ms-blob-type: BlockBlob)
    Z-->>B: 201
    B->>S: POST /api/studio/lessons/{id}/attachments { blobPath, fileName, sizeBytes }
    S->>A: persist LessonAttachment row
    A-->>S: 201
```

Rules that make this safe:

- **User-delegation SAS**, signed with Entra ID via the API's managed identity — not an account key. There is no storage account key in the application at all, so there is nothing to leak and nothing to rotate.
- **Write-only (`c`,`w`), single blob, 15-minute expiry.** The ticket cannot list the container, cannot read other blobs, and cannot be replayed tomorrow.
- **The server chooses `blobPath`.** The client supplies a filename, never a path — otherwise `../` and overwrite games are on the table.
- **Content type and size are validated when minting the SAS.** That is the last moment the server has control; Azure will accept whatever the SAS permits. Allow-list extensions (`pdf`, `zip`, `md`, `png`, `jpg`, `webp`), cap at 50 MB.
- **Reads use a separate, read-only SAS** (`r`, ~15 min), minted per request inside `GET /api/learn/lessons/{id}` and only after the enrollment gate passes. Download URLs are never persisted — a stored SAS is a permanent leak waiting to be logged.
- **Orphan cleanup:** an upload that never gets confirmed leaves a blob with no row. A weekly job deletes unreferenced blobs older than 24 hours. Cheap, and it stops slow storage-cost creep.

Thumbnails follow the same flow via `POST /api/studio/courses/{id}/thumbnail-upload-url`, into the public `course-assets` container, with image-only validation and server-side resizing deferred (accept ≤2 MB, let the CDN handle delivery).

---

## 6. Consequences

**Gained:** zero video cost and zero encoding pipeline; no bytes through the API tier, so upload load never affects request latency; no storage keys anywhere in the system; a single-cloud footprint with one IAM model.

**Accepted:** the limitations in §4 — chiefly that unlisted video is not access-controlled, which caps this design at free content.

**Revisit when:** any course becomes paid (migrate video to Cloudflare Stream first — this is a hard prerequisite, not a nice-to-have); an instructor requires captions/transcripts the platform controls; or per-lesson retention analytics become a product requirement rather than a curiosity.
