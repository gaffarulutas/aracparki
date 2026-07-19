SELECT
    b.id::text AS Key,
    b.name AS Label,
    COUNT(*)::int AS Count
FROM listings l
JOIN brands b ON b.id = l.brand_id
WHERE l.status = 'published'
  AND (@CategoryId IS NULL OR l.category_id = @CategoryId)
GROUP BY b.id, b.name
ORDER BY Count DESC, b.name;
