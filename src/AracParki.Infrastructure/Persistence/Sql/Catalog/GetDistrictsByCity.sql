SELECT id, name
FROM districts
WHERE city_id = @CityId
ORDER BY name;
