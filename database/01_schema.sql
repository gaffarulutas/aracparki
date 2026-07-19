-- Araç Parkı schema — heavy equipment marketplace (PostgreSQL)
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- ---------------------------------------------------------------------------
-- Category taxonomy
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS category_groups (
    id          SERIAL PRIMARY KEY,
    name        TEXT NOT NULL UNIQUE,
    slug        TEXT NOT NULL UNIQUE,
    sort_order  INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS categories (
    id          SERIAL PRIMARY KEY,
    group_id    INT NULL REFERENCES category_groups (id),
    name        TEXT NOT NULL UNIQUE,
    slug        TEXT NOT NULL UNIQUE,
    icon_key    TEXT NOT NULL,
    sort_order  INT NOT NULL DEFAULT 0,
    -- primary capacity metric for filters/labels: weight | capacity_kg | capacity_t
    capacity_metric TEXT NOT NULL DEFAULT 'weight'
        CHECK (capacity_metric IN ('weight', 'capacity_kg', 'capacity_t'))
);

-- ---------------------------------------------------------------------------
-- Brands / models
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS brands (
    id          SERIAL PRIMARY KEY,
    name        TEXT NOT NULL UNIQUE,
    slug        TEXT NOT NULL UNIQUE,
    sort_order  INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS category_brands (
    category_id INT NOT NULL REFERENCES categories (id) ON DELETE CASCADE,
    brand_id    INT NOT NULL REFERENCES brands (id) ON DELETE CASCADE,
    PRIMARY KEY (category_id, brand_id)
);

CREATE TABLE IF NOT EXISTS equipment_models (
    id                    SERIAL PRIMARY KEY,
    brand_id              INT NOT NULL REFERENCES brands (id),
    category_id           INT NOT NULL REFERENCES categories (id),
    name                  TEXT NOT NULL,
    slug                  TEXT NOT NULL,
    typical_weight_min_t  NUMERIC(8, 2) NULL,
    typical_weight_max_t  NUMERIC(8, 2) NULL,
    UNIQUE (brand_id, category_id, slug)
);

-- ---------------------------------------------------------------------------
-- Category attributes + attachments
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS category_attributes (
    id             SERIAL PRIMARY KEY,
    category_id    INT NOT NULL REFERENCES categories (id) ON DELETE CASCADE,
    key            TEXT NOT NULL,
    label          TEXT NOT NULL,
    data_type      TEXT NOT NULL CHECK (data_type IN ('number', 'bool', 'enum', 'text')),
    unit           TEXT NULL,
    is_filterable  BOOLEAN NOT NULL DEFAULT FALSE,
    is_required    BOOLEAN NOT NULL DEFAULT FALSE,
    sort_order     INT NOT NULL DEFAULT 0,
    enum_options   JSONB NULL,
    UNIQUE (category_id, key)
);

CREATE TABLE IF NOT EXISTS attachments (
    id    SERIAL PRIMARY KEY,
    name  TEXT NOT NULL UNIQUE,
    slug  TEXT NOT NULL UNIQUE
);

-- ---------------------------------------------------------------------------
-- Geo (NVI)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS cities (
    id           INTEGER PRIMARY KEY,
    name         TEXT NOT NULL UNIQUE,
    plate_code   INTEGER NOT NULL UNIQUE,
    is_popular   BOOLEAN NOT NULL DEFAULT FALSE,
    sort_order   INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS districts (
    id            INTEGER PRIMARY KEY,
    city_id       INTEGER NOT NULL REFERENCES cities (id),
    name          TEXT NOT NULL,
    identity_no   INTEGER NOT NULL,
    UNIQUE (city_id, name)
);

CREATE TABLE IF NOT EXISTS neighborhoods (
    id               INTEGER PRIMARY KEY,
    city_id          INTEGER NOT NULL REFERENCES cities (id),
    district_id      INTEGER NOT NULL REFERENCES districts (id),
    name             TEXT NOT NULL,
    component_name   TEXT NOT NULL,
    identity_no      INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS streets (
    id               INTEGER PRIMARY KEY,
    city_id          INTEGER NOT NULL REFERENCES cities (id),
    district_id      INTEGER NOT NULL REFERENCES districts (id),
    neighborhood_id  INTEGER NOT NULL REFERENCES neighborhoods (id),
    name             TEXT NOT NULL,
    component_name   TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS location_meta (
    key   TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

-- ---------------------------------------------------------------------------
-- Sellers / listings
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sellers (
    id            BIGSERIAL PRIMARY KEY,
    display_name  TEXT NOT NULL,
    seller_type   TEXT NOT NULL CHECK (seller_type IN ('dealer', 'owner')),
    is_verified   BOOLEAN NOT NULL DEFAULT FALSE,
    phone         TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS listings (
    id                BIGSERIAL PRIMARY KEY,
    ad_no             TEXT NOT NULL UNIQUE,
    title             TEXT NOT NULL,
    description       TEXT NOT NULL,
    category_id       INT NOT NULL REFERENCES categories (id),
    brand_id          INT NOT NULL REFERENCES brands (id),
    model_id          INT NULL REFERENCES equipment_models (id),
    model_name        TEXT NOT NULL,
    serial_no         TEXT NULL,
    city_id           INT NOT NULL REFERENCES cities (id),
    district_id       INT NOT NULL REFERENCES districts (id),
    seller_id         BIGINT NOT NULL REFERENCES sellers (id),
    -- sale | rent
    primary_intent    TEXT NOT NULL CHECK (primary_intent IN ('satilik', 'kiralik')),
    intents           TEXT[] NOT NULL,
    -- new | used
    condition         TEXT NOT NULL DEFAULT 'used'
                      CHECK (condition IN ('new', 'used')),
    model_year        INT NOT NULL,
    hours             INT NOT NULL,
    tons              NUMERIC(8, 2) NOT NULL,          -- operating weight (t) or capacity_t
    capacity_kg       INT NULL,                       -- forklift primary capacity
    horsepower        INT NOT NULL,
    price             NUMERIC(14, 2) NOT NULL,
    price_unit        TEXT NULL,                      -- day|week|month|hour for rent
    includes_operator BOOLEAN NOT NULL DEFAULT FALSE,
    specs             JSONB NOT NULL DEFAULT '{}'::jsonb,
    cover_image_url   TEXT NOT NULL,
    status            TEXT NOT NULL DEFAULT 'published'
                      CHECK (status IN ('draft', 'published', 'archived')),
    listed_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS listing_images (
    id          BIGSERIAL PRIMARY KEY,
    listing_id  BIGINT NOT NULL REFERENCES listings (id) ON DELETE CASCADE,
    url         TEXT NOT NULL,
    sort_order  INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS listing_attachments (
    listing_id     BIGINT NOT NULL REFERENCES listings (id) ON DELETE CASCADE,
    attachment_id  INT NOT NULL REFERENCES attachments (id) ON DELETE CASCADE,
    PRIMARY KEY (listing_id, attachment_id)
);

CREATE TABLE IF NOT EXISTS saved_searches (
    id           BIGSERIAL PRIMARY KEY,
    name         TEXT NOT NULL,
    query_json   JSONB NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ---------------------------------------------------------------------------
-- Indexes
-- ---------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS ix_listings_status_listed_at
    ON listings (status, listed_at DESC, id DESC);

CREATE INDEX IF NOT EXISTS ix_listings_category_status
    ON listings (category_id, status);

CREATE INDEX IF NOT EXISTS ix_listings_brand_category
    ON listings (brand_id, category_id, status);

CREATE INDEX IF NOT EXISTS ix_listings_city_status
    ON listings (city_id, status);

CREATE INDEX IF NOT EXISTS ix_listings_intents_gin
    ON listings USING GIN (intents);

CREATE INDEX IF NOT EXISTS ix_listings_title_trgm
    ON listings USING GIN (title gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_listings_ad_no_trgm
    ON listings USING GIN (ad_no gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_listings_model_name_trgm
    ON listings USING GIN (model_name gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_listings_serial_trgm
    ON listings USING GIN (serial_no gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_listings_price ON listings (price);
CREATE INDEX IF NOT EXISTS ix_listings_hours ON listings (hours);
CREATE INDEX IF NOT EXISTS ix_listings_tons ON listings (tons);
CREATE INDEX IF NOT EXISTS ix_listings_year ON listings (model_year);
CREATE INDEX IF NOT EXISTS ix_listings_condition ON listings (condition, status);
CREATE INDEX IF NOT EXISTS ix_listings_specs_gin ON listings USING GIN (specs jsonb_path_ops);

CREATE UNIQUE INDEX IF NOT EXISTS ux_listings_serial_no
    ON listings (serial_no) WHERE serial_no IS NOT NULL AND serial_no <> '';

CREATE INDEX IF NOT EXISTS ix_equipment_models_brand_cat
    ON equipment_models (brand_id, category_id);

CREATE INDEX IF NOT EXISTS ix_category_attributes_cat
    ON category_attributes (category_id, sort_order);

CREATE INDEX IF NOT EXISTS ix_districts_city ON districts (city_id);
CREATE INDEX IF NOT EXISTS ix_districts_name ON districts (city_id, name);
CREATE INDEX IF NOT EXISTS ix_neighborhoods_district ON neighborhoods (district_id);
CREATE INDEX IF NOT EXISTS ix_neighborhoods_city ON neighborhoods (city_id);
CREATE INDEX IF NOT EXISTS ix_neighborhoods_name ON neighborhoods (district_id, name);
CREATE INDEX IF NOT EXISTS ix_streets_neighborhood ON streets (neighborhood_id);
CREATE INDEX IF NOT EXISTS ix_streets_district ON streets (district_id);
CREATE INDEX IF NOT EXISTS ix_streets_name_trgm ON streets USING GIN (name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS ix_listing_images_listing ON listing_images (listing_id, sort_order);

INSERT INTO location_meta (key, value) VALUES
    ('source', 'adres.nvi.gov.tr via melihozkara/il-ilce-mahalle-sokak-veritabani'),
    ('data_date', '2026-04-12'),
    ('case', 'title')
ON CONFLICT (key) DO UPDATE SET value = EXCLUDED.value;
