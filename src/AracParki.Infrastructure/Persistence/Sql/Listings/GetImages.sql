SELECT url
FROM listing_images
WHERE listing_id = @ListingId
ORDER BY sort_order, id;
