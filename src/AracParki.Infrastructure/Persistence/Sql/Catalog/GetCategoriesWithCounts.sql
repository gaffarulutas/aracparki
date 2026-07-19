SELECT
    c.id,
    c.name,
    c.slug,
    c.icon_key AS IconKey,
    COALESCE(cnt.listing_count, 0)::int AS ListingCount,
    c.group_id AS GroupId,
    g.name AS GroupName
FROM categories c
LEFT JOIN category_groups g ON g.id = c.group_id
LEFT JOIN (
    SELECT category_id, COUNT(*)::int AS listing_count
    FROM listings
    WHERE status = 'published'
    GROUP BY category_id
) cnt ON cnt.category_id = c.id
ORDER BY c.sort_order, c.name;
