-- Kurumsal hesaplar (bayi/galeri) + evraklar + ilan bağlantısı.
-- Applied by startup migrator after 11_moderation_hardening.sql.

-- ---------------------------------------------------------------------------
-- corporate_accounts — bir hesap birden fazla kurumsal profil açabilir
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS corporate_accounts (
    id                  BIGSERIAL PRIMARY KEY,
    account_id          BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    -- sahis | limited | anonim | diger
    company_type        TEXT NOT NULL
                        CHECK (company_type IN ('sahis', 'limited', 'anonim', 'diger')),
    trade_name          TEXT NOT NULL,          -- ticaret unvanı (vergi levhasıyla birebir)
    display_name        TEXT NOT NULL,          -- ilanlarda görünen mağaza adı
    tax_office          TEXT NOT NULL,
    tax_number          TEXT NOT NULL,          -- VKN (10) / şahıs şirketinde TCKN (11)
    mersis_no           TEXT NULL,              -- 16 hane, sermaye şirketleri
    trade_registry_no   TEXT NULL,
    kep_address         TEXT NULL,
    authorized_name     TEXT NOT NULL,          -- yetkili kişi ad soyad
    phone               TEXT NOT NULL,
    email               TEXT NOT NULL,
    website             TEXT NULL,
    city_id             INT NOT NULL REFERENCES cities (id),
    district_id         INT NOT NULL REFERENCES districts (id),
    address_line        TEXT NOT NULL,
    logo_url            TEXT NULL,
    status              TEXT NOT NULL DEFAULT 'draft'
                        CHECK (status IN ('draft', 'pending', 'approved', 'rejected')),
    rejection_reason    TEXT NULL,
    submitted_at        TIMESTAMPTZ NULL,
    reviewed_at         TIMESTAMPTZ NULL,
    reviewed_by_account_id BIGINT NULL REFERENCES accounts (id) ON DELETE SET NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_corporate_accounts_account
    ON corporate_accounts (account_id, created_at DESC);

CREATE INDEX IF NOT EXISTS ix_corporate_accounts_status
    ON corporate_accounts (status, submitted_at DESC NULLS LAST, id DESC);

DROP TRIGGER IF EXISTS trg_corporate_accounts_updated_at ON corporate_accounts;
CREATE TRIGGER trg_corporate_accounts_updated_at
    BEFORE UPDATE ON corporate_accounts
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ---------------------------------------------------------------------------
-- corporate_documents — evraklar (private storage key, public URL yok)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS corporate_documents (
    id                    BIGSERIAL PRIMARY KEY,
    corporate_account_id  BIGINT NOT NULL REFERENCES corporate_accounts (id) ON DELETE CASCADE,
    doc_type              TEXT NOT NULL
                          CHECK (doc_type IN (
                              'vergi_levhasi',
                              'imza_sirkuleri',
                              'ticaret_sicil',
                              'faaliyet_belgesi',
                              'yetki_belgesi'
                          )),
    file_name             TEXT NOT NULL,
    storage_key           TEXT NOT NULL,
    content_type          TEXT NOT NULL,
    byte_size             BIGINT NOT NULL CHECK (byte_size > 0),
    uploaded_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at            TIMESTAMPTZ NULL
);

CREATE INDEX IF NOT EXISTS ix_corporate_documents_account
    ON corporate_documents (corporate_account_id, doc_type, uploaded_at DESC)
    WHERE deleted_at IS NULL;

-- ---------------------------------------------------------------------------
-- listings.corporate_account_id — ilan hangi kurumsal hesap adına verildi
-- ---------------------------------------------------------------------------
ALTER TABLE listings
    ADD COLUMN IF NOT EXISTS corporate_account_id BIGINT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'listings_corporate_account_id_fkey'
    ) THEN
        ALTER TABLE listings
            ADD CONSTRAINT listings_corporate_account_id_fkey
            FOREIGN KEY (corporate_account_id) REFERENCES corporate_accounts (id) ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_listings_corporate_account
    ON listings (corporate_account_id)
    WHERE corporate_account_id IS NOT NULL;
