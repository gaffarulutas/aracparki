---
name: htmx-alpine-js-patterns
description: >-
  Guides hypermedia UI for aracparki.com (ASP.NET Core Razor Pages + HTMX +
  CSP Alpine.js). Use when adding or editing hx-* attributes, Alpine
  components (x-data, Alpine.data, Alpine.store), partial list swaps
  (#list-shell), modals/dropdowns/popovers, htmx lifecycle hooks, OOB swaps,
  or deciding HTMX vs Alpine vs site.js. Also when reviewing interactivity,
  accessibility after swaps, or CSP-safe Alpine patterns.
---

# HTMX + Alpine.js Patterns (aracparki.com)

Server is the **source of truth**. HTMX moves HTML fragments; Alpine owns
ephemeral UI state only. Prefer Razor-rendered HTML over JSON + client render.

Stack in this repo:

| Layer | Location |
|-------|----------|
| Markup | `Pages/**/*.cshtml` |
| Alpine components | `wwwroot/js/site.js` (`alpine:init` → `Alpine.data` / `Alpine.store`) |
| HTMX + Alpine libs | `_Layout.cshtml` (`~/lib/htmx`, `~/lib/alpine`, Focus plugin) |
| CSP | `SecurityHeadersExtensions.cs` (`script-src 'self' 'nonce-…'` — **no** `'unsafe-eval'`) |

## Decision matrix

| Need | Use | Not |
|------|-----|-----|
| Fetch/filter/paginate/navigate list content | HTMX (`hx-get`, `hx-select`, `hx-target`, `hx-push-url`) | `fetch` + DOM paint in Alpine |
| Form POST that returns HTML | HTMX `hx-post` (or full page POST) | Alpine `$fetch` / axios as app data layer |
| Dropdown, popover, modal open/close | Alpine | New HTMX round-trip |
| Tabs / accordion / local toggle | Alpine | Server state |
| Durable data, validation, authz | Server (Razor + Application) | `Alpine.store` as source of truth |
| Complex upload/crop/wizard | Alpine.data in `site.js` | Huge inline `x-data="{…}"` |
| One-off tiny DOM helper | Existing `site.js` patterns | New framework or `_hyperscript` |

**Rule of thumb:** if it must survive refresh or be trusted → server + HTMX.
If it is only “is this menu open?” → Alpine.

## CSP Alpine (mandatory)

This app’s CSP blocks Alpine’s default inline-expression evaluator.

**Do**

```html
<div x-data="listShell" …>
```

```js
// wwwroot/js/site.js — inside alpine:init
Alpine.data("listShell", () => ({
  // methods + state
}));
```

**Do not**

- `x-data="{ open: false }"` or other inline object/expressions in `.cshtml`
- `x-on` / `@click` with complex expressions that need eval
- `$store.foo` / bang-operators in templates when the codebase uses explicit methods
- `x-html` with untrusted content
- `_hyperscript`, `hx-on="…js…"`, or inline `<script>` without nonce

Seed server values via `data-*` attributes; read them in `init()` / `Alpine.data`.

Register every new component in `site.js` under the existing `alpine:init` block.
Reuse Focus plugin (`x-trap` / focus helpers) already loaded in `_Layout`.

## Ownership rules

1. **HTMX** — requests, fragment swaps, history (`hx-push-url`), OOB updates.
2. **Server** — durable state, FluentValidation, permissions, canonical HTML.
3. **Alpine** — open/closed, temporary UI, focus shells, client-only compare list, etc.
4. **Do not** mirror the same durable field in both DB and an Alpine store without a reconciliation plan.

Keep long-lived Alpine roots **outside** HTMX swap targets. If HTMX replaces the
node that owns `x-data`, that state resets (often desirable for list fragments;
bad for chrome/modals).

## Canonical HTMX pattern: list shell

Listing navigation already uses:

```html
hx-get="…"
hx-select="#list-shell"
hx-target="#list-shell"
hx-swap="outerHTML"
hx-push-url="true"
```

Follow this for category/filter/sort/pagination on `/ilanlar` (and similar shells):

- Target a stable shell id (`#list-shell`); do not swap all of `<body>`.
- Prefer `hx-select` so full-page responses still extract the fragment (progressive enhancement + direct URL works).
- Update related chrome with `hx-swap-oob` when needed (see `_ListBreadcrumb`).
- Bind list lifecycle once on `document.body` in `site.js` (`htmx:beforeRequest` / `afterRequest` / `afterSwap`) — do **not** re-bind inside swapped fragments.
- After swap: restore focus (`#list-title`), announce results, re-init lazy images — match `initListEnhancements`.

Prefer `hx-indicator` / `hx-disabled-elt` / `#route-progress` before inventing new spinners.

## Event boundary

Bridge server results → local UI with events, not dual stores:

1. HTMX request → server HTML (+ optional `HX-Trigger` / custom event).
2. Alpine or `site.js` listens and closes a modal, clears busy state, focuses a node.

In Alpine templates, HTMX events are **kebab-case**:
`@htmx:after-request`, not `@htmx:afterRequest`.

Prefer domain names when dispatching (`listing-saved`) over framework names.

## Accessibility

- Real `<button>` for local actions; keep real `<a href>` for navigable HTMX links.
- Sync `aria-expanded` / `aria-controls` / `aria-hidden` from Alpine state.
- Trap focus in modals (Focus plugin); Escape closes overlays.
- After HTMX swap, move focus to a sensible landmark (title / first result) — never leave focus on a detached node.
- Use live regions for result counts when filtering (existing announce helpers).

## Security

- Escape user text in Razor; never trust stored HTML with `hx-*` or event handlers.
- Mutations: auth + antiforgery + server validation (HTMX headers are not auth).
- Do not put secrets or authorization facts in `hx-vals` / `data-*` / Alpine state.
- See CSP notes in `SecurityHeadersExtensions.cs` when adding third-party scripts.

## Avoid

- Restarting Alpine globally after every swap
- Putting listing filters only in Alpine while the URL/server filter diverges
- Broad `document` listeners inside partials that swap repeatedly
- New SPA fetch/render paths next to HTMX for the same UI
- Copy-pasting generic CDN Alpine/HTMX snippets that assume `'unsafe-eval'`

## Workflow checklist

When adding interactivity:

1. Classify: server trip vs local UI (decision matrix).
2. If HTMX: pick target/swap/select; ensure non-HTMX full page still works.
3. If Alpine: add `Alpine.data("name", …)` in `site.js`; markup uses `x-data="name"` only.
4. Keep Alpine root outside replaceable targets when state must persist.
5. Wire a11y + focus after open/close and after swap.
6. Smoke under real CSP (no console CSP violations).

## Additional resources

- Attribute & pattern deep-dives: [htmx-attributes.md](references/htmx-attributes.md), [htmx-patterns.md](references/htmx-patterns.md), [alpine-directives.md](references/alpine-directives.md), [alpine-patterns.md](references/alpine-patterns.md)
- Upstream attribution: [SOURCE.md](references/SOURCE.md)

When upstream examples show inline `x-data="{…}"`, rewrite them to CSP `Alpine.data` for this project.
