SELECT phone
FROM listings l
JOIN sellers s ON s.id = l.seller_id
WHERE l.status = 'published'
  AND (l.expires_at IS NULL OR l.expires_at > NOW())
  AND l.ad_no = @AdNo
LIMIT 1;
