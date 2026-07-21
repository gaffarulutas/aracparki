-- Listing price currency (TRY / USD / EUR). Default TRY for existing rows.
ALTER TABLE listings
    ADD COLUMN IF NOT EXISTS currency TEXT NOT NULL DEFAULT 'TRY';

ALTER TABLE listings
    DROP CONSTRAINT IF EXISTS listings_currency_check;

ALTER TABLE listings
    ADD CONSTRAINT listings_currency_check
        CHECK (currency IN ('TRY', 'USD', 'EUR'));
