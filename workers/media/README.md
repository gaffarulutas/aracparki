# aracparki-media Worker

Cloudflare R2 + Images binding media pipeline for listing photos.

## Endpoints

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| `POST` | `/v1/ingest` | `Authorization: Bearer $INGEST_SECRET` | Validate, sanitize (EXIF strip + orient), store **master without watermark** |
| `POST` | `/v1/delete` | `Authorization: Bearer $INGEST_SECRET` | Hard-delete master object from R2 (`{ "storageKey": "masters/…" }`) |
| `GET` | `/m/{storageKey}?v=card` | public | On-demand variants |
| `GET` | `/health` | public | Liveness |

### Variants

| `v` | Width | Watermark |
|-----|-------|-----------|
| `thumb` | 160 | no |
| `card` | 480 | yes (centered, soft) |
| `md` | 768 | yes (centered, soft) |
| `lg` | 1280 | yes (centered, soft) |
| `xl` | 1920 | yes (centered, soft) |
| `og` | 1200×630 | yes (centered, soft) |

Watermark defaults (`wrangler.toml`): large horizontal logo (~58% width), centered, opacity `0.14`.

## Setup

```bash
cd workers/media
npm install
npx wrangler login
npx wrangler r2 bucket create aracparki-media
npx wrangler r2 bucket create aracparki-media-preview   # optional, for wrangler dev
npx wrangler secret put INGEST_SECRET
# upload logo (required for watermarks):
npx wrangler r2 object put aracparki-media/assets/watermarks/default.png \
  --file=../../src/AracParki.Web/wwwroot/assets/logo/logo.png --remote
npm run deploy
```

Deploy sonrası `PUBLIC_BASE_URL` production origin’e güncelle (dashboard vars veya `wrangler.toml` `[vars]`).

Local:

```bash
npx wrangler secret put INGEST_SECRET --local
npm run dev
```

Point ASP.NET `CloudflareMedia` at the Worker URL and set the same ingest secret.
