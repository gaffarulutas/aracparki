# Araç Parkı (.NET)

İş makinesi ilan pazarı — ASP.NET Core Razor Pages, Dapper, PostgreSQL, HTMX, Alpine.js (CSP), FluentValidation, Serilog.

## Mimari

```
src/
  AracParki.Web/               Presentation (Razor + HTMX + Alpine CSP)
  AracParki.Application/       Services, DTOs, validators
  AracParki.Domain/            Constants / entities
  AracParki.Infrastructure/    Dapper, SQL files, Postgres
database/                      Schema + seed (SQL)
```

## Çalıştırma

```bash
cp .env.example .env
docker compose up -d
dotnet run --project src/AracParki.Web --launch-profile http
```

- Uygulama: http://localhost:5245  
- Health: http://localhost:5245/health  

## Database

| File | Purpose |
|------|---------|
| `01_schema.sql` | Tables + indexes (equipment + geo) |
| `02_cities.sql` | 81 cities |
| `03_districts.sql` | 973 districts |
| `04_neighborhoods.sql` | ~73.5k neighborhoods |
| `05_equipment_catalog.sql` | Groups, brands, models, attributes |
| `06_demo.sql` | Sellers + demo listings |

## Auth

| Route | Purpose |
|-------|---------|
| `/giris` | Login (requires confirmed email) |
| `/kayit` | Register (first name, last name, email, password) — phone at first listing |
| `/eposta-dogrula` | Confirm email / resend verification |
| `/sifremi-unuttum` | Forgot password |
| `/sifre-sifirla` | Reset with token |
| `/cikis` | Logout (POST) |

| Legal | Purpose |
|-------|---------|
| `/kullanim-kosullari` | Terms of use |
| `/gizlilik` | Privacy notice |
| `/kvkk` | KVKK disclosure |
| `/guvenli-alisveris` | Safe trading tips |

Cookie auth (no full Identity framework). Schema: `database/07_accounts.sql` (+ email verification migration).

Password rules: min 8 chars, ≥1 letter, ≥1 digit, no triple repeat, must not contain name/email local-part.

Icons: **Lucide** (`lucide-static@1.25.0`, ISC) — vendored SVGs under `wwwroot/lib/lucide/icons/`, rendered via `<ap-icon name="…" />` TagHelper.

- **Intent:** `satilik` \| `kiralik`
- **Condition:** `new` \| `used`
- **Catalog:** category groups → categories → brands → models
- **Specs:** `listings.specs` JSONB + `category_attributes`
- **Attachments:** M2M `listing_attachments`

## API

| Endpoint | Description |
|----------|-------------|
| `GET /api/catalog/categories` | Categories |
| `GET /api/catalog/category-groups` | Groups + children |
| `GET /api/catalog/brands` | All brands |
| `GET /api/catalog/categories/{id}/brands` | Brands by category |
| `GET /api/catalog/categories/{id}/brands/{brandId}/models` | Models |
| `GET /api/catalog/categories/{id}/attributes` | Spec schema |
| `GET /api/catalog/facets/brands` | Brand facet counts |
| `GET /api/locations/...` | City → district → neighborhood → street |

## Test

```bash
dotnet test src/AracParki.slnx
```
