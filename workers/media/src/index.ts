/**
 * Cloudflare media Worker for aracparki.com
 *
 * POST /v1/ingest   — validate, strip EXIF (re-encode), store master in R2 (no watermark)
 * POST /v1/delete   — hard-delete master object from R2
 * GET  /m/*         — on-demand variants (thumb clean; card/md/lg/xl/og subtle centered watermark)
 * GET  /health
 *
 * Secrets (wrangler secret put):
 *   INGEST_SECRET
 */

export interface Env {
  MEDIA: R2Bucket;
  IMAGES: ImagesBinding;
  INGEST_SECRET: string;
  PUBLIC_BASE_URL: string;
  DEFAULT_WATERMARK_KEY: string;
  WATERMARK_OPACITY: string;
  WATERMARK_SCALE_PCT: string;
  MAX_UPLOAD_BYTES: string;
  MAX_MEGAPIXELS: string;
  MAX_EDGE_PX: string;
}

const VARIANT_WIDTH: Record<string, number> = {
  thumb: 160,
  card: 480,
  md: 768,
  lg: 1280,
  xl: 1920,
  og: 1200,
};

const WATERMARK_VARIANTS = new Set(["card", "md", "lg", "xl", "og"]);

const ALLOWED_MIME = new Set([
  "image/jpeg",
  "image/png",
  "image/webp",
  "image/heic",
  "image/heif",
]);

export default {
  async fetch(request: Request, env: Env, ctx: ExecutionContext): Promise<Response> {
    const url = new URL(request.url);

    if (request.method === "GET" && url.pathname === "/health") {
      return Response.json({ ok: true, service: "aracparki-media" });
    }

    if (request.method === "POST" && url.pathname === "/v1/ingest") {
      return ingest(request, env);
    }

    if (request.method === "POST" && url.pathname === "/v1/delete") {
      return hardDelete(request, env);
    }

    if (request.method === "GET" && url.pathname.startsWith("/m/")) {
      return deliver(request, env, ctx);
    }

    return new Response("Not Found", { status: 404 });
  },
} satisfies ExportedHandler<Env>;

function requireIngestAuth(request: Request, env: Env): Response | null {
  const auth = request.headers.get("Authorization") ?? "";
  const expected = `Bearer ${env.INGEST_SECRET}`;
  if (!env.INGEST_SECRET || auth !== expected) {
    return jsonError("UNAUTHORIZED", 401);
  }
  return null;
}

function isDeletableMasterKey(storageKey: string): boolean {
  return (
    !!storageKey &&
    storageKey.startsWith("masters/") &&
    !storageKey.includes("..") &&
    !storageKey.includes("\\") &&
    storageKey.length < 400
  );
}

async function hardDelete(request: Request, env: Env): Promise<Response> {
  const denied = requireIngestAuth(request, env);
  if (denied) return denied;

  let storageKey = "";
  try {
    const body = (await request.json()) as { storageKey?: unknown };
    storageKey = String(body.storageKey ?? "").trim();
  } catch {
    return jsonError("INVALID_JSON", 400);
  }

  if (!isDeletableMasterKey(storageKey)) {
    return jsonError("INVALID_KEY", 400);
  }

  const existing = await env.MEDIA.head(storageKey);
  await env.MEDIA.delete(storageKey);
  if (!existing) {
    return jsonError("NOT_FOUND", 404);
  }

  return Response.json({ ok: true, storageKey });
}

async function ingest(request: Request, env: Env): Promise<Response> {
  const denied = requireIngestAuth(request, env);
  if (denied) return denied;

  const maxBytes = Number(env.MAX_UPLOAD_BYTES || 10_485_760);
  const maxEdge = Number(env.MAX_EDGE_PX || 8000);
  const maxMp = Number(env.MAX_MEGAPIXELS || 40);

  let form: FormData;
  try {
    form = await request.formData();
  } catch (err) {
    console.error("formData parse failed", err);
    return jsonError("INVALID_FORM", 400);
  }

  const accountRaw = String(form.get("accountId") ?? "");
  const accountId = Number(accountRaw);
  if (!Number.isFinite(accountId) || accountId <= 0) {
    return jsonError("INVALID_ACCOUNT", 400);
  }

  const file = form.get("file");
  if (!(file instanceof File) || file.size === 0) {
    return jsonError("MISSING_FILE", 400);
  }

  if (file.size > maxBytes) {
    return jsonError("FILE_TOO_LARGE", 413);
  }

  const contentType = (file.type || "application/octet-stream").toLowerCase();
  if (!ALLOWED_MIME.has(contentType)) {
    return jsonError("UNSUPPORTED_MIME", 415);
  }

  const originalFilename =
    typeof form.get("originalFilename") === "string"
      ? String(form.get("originalFilename")).slice(0, 200)
      : file.name?.slice(0, 200) || null;

  const inputBytes = await file.arrayBuffer();

  let info: { width: number; height: number; format?: string };
  try {
    info = await env.IMAGES.info(new Uint8Array(inputBytes).buffer);
  } catch (err) {
    console.error("IMAGES.info failed", err);
    return jsonError("INVALID_IMAGE", 415);
  }

  if (info.width > maxEdge || info.height > maxEdge) {
    return jsonError("DIMENSION_LIMIT", 400);
  }

  const megapixels = (info.width * info.height) / 1_000_000;
  if (megapixels > maxMp) {
    return jsonError("MEGAPIXEL_LIMIT", 400);
  }

  // Re-encode master: auto-orient + strip EXIF/GPS. Never apply watermark to master.
  // Images binding requires MIME format strings (e.g. "image/jpeg"), not short names.
  let sanitized: ReadableStream;
  let byteSize: number;
  try {
    const output = await env.IMAGES.input(new Uint8Array(inputBytes).buffer)
      .transform({ fit: "scale-down", width: maxEdge, height: maxEdge })
      .output({ format: "image/jpeg", quality: 92 });
    const blob = await output.response().blob();
    byteSize = blob.size;
    sanitized = blob.stream();
  } catch (err) {
    console.error("sanitize failed", err);
    return jsonError("SANITIZE_FAILED", 500);
  }

  const imageId = crypto.randomUUID().replaceAll("-", "");
  const version = 1;
  const storageKey = `masters/${accountId}/${imageId}/v${version}`;

  const digest = await sha256Hex(inputBytes);
  await env.MEDIA.put(storageKey, sanitized, {
    httpMetadata: { contentType: "image/jpeg" },
    customMetadata: {
      accountId: String(accountId),
      imageId,
      version: String(version),
      checksumSha256: digest,
      originalFilename: originalFilename ?? "",
      sourceWidth: String(info.width),
      sourceHeight: String(info.height),
    },
  });

  // Re-read dimensions after sanitize (may differ if scaled)
  const masterObj = await env.MEDIA.get(storageKey);
  let width = info.width;
  let height = info.height;
  if (masterObj) {
    try {
      const masterInfo = await env.IMAGES.info(await masterObj.arrayBuffer());
      width = masterInfo.width;
      height = masterInfo.height;
    } catch {
      /* keep source dims */
    }
  }

  const publicBase = (env.PUBLIC_BASE_URL || new URL(request.url).origin).replace(/\/$/, "");
  const deliveryUrl = `${publicBase}/m/${storageKey}?v=card`;

  return Response.json({
    deliveryUrl,
    imageId,
    storageKey,
    version,
    width,
    height,
    byteSize,
    mimeType: "image/jpeg",
    checksumSha256: digest,
    variants: {
      thumb: `${publicBase}/m/${storageKey}?v=thumb`,
      card: deliveryUrl,
      md: `${publicBase}/m/${storageKey}?v=md`,
      lg: `${publicBase}/m/${storageKey}?v=lg`,
      xl: `${publicBase}/m/${storageKey}?v=xl`,
      og: `${publicBase}/m/${storageKey}?v=og`,
    },
  });
}

async function deliver(request: Request, env: Env, ctx: ExecutionContext): Promise<Response> {
  const url = new URL(request.url);
  const storageKey = decodeURIComponent(url.pathname.slice("/m/".length));
  if (!storageKey || storageKey.includes("..") || storageKey.startsWith("tmp/")) {
    return new Response("Not Found", { status: 404 });
  }

  const variant = (url.searchParams.get("v") || "card").toLowerCase();
  const width = VARIANT_WIDTH[variant] ?? VARIANT_WIDTH.card;
  const cache = caches.default;
  // Include watermark profile in cache key so style changes bust stale immutable entries.
  const cacheUrl = new URL(url.toString());
  cacheUrl.searchParams.set("_wm", "center-soft-v1");
  const cacheKey = new Request(cacheUrl.toString(), request);
  const cached = await cache.match(cacheKey);
  if (cached) {
    return cached;
  }

  const object = await env.MEDIA.get(storageKey);
  if (!object) {
    return new Response("Not Found", { status: 404 });
  }

  const accept = request.headers.get("Accept") || "";
  const format = pickFormat(accept);

  try {
    let pipeline = env.IMAGES.input(await object.arrayBuffer()).transform({
      width,
      fit: variant === "og" ? "cover" : "scale-down",
      height: variant === "og" ? 630 : undefined,
    });

    if (WATERMARK_VARIANTS.has(variant)) {
      const watermarkKey = env.DEFAULT_WATERMARK_KEY || "assets/watermarks/default.png";
      const wm = await env.MEDIA.get(watermarkKey);
      if (wm) {
        // Large horizontal logo, centered, very low opacity (barely visible brand mark).
        const opacity = Number(env.WATERMARK_OPACITY || 0.14);
        const scalePct = Number(env.WATERMARK_SCALE_PCT || 58);
        const wmWidth = Math.max(96, Math.round(width * (scalePct / 100)));
        pipeline = pipeline.draw(
          env.IMAGES.input(await wm.arrayBuffer()).transform({ width: wmWidth }),
          {
            opacity,
            // No top/left/bottom/right → Images binding centers the overlay.
          },
        );
      } else {
        console.error("watermark missing", watermarkKey);
      }
    }

    const output = await pipeline.output({ format, quality: variant === "thumb" ? 70 : 82 });
    const response = output.response();
    const headers = new Headers(response.headers);
    headers.set("Cache-Control", "public, max-age=31536000, immutable");
    headers.set("Vary", "Accept");
    headers.set("X-Media-Variant", variant);
    headers.set("X-Media-Key", storageKey);

    const finalResponse = new Response(response.body, { status: 200, headers });
    ctx.waitUntil(cache.put(cacheKey, finalResponse.clone()));
    return finalResponse;
  } catch (err) {
    console.error("deliver failed", err);
    return new Response("Transform failed", { status: 502 });
  }
}

function pickFormat(accept: string): "image/avif" | "image/webp" | "image/jpeg" {
  if (/image\/avif/i.test(accept)) return "image/avif";
  if (/image\/webp/i.test(accept)) return "image/webp";
  return "image/jpeg";
}

async function sha256Hex(buffer: ArrayBuffer): Promise<string> {
  const hash = await crypto.subtle.digest("SHA-256", buffer);
  return [...new Uint8Array(hash)].map((b) => b.toString(16).padStart(2, "0")).join("");
}

function jsonError(code: string, status: number): Response {
  return Response.json({ error: code }, { status });
}
