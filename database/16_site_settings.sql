-- Singleton site contact / social settings (admin-editable)

CREATE TABLE IF NOT EXISTS site_settings (
    id                      SMALLINT PRIMARY KEY DEFAULT 1
                            CHECK (id = 1),
    support_email           TEXT NOT NULL,
    support_phone           TEXT NULL,
    whatsapp_phone          TEXT NULL,
    ads_email               TEXT NULL,
    working_hours           TEXT NULL,
    response_note           TEXT NULL,
    company_display_name    TEXT NOT NULL DEFAULT 'Araç Parkı',
    legal_company_name      TEXT NULL,
    address_line            TEXT NULL,
    city                    TEXT NULL,
    postal_code             TEXT NULL,
    footer_tagline          TEXT NULL,
    instagram_url           TEXT NULL,
    facebook_url            TEXT NULL,
    twitter_url             TEXT NULL,
    youtube_url             TEXT NULL,
    linkedin_url            TEXT NULL,
    tiktok_url              TEXT NULL,
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_account_id   BIGINT NULL REFERENCES accounts (id) ON DELETE SET NULL
);

INSERT INTO site_settings (
    id,
    support_email,
    working_hours,
    response_note,
    company_display_name,
    footer_tagline
)
VALUES (
    1,
    'destek@aracparki.com',
    'Hafta içi 09:00–18:00 (Türkiye saati)',
    'İş günlerinde genellikle 1–2 gün içinde dönüş sağlarız.',
    'Araç Parkı',
    'Türkiye genelinde satılık, kiralık ve ikinci el iş makineleri için uzman ilan platformu.'
)
ON CONFLICT (id) DO NOTHING;
