SELECT id, name, slug,
       brand_id AS BrandId,
       category_id AS CategoryId,
       typical_weight_min_t AS TypicalWeightMinT,
       typical_weight_max_t AS TypicalWeightMaxT,
       horsepower AS Horsepower,
       capacity_kg AS CapacityKg,
       capacity_t AS CapacityT,
       default_specs::text AS DefaultSpecsJson
FROM equipment_models
WHERE id = @Id
LIMIT 1;
