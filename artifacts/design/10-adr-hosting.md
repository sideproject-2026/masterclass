# 10 — ADR: Hosting

**`D-0` · 2026-08-10 · status: 🟡 RECOMMENDED, awaiting sign-off**

> This card's deliverable is a *decision*, not infrastructure — same discipline as
> [`SP-1`](../spikes/sp-1-youtube-iframe-api.md). Nothing was provisioned.
>
> **The recommendation below is not yet a decision.** Cost tolerance, which accounts exist, and
> where the data may legally sit are the owner's call, not the researcher's. `D-1` and `D-3`
> stay unscheduled until this document says ACCEPTED.

---

## 1. Why this is being asked again

[`08` revision 4](08-delivery-plan.md) reopened it. Azure was chosen in the original stack doc and
`D-1`/`D-3` were written as Bicep, Container Apps, Key Vault and managed identity — none of which
survives a change of provider.

## 2. What the app actually needs

Derived from the design docs, not from a vendor's feature list. **The first three are
disqualifiers**: a host that cannot do them is out, not worked around.

| # | Requirement | Where it comes from |
|---|---|---|
| 1 | A **one-shot job that gates the rollout** — migrations run to completion, and the new version does not go live if they fail | [`06 §3`](06-tech-stack.md). Startup migration races across replicas; `IT-1` already found the two-replica seeder crash this would have caused |
| 2 | **Object storage with pre-signed direct upload** — the browser uploads straight to storage, the API never proxies bytes | [`05 §5`](05-adr-video-and-storage.md) |
| 3 | **Secret storage reachable without a key in the repo** | [`04 §3.1`](04-adr-authentication.md) — the JWT signing key and session secret |
| 4 | Two long-running containers (`api`, `web`) | [`01 §3`](01-architecture.md) |
| 5 | Managed PostgreSQL with backups someone else rehearses | [`01 §7.2`](01-architecture.md) |
| 6 | Low cost at **zero** traffic — this runs for months before it has users | [`08 §2`](08-delivery-plan.md) |
| 7 | Low **exit** cost | PostgreSQL was chosen over Azure SQL specifically to stay portable. Do not spend that on the host |

## 3. The decoupling that makes this easy

**Object storage does not have to come from the compute provider.** Requirement 2 is satisfied by
any S3-compatible store, independently of where the containers run. Deciding storage separately
removes it from the comparison entirely and improves requirement 7 at the same time.

**Recommendation: Cloudflare R2 for object storage, whatever wins on compute.** S3-compatible with
full pre-signed URL support, **$0 egress at any volume**, ~$0.015/GB stored, and a free tier of
10 GB storage / 1M writes / 10M reads that does not expire. Course thumbnails and lesson PDFs are
a read-heavy, egress-shaped workload, which is the exact case zero-egress pricing is good at.

## 4. Candidates against the disqualifiers

| | Migration gate | Pre-signed upload | Managed Postgres | ~Cost at low traffic | Ops burden |
|---|---|---|---|---|---|
| **Render** | ✅ **pre-deploy command** — runs before traffic switches | ✅ via R2 | ✅ | ~$14 compute (2 services × $7) + DB | Lowest |
| **Fly.io** | ✅ **`release_command`** — temp Machine, deploy **stops** if it fails | ✅ via R2 | ⚠️ see below | ~$2/machine; ~$13–20 typical with DB | Low–medium |
| **Azure Container Apps** *(incumbent)* | ✅ Container Apps Job, orchestrated from CI | ✅ native (user-delegation SAS) or R2 | ✅ | ~$13 ACA idle + ~$12.41 B1ms Postgres = **$25–30 compute only**, before storage and bandwidth | Medium — and `D-1` is the largest infra card |
| **Hetzner + Coolify** | ✅ but you write and own it | ✅ via R2 | ❌ you run Postgres, and its backups | ~€6–15/mo | **Highest** — OS patching, TLS, backups, restore drills |

Not carried forward: **AWS App Runner** (no first-class pre-deploy gate — requirement 1 becomes CI
choreography), **Firebase/Supabase-as-platform** (rejected for the same reason `05 §3` rejected
Firebase storage — a second IAM model for no gain), **AKS/ECS/Cloud Run+Terraform** (unjustifiable
operational surface at this size).

## 5. Recommendation

> ### Render for compute and Postgres · Cloudflare R2 for object storage

**Why Render over Fly**, which is cheaper and whose `release_command` is the single best fit for
requirement 1: the scarcest resource on this project is **evenings**, not dollars.
[`08 §2.1`](08-delivery-plan.md) budgets ~11h/week and protects Fridays and Sundays. Render's
managed Postgres removes backup and restore ownership completely; Fly's Postgres story has
historically leaned more DIY, and "the database is my problem now" is exactly the wrong debt for
someone with four evenings a week. The delta is roughly $10/month against hours that are already
fully committed.

**Why not Azure**, which is not a bad platform: at this size you pay roughly double for features
this project does not use yet, and `D-1` — Bicep for a resource group, Flexible Server, Storage,
Key Vault, a Container Apps environment and two apps — is the single largest infra card in the
plan. Managed identity and Key Vault are genuinely excellent and genuinely unnecessary for a
pre-launch side project with one developer.

**Why not Hetzner + Coolify**, which is cheapest and the most fun: it converts a monthly bill into
a recurring time cost — patching, TLS renewal, and backups you have to *test*, not just configure.
Reconsider it after launch, when the app has revenue and the hours have a different value.

**Exit cost, deliberately kept low.** PostgreSQL is portable everywhere. The containers are
portable by construction (`D-2`). R2 is S3-compatible, so storage swaps to any S3 provider by
changing an endpoint. The only lock-in is a `render.yaml`, which is a few dozen lines.

## 6. What this changes if accepted

| Card / doc | Change |
|---|---|
| `D-1` | Not Bicep. Render services + managed Postgres + an R2 bucket, largely declarative. **Re-estimate ~5 → ~2–3 pts.** |
| `D-3` | Render deploys on push to `main`; the migration gate becomes the pre-deploy command rather than a CI-orchestrated job. **~2 pts holds, possibly less.** |
| `S-7` (Sprint 12) | **`AWSSDK.S3` pre-signed URLs instead of `Azure.Storage.Blobs` + user-delegation SAS.** Same shape, different SDK — `05 §5`'s flow is unchanged. Decide before Sprint 12. |
| [`06 §3`](06-tech-stack.md) | Provisional rows resolve; `Azure.Identity` leaves the package list |
| [`05 §3`](05-adr-video-and-storage.md) | "Two containers in one Azure Storage account" → two R2 buckets. The Firebase rejection still stands on its own reasoning |
| [`08`](08-delivery-plan.md) | `D-1`/`D-3` get real estimates and a sprint; **M1b** gets a date |
| Local dev | **Unchanged.** Aspire keeps running Postgres and Azurite in Docker. Azurite speaks the Azure Blob API, so if `S-7` moves to S3 the local emulator should become MinIO. Small, and it belongs to `S-7`. |

## 7. Confidence, and what is not verified

Stated plainly, because this is a document that will be trusted later.

- **Pricing is from secondary sources** (comparison articles, August 2026), not vendor pricing
  pages, and it moves. **Confirm against Render, Fly and Cloudflare directly before committing.**
  Treat every figure here as an order of magnitude, not a quote.
- **Render's pre-deploy command was confirmed** to exist and to run before traffic switches.
  **Fly's `release_command` was confirmed** to abort the deploy on failure. Requirement 1 is
  genuinely met by both.
- **Railway and DigitalOcean App Platform were not investigated properly.** Both plausibly satisfy
  requirement 1; neither was verified, so neither is ranked. If either is already familiar, it is
  worth thirty minutes before accepting this.
- **Fly's current managed-Postgres offering was not verified.** The recommendation leans on a
  historical characterisation. If Fly now offers fully managed Postgres with backups and PITR, the
  gap to Render narrows to roughly nothing and Fly becomes the cheaper equivalent — **this is the
  one check most likely to change the answer.**
- No account was created and nothing was deployed.

## 8. Decision

**Status: 🟡 RECOMMENDED — awaiting the owner's sign-off.**

Accepting this means editing §8 to **ACCEPTED**, dating it, and letting `D-1`/`D-3` be re-estimated
and scheduled. Rejecting it means saying which requirement or cost line was wrong, which is more
useful than a different preference.

**Sources:** [Fly release_command](https://fly.io/docs/reference/configuration/) ·
[Fly seamless deployments](https://fly.io/docs/blueprints/seamless-deployments/) ·
[Render pre-deploy command](https://render.com/changelog/predeploy-command) ·
[Azure Container Apps pricing](https://azure.microsoft.com/en-us/pricing/details/container-apps/) ·
[Azure PostgreSQL Flexible Server pricing](https://azure.microsoft.com/en-us/pricing/details/postgresql/flexible-server/) ·
[Cloudflare R2 GA / pre-signed URLs](https://blog.cloudflare.com/r2-ga/)
