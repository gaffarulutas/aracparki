-- Accounts (cookie auth — no ASP.NET Identity framework)
CREATE TABLE IF NOT EXISTS accounts (
    id                  BIGSERIAL PRIMARY KEY,
    email               TEXT NOT NULL,
    password_hash       TEXT NOT NULL,
    first_name          TEXT NOT NULL,
    last_name           TEXT NOT NULL,
    phone               TEXT NULL,
    email_confirmed_at  TIMESTAMPTZ NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT accounts_email_lower UNIQUE (email)
);

CREATE TABLE IF NOT EXISTS password_reset_tokens (
    id           BIGSERIAL PRIMARY KEY,
    account_id   BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    token_hash   TEXT NOT NULL UNIQUE,
    expires_at   TIMESTAMPTZ NOT NULL,
    used_at      TIMESTAMPTZ NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_password_reset_account
    ON password_reset_tokens (account_id, expires_at DESC);

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
