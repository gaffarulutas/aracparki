SELECT id, name, component_name AS DisplayName
FROM streets
WHERE neighborhood_id = @NeighborhoodId
  AND (@Query IS NULL OR name ILIKE ('%' || @Query || '%') OR component_name ILIKE ('%' || @Query || '%'))
ORDER BY name
LIMIT @Take;
