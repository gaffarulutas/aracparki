-- Favorites + listing publish expiry (30-day default, set on approve in app).

ALTER TABLE listings
    ADD COLUMN IF NOT EXISTS expires_at TIMESTAMPTZ NULL;

-- Existing live listings: start a fresh 30-day window from listed_at (or now).
UPDATE listings
SET expires_at = COALESCE(listed_at, NOW()) + INTERVAL '30 days'
WHERE status = 'published'
  AND expires_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_listings_published_expires_at
    ON listings (expires_at)
    WHERE status = 'published' AND expires_at IS NOT NULL;

CREATE TABLE IF NOT EXISTS listing_favorites (
    account_id BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    listing_id BIGINT NOT NULL REFERENCES listings (id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (account_id, listing_id)
);

CREATE INDEX IF NOT EXISTS ix_listing_favorites_account_created
    ON listing_favorites (account_id, created_at DESC);

CREATE INDEX IF NOT EXISTS ix_listing_favorites_listing
    ON listing_favorites (listing_id);

-- One saved search per account+url (expression unique).
DELETE FROM saved_searches a
    USING saved_searches b
WHERE a.account_id = b.account_id
  AND a.query_json->>'url' IS NOT DISTINCT FROM b.query_json->>'url'
  AND a.id > b.id;

CREATE UNIQUE INDEX IF NOT EXISTS ux_saved_searches_account_url
    ON saved_searches (account_id, (query_json->>'url'));
