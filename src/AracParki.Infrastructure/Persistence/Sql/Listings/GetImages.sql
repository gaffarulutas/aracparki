SELECT url
FROM listing_images
WHERE listing_id = @ListingId
  AND deleted_at IS NULL
  AND status = 'ready'
ORDER BY sort_order, id;
