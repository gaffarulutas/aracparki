SELECT COUNT(*)::int
FROM listings
WHERE status = 'published'
  AND (expires_at IS NULL OR expires_at > NOW());
