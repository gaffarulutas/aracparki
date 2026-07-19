-- Email confirmation for accounts (idempotent)
ALTER TABLE accounts
    ADD COLUMN IF NOT EXISTS email_confirmed_at TIMESTAMPTZ NULL;

-- Existing accounts stay usable without re-verify
UPDATE accounts
SET email_confirmed_at = COALESCE(email_confirmed_at, created_at)
WHERE email_confirmed_at IS NULL;

CREATE TABLE IF NOT EXISTS email_verification_tokens (
    id           BIGSERIAL PRIMARY KEY,
    account_id   BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    token_hash   TEXT NOT NULL UNIQUE,
    expires_at   TIMESTAMPTZ NOT NULL,
    used_at      TIMESTAMPTZ NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_email_verification_account
    ON email_verification_tokens (account_id, expires_at DESC);
