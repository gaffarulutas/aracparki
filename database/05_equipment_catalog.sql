-- Equipment catalog seed: groups, categories, brands, models, attributes, attachments
BEGIN;

INSERT INTO category_groups (id, name, slug, sort_order) VALUES
 (1, 'Hafriyat', 'earthmoving', 1),
 (2, 'Yol', 'road', 2),
 (3, 'Kaldırma / Elleçleme', 'lifting', 3),
 (4, 'Beton / Kırma', 'concrete', 4)
ON CONFLICT (id) DO UPDATE SET name=EXCLUDED.name, slug=EXCLUDED.slug, sort_order=EXCLUDED.sort_order;

INSERT INTO categories (id, group_id, name, slug, icon_key, sort_order, capacity_metric) VALUES
 (1, 1, 'Paletli Ekskavatör', 'paletli-ekskavator', 'excavator', 1, 'weight'),
 (2, 1, 'Beko Loder', 'beko-loder', 'backhoe', 2, 'weight'),
 (3, 1, 'Lastikli Yükleyici', 'lastikli-yukleyici', 'loader', 3, 'weight'),
 (4, 3, 'Forklift', 'forklift', 'forklift', 4, 'capacity_kg'),
 (5, 3, 'Vinç', 'vinc', 'crane', 5, 'capacity_t'),
 (6, 1, 'Dozer', 'dozer', 'dozer', 6, 'weight'),
 (7, 1, 'Greyder', 'greyder', 'grader', 7, 'weight'),
 (8, 2, 'Silindir', 'silindir', 'roller', 8, 'weight'),
 (9, 1, 'Mini Ekskavatör', 'mini-ekskavator', 'mini', 9, 'weight'),
 (10, 3, 'Telehandler', 'telehandler', 'lift', 10, 'capacity_kg'),
 (11, 4, 'Beton', 'beton', 'concrete', 11, 'capacity_t'),
 (12, 4, 'Kırıcı', 'kirici', 'crusher', 12, 'weight')
ON CONFLICT (id) DO UPDATE SET
  group_id=EXCLUDED.group_id, name=EXCLUDED.name, slug=EXCLUDED.slug,
  icon_key=EXCLUDED.icon_key, sort_order=EXCLUDED.sort_order,
  capacity_metric=EXCLUDED.capacity_metric;

INSERT INTO brands (id, name, slug, sort_order) VALUES
 (1, 'Caterpillar', 'caterpillar', 1),
 (2, 'Komatsu', 'komatsu', 2),
 (3, 'Hitachi', 'hitachi', 3),
 (4, 'Hyundai', 'hyundai', 4),
 (5, 'Volvo', 'volvo', 5),
 (6, 'JCB', 'jcb', 6),
 (7, 'Hidromek', 'hidromek', 7),
 (8, 'Bobcat', 'bobcat', 8),
 (9, 'Liebherr', 'liebherr', 9),
 (10, 'Doosan', 'doosan', 10),
 (11, 'Kubota', 'kubota', 11),
 (12, 'Case', 'case', 12),
 (13, 'New Holland', 'new-holland', 13),
 (14, 'Manitou', 'manitou', 14),
 (15, 'Toyota', 'toyota', 15),
 (16, 'Still', 'still', 16),
 (17, 'Sany', 'sany', 17),
 (18, 'XCMG', 'xcmg', 18)
ON CONFLICT (id) DO UPDATE SET name=EXCLUDED.name, slug=EXCLUDED.slug, sort_order=EXCLUDED.sort_order;

-- category ↔ brand (subset of common pairings)
INSERT INTO category_brands (category_id, brand_id) VALUES
 (1,1),(1,2),(1,3),(1,4),(1,5),(1,7),(1,10),(1,17),(1,18),
 (2,6),(2,7),(2,12),(2,13),(2,1),
 (3,1),(3,2),(3,5),(3,7),(3,10),(3,12),
 (4,15),(4,16),(4,8),(4,6),(4,14),
 (5,9),(5,1),(5,18),(5,17),
 (6,1),(6,2),(6,5),(6,12),
 (7,1),(7,2),(7,5),(7,7),
 (8,1),(8,5),(8,8),(8,12),
 (9,1),(9,8),(9,11),(9,6),(9,10),
 (10,14),(10,6),(10,8),(10,13),
 (11,1),(11,17),(11,18),
 (12,1),(12,17),(12,18)
ON CONFLICT DO NOTHING;

INSERT INTO equipment_models (id, brand_id, category_id, name, slug, typical_weight_min_t, typical_weight_max_t) VALUES
 (1, 1, 1, '320D', '320d', 20, 23),
 (2, 1, 1, '336', '336', 35, 38),
 (3, 1, 1, '312D', '312d', 12, 14),
 (4, 2, 1, 'PC210', 'pc210', 21, 24),
 (5, 2, 1, 'PC360', 'pc360', 35, 38),
 (6, 3, 1, 'ZX210', 'zx210', 21, 23),
 (7, 4, 1, '220LC-9S', '220lc-9s', 21, 23),
 (8, 7, 2, 'HMK 102B', 'hmk-102b', 8, 9),
 (9, 6, 2, '3CX', '3cx', 7, 9),
 (10, 1, 3, '950M', '950m', 18, 20),
 (11, 5, 3, 'L120H', 'l120h', 18, 21),
 (12, 15, 4, '8FBE15', '8fbe15', NULL, NULL),
 (13, 8, 4, 'S175', 's175', NULL, NULL),
 (14, 9, 5, 'LTM 1100', 'ltm-1100', NULL, NULL),
 (15, 1, 6, 'D6T', 'd6t', 18, 22),
 (16, 1, 7, '140M', '140m', 18, 20),
 (17, 5, 8, 'SD115B', 'sd115b', 11, 13),
 (18, 8, 9, 'E35', 'e35', 3, 4),
 (19, 11, 9, 'U55-4', 'u55-4', 5, 6),
 (20, 14, 10, 'MT1840', 'mt1840', NULL, NULL),
 (21, 1, 9, '305.5E2', '305-5e2', 5, 6),
 (22, 7, 1, 'HMK 220 LC', 'hmk-220-lc', 21, 24)
ON CONFLICT (id) DO UPDATE SET
  brand_id=EXCLUDED.brand_id, category_id=EXCLUDED.category_id, name=EXCLUDED.name, slug=EXCLUDED.slug,
  typical_weight_min_t=EXCLUDED.typical_weight_min_t, typical_weight_max_t=EXCLUDED.typical_weight_max_t;

SELECT setval(pg_get_serial_sequence('equipment_models','id'), GREATEST((SELECT MAX(id) FROM equipment_models),1));
SELECT setval(pg_get_serial_sequence('brands','id'), GREATEST((SELECT MAX(id) FROM brands),1));
SELECT setval(pg_get_serial_sequence('categories','id'), GREATEST((SELECT MAX(id) FROM categories),1));
SELECT setval(pg_get_serial_sequence('category_groups','id'), GREATEST((SELECT MAX(id) FROM category_groups),1));

INSERT INTO attachments (id, name, slug) VALUES
 (1, 'Kırıcı', 'breaker'),
 (2, 'Kepçe', 'bucket'),
 (3, 'Grapple', 'grapple'),
 (4, 'Çatal', 'forks'),
 (5, 'Tilt Kepçe', 'tilt-bucket'),
 (6, 'Ripper', 'ripper')
ON CONFLICT (id) DO UPDATE SET name=EXCLUDED.name, slug=EXCLUDED.slug;

-- Excavator attributes
INSERT INTO category_attributes (category_id, key, label, data_type, unit, is_filterable, is_required, sort_order, enum_options) VALUES
 (1, 'undercarriage', 'Yürüyüş', 'enum', NULL, true, false, 1, '["steel_track","rubber_track"]'::jsonb),
 (1, 'tail_swing', 'Kuyruk dönüşü', 'enum', NULL, true, false, 2, '["standard","reduced","zero"]'::jsonb),
 (1, 'breaker_circuit', 'Kırıcı tesisatı', 'bool', NULL, true, false, 3, NULL),
 (1, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 4, NULL),
 (1, 'bucket_volume_m3', 'Kepçe hacmi', 'number', 'm³', false, false, 5, NULL),
 (9, 'undercarriage', 'Yürüyüş', 'enum', NULL, true, false, 1, '["steel_track","rubber_track"]'::jsonb),
 (9, 'tail_swing', 'Kuyruk dönüşü', 'enum', NULL, true, false, 2, '["standard","reduced","zero"]'::jsonb),
 (9, 'breaker_circuit', 'Kırıcı tesisatı', 'bool', NULL, true, false, 3, NULL),
 (9, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 4, NULL),
 (2, 'four_wd', '4x4', 'bool', NULL, true, false, 1, NULL),
 (2, 'extendable_dipper', 'Uzatmalı dipper', 'bool', NULL, true, false, 2, NULL),
 (2, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 3, NULL),
 (3, 'bucket_volume_m3', 'Kova hacmi', 'number', 'm³', true, false, 1, NULL),
 (3, 'articulated', 'Mafsallı', 'bool', NULL, true, false, 2, NULL),
 (4, 'fuel', 'Yakıt', 'enum', NULL, true, true, 1, '["diesel","lpg","electric"]'::jsonb),
 (4, 'mast_type', 'Mast tipi', 'enum', NULL, true, false, 2, '["simplex","duplex","triplex"]'::jsonb),
 (4, 'lift_height_m', 'Kaldırma yüksekliği', 'number', 'm', true, false, 3, NULL),
 (5, 'crane_type', 'Vinç tipi', 'enum', NULL, true, true, 1, '["mobile","crawler","tower"]'::jsonb),
 (5, 'boom_length_m', 'Bom uzunluğu', 'number', 'm', true, false, 2, NULL),
 (6, 'lgp', 'LGP', 'bool', NULL, true, false, 1, NULL),
 (6, 'blade_width_m', 'Bıçak genişliği', 'number', 'm', false, false, 2, NULL),
 (7, 'blade_width_m', 'Bıçak genişliği', 'number', 'm', true, false, 1, NULL),
 (8, 'drum_width_m', 'Tambur genişliği', 'number', 'm', true, false, 1, NULL),
 (8, 'vibration', 'Titreşimli', 'bool', NULL, true, false, 2, NULL),
 (10, 'lift_height_m', 'Kaldırma yüksekliği', 'number', 'm', true, false, 1, NULL),
 (10, 'fuel', 'Yakıt', 'enum', NULL, true, false, 2, '["diesel","lpg","electric"]'::jsonb)
ON CONFLICT (category_id, key) DO UPDATE SET
  label=EXCLUDED.label, data_type=EXCLUDED.data_type, unit=EXCLUDED.unit,
  is_filterable=EXCLUDED.is_filterable, is_required=EXCLUDED.is_required,
  sort_order=EXCLUDED.sort_order, enum_options=EXCLUDED.enum_options;

COMMIT;
