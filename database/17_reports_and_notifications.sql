-- Listing reports (user complaints) + generic in-app notifications inbox.

CREATE TABLE IF NOT EXISTS listing_reports (
    id                      BIGSERIAL PRIMARY KEY,
    listing_id              BIGINT NOT NULL REFERENCES listings (id) ON DELETE CASCADE,
    ad_no                   TEXT NOT NULL,
    reporter_account_id     BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    reason_code             TEXT NOT NULL,
    message                 TEXT NULL,
    status                  TEXT NOT NULL DEFAULT 'open'
                            CHECK (status IN ('open', 'reviewing', 'actioned', 'dismissed')),
    admin_notes             TEXT NULL,
    reviewed_by_account_id  BIGINT NULL REFERENCES accounts (id) ON DELETE SET NULL,
    reviewed_at             TIMESTAMPTZ NULL,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT ck_listing_reports_message_len
        CHECK (message IS NULL OR char_length(message) <= 250),
    CONSTRAINT ck_listing_reports_admin_notes_len
        CHECK (admin_notes IS NULL OR char_length(admin_notes) <= 1000)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_listing_reports_open_reporter_listing
    ON listing_reports (reporter_account_id, listing_id)
    WHERE status IN ('open', 'reviewing');

CREATE INDEX IF NOT EXISTS ix_listing_reports_status_created
    ON listing_reports (status, created_at DESC);

CREATE INDEX IF NOT EXISTS ix_listing_reports_listing
    ON listing_reports (listing_id);

CREATE TABLE IF NOT EXISTS notifications (
    id          BIGSERIAL PRIMARY KEY,
    account_id  BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    type        TEXT NOT NULL,
    title       TEXT NOT NULL,
    body        TEXT NOT NULL,
    data        JSONB NOT NULL DEFAULT '{}'::jsonb,
    read_at     TIMESTAMPTZ NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_notifications_account_created
    ON notifications (account_id, created_at DESC);

CREATE INDEX IF NOT EXISTS ix_notifications_account_unread
    ON notifications (account_id)
    WHERE read_at IS NULL;
