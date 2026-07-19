-- Split display_name into first_name + last_name (idempotent)
ALTER TABLE accounts
    ADD COLUMN IF NOT EXISTS first_name TEXT,
    ADD COLUMN IF NOT EXISTS last_name TEXT;

UPDATE accounts
SET
    first_name = COALESCE(
        NULLIF(trim(first_name), ''),
        NULLIF(split_part(trim(display_name), ' ', 1), ''),
        'Üye'),
    last_name = COALESCE(
        NULLIF(trim(last_name), ''),
        NULLIF(trim(substring(trim(display_name) FROM length(split_part(trim(display_name), ' ', 1)) + 1)), ''),
        '—')
WHERE first_name IS NULL
   OR last_name IS NULL
   OR trim(first_name) = ''
   OR trim(last_name) = '';

ALTER TABLE accounts
    ALTER COLUMN first_name SET NOT NULL,
    ALTER COLUMN last_name SET NOT NULL;

ALTER TABLE accounts
    DROP COLUMN IF EXISTS display_name;
