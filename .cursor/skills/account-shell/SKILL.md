---
name: account-shell
description: >-
  Enforces aracparki.com account/admin/panel page shell layout and content
  width (İlanlarım standard). Use when creating or editing Panel, Bilgilerim,
  İlanlarım, Favorilerim, Kayıtlı aramalar, Kurumsal hesap, Admin (/admin/*),
  or any page using Shared/_AccountLayout — or when choosing max-width,
  container, sidebar, or form width for account UI.
---

# Account / admin shell (İlanlarım width)

## Canonical layout

Every signed-in workspace page **must** use:

```cshtml
Layout = "Shared/_AccountLayout";
```

Do **not** invent a second account chrome, nested `.container`, or page-only
max-width that diverges from İlanlarım.

Shell markup (`_AccountLayout.cshtml`):

```html
<main class="container page-account" id="main">
  <div class="account-layout">
    <aside class="account-aside">…nav…</aside>
    <div class="account-main">@RenderBody()</div>
  </div>
</main>
```

## Width standard (required)

| Layer | Rule | Source |
|-------|------|--------|
| Site shell | `width: min(100% - 32px, var(--max))` | `--max: 1240px` in `tokens.css` |
| Sidebar | `260px` | `--account-sidebar-w` on `.page-account` |
| Content column | **Full** `.account-main` (`minmax(0, 1fr)`) | Same as `/ilanlarim` |
| Page content | `max-width: none` / `width: 100%` of main | `account.css` |

**Forbidden**

- Page-level `max-width: 42rem` (or similar) on forms, dashboards, lists, admin
- Extra wrapping `.container` inside `account-main`
- One-off wider/narrower shells for Admin vs Panel vs İlanlarım

**Allowed**

- Lead/hint text measure (e.g. `.page-account-lead { max-width: 52rem }`)
- Local UI chips (gallery host, empty-state copy) — not the page column
- Two-column *detail* grids inside main (admin listing/corp detail) that still
  sit in the same `account-main` column

## New page checklist

1. `Layout = "Shared/_AccountLayout"`
2. `SetAccountMeta(...)` or equivalent `PageKey = "account"` (loads `account.css`)
3. Header: `page-account-head` + breadcrumb + `h1` + optional `page-account-lead`
4. Body: lists → `account-filter-tabs` / row lists; forms → `corp-form` + `corp-grid`
   at **full main width**; dashboards → `account-dash`
5. Status labels → status-badges skill (`.badge.badge-ok|warn|…`)
6. Do not add a content `max-width` unless product explicitly asks to diverge
   from İlanlarım (document the exception in CSS comment)

## CSS knobs

```css
.page-account {
  --account-sidebar-w: 260px;
  --account-content-max: none; /* keep none = İlanlarım standard */
}
```

Desktop sidebar grid: `responsive.css` `@media (min-width: 960px)`.

## Examples

```cshtml
@* ✅ Same width as /ilanlarim *@
<header class="page-account-head">…</header>
<form method="post" class="corp-form">…</form>

@* ❌ Settings trap *@
<form class="corp-form" style="max-width:42rem">…</form>
<div class="container">…</div>
```
