SELECT id, name, slug,
       typical_weight_min_t AS TypicalWeightMinT,
       typical_weight_max_t AS TypicalWeightMaxT,
       horsepower AS Horsepower,
       capacity_kg AS CapacityKg,
       capacity_t AS CapacityT,
       default_specs::text AS DefaultSpecsJson
FROM equipment_models
WHERE brand_id = @BrandId
  AND category_id = @CategoryId
ORDER BY name;
