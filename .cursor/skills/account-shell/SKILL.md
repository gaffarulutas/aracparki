---
name: account-shell
description: >-
  Enforces aracparki.com account/admin/panel page shell layout, content card
  surface (homepage Son bakılan ilanlar / .vitrin-block style), content width
  (İlanlarım standard), and the required page-head back link. Use when creating
  or editing Panel, Bilgilerim, İlanlarım, Favorilerim, Mesajlarım, Bildirimler,
  Kayıtlı aramalar, Kurumsal hesap, Ayarlar, Admin (/admin/*), or any page using
  Shared/_AccountLayout — or when choosing max-width, container, sidebar,
  content card, form width, or back navigation for account UI.
---

# Account / admin shell (İlanlarım width + content card)

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
    <div class="account-main">
      <div class="account-content-card">
        @RenderBody()
      </div>
    </div>
  </div>
</main>
```

## Content card (required)

Page body always renders inside `.account-content-card` — same surface as the
homepage **Son bakılan ilanlar** block (`.vitrin-block` / `.recent-block`):

| Token | Value |
|-------|--------|
| Background | `var(--raised)` |
| Border | `1px solid var(--line)` |
| Radius | `var(--radius)` |
| Padding | `var(--space-2)` |

**Provided by the layout** — do **not** wrap `@RenderBody()` again in another
`.account-content-card` / `.vitrin-block` on individual pages.

**Page head** (back link + `h1` + lead) stays **inside** the card. The card
styles the head with a machine underline (`border-bottom: 2px solid var(--machine)`),
matching `.vitrin-head`. Do **not** add account/admin breadcrumbs.

**Nested UI** (dash tiles, listing rows, forms, admin detail panels) lives
*inside* the card as **inset tiles** (`background: var(--surface)`, no extra
shadow). Do not restyle children back to `var(--raised)` + `box-shadow` — that
creates white-on-white double cards.

**Forbidden**

- Skipping `_AccountLayout` and hand-rolling a card around account content
- Extra `.vitrin-block` / duplicate `.account-content-card` around the whole page
- Flat body content sitting directly on the page background (no card)
- Nested full white “raised” cards with shadows inside `.account-content-card`

## Back link (required)

Every account/admin page head **must** start with the shared back control:

```cshtml
<header class="page-account-head">
  <partial name="Shared/Partials/_AccountBack" model='("/panel", "Panel")' />
  <h1>…</h1>
  …
</header>
```

| Rule | Detail |
|------|--------|
| Partial | `Shared/Partials/_AccountBack` — model `(string Href, string Label)` |
| Markup | `.account-back` + `arrow-left` icon + destination label |
| Placement | First child inside `.page-account-head`, above title / hero |
| Label | Destination name (not bare “Geri”) — e.g. `Panel`, `Admin`, `Şikayetler` |
| Detail pages | Parent list URL, keep filter query when useful (`?durum=…`) |

**Hierarchy**

| Page | Back to |
|------|---------|
| Panel | `/` · Anasayfa |
| Account section indexes | `/panel` · Panel |
| Admin hub | `/panel` · Panel |
| Admin section indexes | `/admin` · Admin |
| Detail / edit pages | Parent list or section (İlanlar, Şikayetler, Mesajlarım, Kurumsal hesap…) |

**Forbidden**

- Missing back link on any `_AccountLayout` page
- Account breadcrumbs / crumb trails instead of (or in addition to) the back link
- One-off back buttons (`← Geri`, ghost btn) that skip `_AccountBack`
- `history.back()` as the only navigation (breaks deep links / new tabs)

## Width standard (required)

| Layer | Rule | Source |
|-------|------|--------|
| Site shell | `width: min(100% - 32px, var(--max))` | `--max: 1240px` in `tokens.css` |
| Sidebar | `260px` | `--account-sidebar-w` on `.page-account` |
| Content column | **Full** `.account-main` (`minmax(0, 1fr)`) | Same as `/ilanlarim` |
| Content card | `width: 100%` of main | `.account-content-card` |
| Page content | `max-width: none` / full card width | `account.css` |

**Forbidden**

- Page-level `max-width: 42rem` (or similar) on forms, dashboards, lists, admin
- Extra wrapping `.container` inside `account-main` / `account-content-card`
- One-off wider/narrower shells for Admin vs Panel vs İlanlarım

**Allowed**

- Lead/hint text measure (e.g. `.page-account-lead { max-width: 52rem }`)
- Local UI chips (gallery host, empty-state copy) — not the page column
- Two-column *detail* grids inside the card (admin listing/corp detail)

## New page checklist

1. `Layout = "Shared/_AccountLayout"` (gets content card automatically)
2. `SetAccountMeta(...)` or equivalent `PageKey = "account"` (loads `account.css`)
3. Header: `page-account-head` + `_AccountBack` + `h1` + optional `page-account-lead`
4. Body: lists → `account-filter-tabs` / row lists; forms → `corp-form` + `corp-grid`
   at **full card width**; dashboards → `account-dash`
5. Status labels → status-badges skill (`.badge.badge-ok|warn|…`)
6. Do **not** add a second outer content card or a content `max-width` unless
   product explicitly asks to diverge from İlanlarım (document in CSS comment)

## CSS knobs

```css
.page-account {
  --account-sidebar-w: 260px;
  --account-content-max: none; /* keep none = İlanlarım standard */
}

.account-content-card {
  /* homepage .vitrin-block surface — do not restyle per page */
}

.account-back {
  /* shared back link — do not restyle per page */
}
```

Desktop sidebar grid: `responsive.css` `@media (min-width: 960px)`.

## Examples

```cshtml
@* ✅ Layout provides the card; page only fills it *@
@{ Layout = "Shared/_AccountLayout"; }
<header class="page-account-head">
  <partial name="Shared/Partials/_AccountBack" model='("/panel", "Panel")' />
  <h1>İlanlarım</h1>
  <p class="page-account-lead">…</p>
</header>
<form method="post" class="corp-form">…</form>

@* ✅ Detail: back to filtered parent list *@
<header class="page-account-head">
  <partial name="Shared/Partials/_AccountBack"
           model='($"/admin/sikayetler?durum={report.Status}", "Şikayetler")' />
  <h1>Şikayet #@report.Id</h1>
</header>

@* ❌ Duplicate shell card *@
<div class="account-content-card">
  <header class="page-account-head">…</header>
  …
</div>

@* ❌ Missing back / breadcrumb instead of back *@
<header class="page-account-head">
  <partial name="Shared/Partials/_AccountBreadcrumb" />
  <h1>…</h1>
</header>

@* ❌ Settings trap / no layout *@
<form class="corp-form" style="max-width:42rem">…</form>
<div class="container">…</div>
```
