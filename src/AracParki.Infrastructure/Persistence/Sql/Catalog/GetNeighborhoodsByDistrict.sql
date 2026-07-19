SELECT id, name, component_name AS DisplayName
FROM neighborhoods
WHERE district_id = @DistrictId
ORDER BY name
LIMIT @Take;
