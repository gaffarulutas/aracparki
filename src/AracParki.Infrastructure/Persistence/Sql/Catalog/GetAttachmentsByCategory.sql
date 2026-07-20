SELECT a.id, a.name, a.slug
FROM attachments a
INNER JOIN category_attachments ca ON ca.attachment_id = a.id
WHERE ca.category_id = @CategoryId
ORDER BY a.name;
