-- Buyer ↔ seller messaging tied to a listing (one thread per listing + buyer).

CREATE TABLE IF NOT EXISTS message_threads (
    id                  BIGSERIAL PRIMARY KEY,
    listing_id          BIGINT NOT NULL REFERENCES listings (id) ON DELETE CASCADE,
    buyer_account_id    BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    seller_account_id   BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    last_message_at     TIMESTAMPTZ NULL,
    buyer_last_read_at  TIMESTAMPTZ NULL,
    seller_last_read_at TIMESTAMPTZ NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT ck_message_threads_parties
        CHECK (buyer_account_id <> seller_account_id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_message_threads_listing_buyer
    ON message_threads (listing_id, buyer_account_id);

CREATE INDEX IF NOT EXISTS ix_message_threads_buyer_last
    ON message_threads (buyer_account_id, last_message_at DESC NULLS LAST);

CREATE INDEX IF NOT EXISTS ix_message_threads_seller_last
    ON message_threads (seller_account_id, last_message_at DESC NULLS LAST);

CREATE TABLE IF NOT EXISTS messages (
    id                  BIGSERIAL PRIMARY KEY,
    thread_id           BIGINT NOT NULL REFERENCES message_threads (id) ON DELETE CASCADE,
    sender_account_id   BIGINT NOT NULL REFERENCES accounts (id) ON DELETE CASCADE,
    body                TEXT NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT ck_messages_body_len
        CHECK (char_length(body) BETWEEN 1 AND 2000)
);

CREATE INDEX IF NOT EXISTS ix_messages_thread_created
    ON messages (thread_id, created_at ASC, id ASC);
