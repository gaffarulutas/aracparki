SELECT id, name, slug, capacity_metric AS CapacityMetric, group_id AS GroupId
FROM categories
ORDER BY sort_order, name;
