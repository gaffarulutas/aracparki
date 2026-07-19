SELECT a.id, a.name, a.slug
FROM listing_attachments la
JOIN attachments a ON a.id = la.attachment_id
WHERE la.listing_id = @ListingId
ORDER BY a.name;
