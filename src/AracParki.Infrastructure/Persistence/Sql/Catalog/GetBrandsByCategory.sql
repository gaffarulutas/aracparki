SELECT b.id, b.name, b.slug
FROM brands b
JOIN category_brands cb ON cb.brand_id = b.id
WHERE cb.category_id = @CategoryId
ORDER BY b.sort_order, b.name;
