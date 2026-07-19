SELECT
    id,
    key,
    label,
    data_type AS DataType,
    unit,
    is_filterable AS IsFilterable,
    is_required AS IsRequired,
    enum_options::text AS EnumOptionsJson
FROM category_attributes
WHERE category_id = @CategoryId
ORDER BY sort_order, id;
