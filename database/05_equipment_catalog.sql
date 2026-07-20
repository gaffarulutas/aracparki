-- Equipment catalog seed: groups, categories, brands, models, attributes, attachments,
-- category_attachments mapping, and OEM model metrics (HP / capacity / default_specs).
-- OEM block between BEGIN/END OEM_MODEL_SPECS is regenerated via:
--   python3 scripts/generate_model_specs.py

BEGIN;

INSERT INTO category_groups (id, name, slug, sort_order) VALUES
 (1, 'Hafriyat', 'earthmoving', 1),
 (2, 'Yol', 'road', 2),
 (3, 'Kaldırma / Elleçleme', 'lifting', 3),
 (4, 'Beton / Kırma', 'concrete', 4)
ON CONFLICT (id) DO UPDATE SET name=EXCLUDED.name, slug=EXCLUDED.slug, sort_order=EXCLUDED.sort_order;

INSERT INTO categories (id, group_id, name, slug, icon_key, sort_order, capacity_metric) VALUES
 -- Hafriyat
 (1, 1, 'Paletli Ekskavatör', 'paletli-ekskavator', 'excavator', 1, 'weight'),
 (13, 1, 'Lastikli Ekskavatör', 'lastikli-ekskavator', 'wheeled', 2, 'weight'),
 (2, 1, 'Beko Loder', 'beko-loder', 'backhoe', 3, 'weight'),
 (3, 1, 'Lastikli Yükleyici', 'lastikli-yukleyici', 'loader', 4, 'weight'),
 (9, 1, 'Mini Ekskavatör', 'mini-ekskavator', 'mini', 5, 'weight'),
 (14, 1, 'Mini Yükleyici', 'mini-yukleyici', 'skid', 6, 'weight'),
 (6, 1, 'Dozer', 'dozer', 'dozer', 7, 'weight'),
 (7, 1, 'Greyder', 'greyder', 'grader', 8, 'weight'),
 (19, 1, 'Damper', 'damper', 'dump', 9, 'capacity_t'),
 -- Yol
 (8, 2, 'Silindir', 'silindir', 'roller', 10, 'weight'),
 (15, 2, 'Finişer', 'finiser', 'paver', 11, 'weight'),
 (20, 2, 'Asfalt Frezesi', 'asfalt-frezesi', 'milling', 12, 'weight'),
 -- Kaldırma
 (4, 3, 'Forklift', 'forklift', 'forklift', 13, 'capacity_kg'),
 (5, 3, 'Vinç', 'vinc', 'crane', 14, 'capacity_t'),
 (10, 3, 'Telehandler', 'telehandler', 'lift', 15, 'capacity_kg'),
 (16, 3, 'Sepetli Platform', 'sepetli-platform', 'platform', 16, 'capacity_kg'),
 -- Beton / Kırma
 (17, 4, 'Transmikser', 'transmikser', 'mixer', 17, 'capacity_t'),
 (18, 4, 'Beton Pompası', 'beton-pompasi', 'pump', 18, 'capacity_t'),
 (11, 4, 'Beton Santrali', 'beton-santrali', 'concrete', 19, 'capacity_t'),
 (12, 4, 'Kırıcı', 'kirici', 'crusher', 20, 'weight')
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
 (18, 'XCMG', 'xcmg', 18),
 (19, 'MST', 'mst', 19),
 (20, 'Dynapac', 'dynapac', 20),
 (21, 'Bomag', 'bomag', 21),
 (22, 'Genie', 'genie', 22),
 (23, 'Putzmeister', 'putzmeister', 23),
 (24, 'Zoomlion', 'zoomlion', 24),
 (25, 'Çukurova', 'cukurova', 25),
 (26, 'Takeuchi', 'takeuchi', 26),
 (27, 'Sumitomo', 'sumitomo', 27),
 (28, 'LiuGong', 'liugong', 28),
 (29, 'Heli', 'heli', 29),
 (30, 'Hangcha', 'hangcha', 30),
 (31, 'Hyster', 'hyster', 31),
 (32, 'Yale', 'yale', 32),
 (33, 'Jungheinrich', 'jungheinrich', 33),
 (34, 'Linde', 'linde', 34),
 (35, 'Merlo', 'merlo', 35),
 (36, 'Potain', 'potain', 36),
 (37, 'Haulotte', 'haulotte', 37),
 (38, 'JLG', 'jlg', 38),
 (39, 'Wirtgen', 'wirtgen', 39),
 (40, 'Vögele', 'vogele', 40),
 (41, 'Atlas Copco', 'atlas-copco', 41),
 (42, 'Kobelco', 'kobelco', 42),
 (43, 'Develon', 'develon', 43),
 (44, 'SDLG', 'sdlg', 44),
 (45, 'Shantui', 'shantui', 45),
 (46, 'SEM', 'sem', 46),
 (47, 'Yanmar', 'yanmar', 47),
 (48, 'Wacker Neuson', 'wacker-neuson', 48),
 (49, 'Terex', 'terex', 49),
 (50, 'Hamm', 'hamm', 50),
 (51, 'Ammann', 'ammann', 51),
 (52, 'Sakai', 'sakai', 52),
 (53, 'Mitsubishi', 'mitsubishi', 53),
 (54, 'Clark', 'clark', 54),
 (55, 'UniCarriers', 'unicarriers', 55),
 (56, 'Crown', 'crown', 56),
 (57, 'EP', 'ep', 57),
 (58, 'Baoli', 'baoli', 58),
 (59, 'Grove', 'grove', 59),
 (60, 'Tadano', 'tadano', 60),
 (61, 'Kato', 'kato', 61),
 (62, 'Palfinger', 'palfinger', 62),
 (63, 'Manitowoc', 'manitowoc', 63),
 (64, 'Dieci', 'dieci', 64),
 (65, 'Magni', 'magni', 65),
 (66, 'Skyjack', 'skyjack', 66),
 (67, 'Sinoboom', 'sinoboom', 67),
 (68, 'Dingli', 'dingli', 68),
 (69, 'Snorkel', 'snorkel', 69),
 (70, 'Socage', 'socage', 70),
 (71, 'Mercedes-Benz', 'mercedes-benz', 71),
 (72, 'MAN', 'man', 72),
 (73, 'Ford Trucks', 'ford-trucks', 73),
 (74, 'IMER-L&T', 'imer-lt', 74),
 (75, 'Schwing-Stetter', 'schwing-stetter', 75),
 (76, 'Cifa', 'cifa', 76),
 (77, 'Betonstar', 'betonstar', 77),
 (78, 'Everdigm', 'everdigm', 78),
 (79, 'Elkon', 'elkon', 79),
 (80, 'Meka', 'meka', 80),
 (81, 'Semix', 'semix', 81),
 (82, 'Constmach', 'constmach', 82),
 (83, 'Simem', 'simem', 83),
 (84, 'Metso', 'metso', 84),
 (85, 'Sandvik', 'sandvik', 85),
 (86, 'Kleemann', 'kleemann', 86),
 (87, 'Powerscreen', 'powerscreen', 87),
 (88, 'Fabo', 'fabo', 88),
 (89, 'General Makina', 'general-makina', 89),
 (90, 'Lonking', 'lonking', 90),
 (91, 'BMC', 'bmc', 91),
 (92, 'Iveco', 'iveco', 92),
 (93, 'Scania', 'scania', 93),
 (94, 'DAF', 'daf', 94),
 (95, 'Bell', 'bell', 95)
ON CONFLICT (id) DO UPDATE SET name=EXCLUDED.name, slug=EXCLUDED.slug, sort_order=EXCLUDED.sort_order;

-- category ↔ brand (common pairings)
DELETE FROM category_brands WHERE category_id = 15 AND brand_id = 39; -- Wirtgen → freze, not finişer

INSERT INTO category_brands (category_id, brand_id) VALUES
 (1,1),(1,2),(1,3),(1,4),(1,5),(1,7),(1,10),(1,17),(1,18),(1,19),(1,27),(1,28),
 (13,1),(13,2),(13,4),(13,5),(13,7),(13,10),(13,19),(13,28),
 (2,6),(2,7),(2,12),(2,13),(2,1),(2,19),(2,25),
 (3,1),(3,2),(3,5),(3,7),(3,10),(3,12),(3,19),(3,25),(3,28),
 (9,1),(9,8),(9,11),(9,6),(9,10),(9,12),(9,26),
 (14,8),(14,6),(14,12),(14,13),(14,1),(14,26),
 (6,1),(6,2),(6,5),(6,12),(6,28),
 (7,1),(7,2),(7,5),(7,7),
 (8,1),(8,5),(8,8),(8,12),(8,20),(8,21),
 (15,1),(15,5),(15,20),(15,21),(15,18),(15,40),
 (4,15),(4,16),(4,8),(4,6),(4,14),(4,29),(4,30),(4,31),(4,32),(4,33),(4,34),
 (5,9),(5,1),(5,18),(5,17),(5,24),(5,36),
 (10,14),(10,6),(10,8),(10,13),(10,35),
 (16,22),(16,6),(16,14),(16,18),(16,37),(16,38),
 (17,5),(17,17),(17,18),(17,24),
 (18,23),(18,17),(18,18),(18,24),(18,9),
 (11,1),(11,17),(11,18),(11,24),
 (12,1),(12,17),(12,18),(12,24),(12,41),
 -- yeni eklenen eşleşmeler
 (1,6),(1,42),(1,43),
 (13,43),
 (2,49),
 (3,18),(3,44),(3,45),(3,46),
 (9,47),(9,48),
 (14,48),
 (6,9),(6,45),
 (7,18),(7,44),(7,45),
 (8,50),(8,51),(8,52),
 (15,17),(15,27),
 (4,53),(4,54),(4,55),(4,56),(4,57),(4,58),
 (5,59),(5,60),(5,61),(5,62),(5,63),
 (10,38),(10,64),(10,65),
 (16,66),(16,67),(16,68),(16,69),(16,70),
 (17,71),(17,72),(17,73),(17,74),(17,75),(17,76),
 (18,75),(18,76),(18,77),(18,78),
 (11,79),(11,80),(11,81),(11,82),(11,83),
 (12,80),(12,82),(12,84),(12,85),(12,86),(12,87),(12,88),(12,89),
 -- Damper / Freze / Lonking
 (19,1),(19,2),(19,5),(19,17),(19,18),(19,49),(19,71),(19,72),(19,73),(19,91),(19,92),(19,93),(19,94),(19,95),
 (20,1),(20,21),(20,39),(20,50),
 (1,90),(3,90),
 -- Sprint gap fill: category bindings
 (1,9),(3,4),(8,7),(10,7),(4,4)
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
 (22, 7, 1, 'HMK 220 LC', 'hmk-220-lc', 21, 24),
 (23, 7, 13, 'HMK 140 W', 'hmk-140-w', 14, 16),
 (24, 5, 13, 'EW160E', 'ew160e', 16, 18),
 (25, 8, 14, 'S650', 's650', 3, 4),
 (26, 6, 14, '190', '190', 2, 3),
 (27, 20, 15, 'SD2550C', 'sd2550c', 18, 22),
 (28, 21, 15, 'BF 800', 'bf-800', 20, 24),
 (29, 22, 16, 'S-65', 's-65', NULL, NULL),
 (30, 23, 18, 'BSA 1409 D', 'bsa-1409-d', NULL, NULL),
 (31, 5, 17, 'FMX 460', 'fmx-460', NULL, NULL),
 (32, 19, 1, 'M330 LC', 'm330-lc', 32, 35),
 (33, 25, 2, '880', '880', 8, 9),
 (34, 26, 9, 'TB260', 'tb260', 5, 6),
 (35, 27, 1, 'SH210-6', 'sh210-6', 21, 23),
 (36, 28, 3, '856H', '856h', 17, 19),
 (37, 29, 4, 'CPCD30', 'cpcd30', NULL, NULL),
 (38, 31, 4, 'H2.5XT', 'h25xt', NULL, NULL),
 (39, 35, 10, 'TF42.7', 'tf42-7', NULL, NULL),
 (40, 36, 5, 'MDT 178', 'mdt-178', NULL, NULL),
 (41, 37, 16, 'HA16 RTJ', 'ha16-rtj', NULL, NULL),
 (42, 40, 15, 'Super 1800-3', 'super-1800-3', 18, 20),
 -- Paletli Ekskavatör (kat 1)
 (43, 1, 1, '320', '320-ng', 22, 25),
 (44, 1, 1, '323', '323', 24, 26),
 (45, 1, 1, '330', '330', 29, 32),
 (46, 1, 1, '349', '349', 46, 49),
 (47, 2, 1, 'PC130-8', 'pc130-8', 13, 14),
 (48, 2, 1, 'PC300LC', 'pc300lc', 32, 34),
 (49, 3, 1, 'ZX130', 'zx130', 13, 15),
 (50, 3, 1, 'ZX350', 'zx350', 34, 36),
 (51, 4, 1, 'HX300', 'hx300', 30, 32),
 (52, 5, 1, 'EC220E', 'ec220e', 22, 24),
 (53, 5, 1, 'EC300E', 'ec300e', 30, 32),
 (54, 7, 1, 'HMK 300 LC', 'hmk-300-lc', 30, 33),
 (55, 7, 1, 'HMK 370 LC', 'hmk-370-lc', 37, 40),
 (56, 6, 1, 'JS220', 'js220', 22, 24),
 (57, 17, 1, 'SY215C', 'sy215c', 22, 24),
 (58, 42, 1, 'SK210LC-10', 'sk210lc-10', 21, 23),
 (59, 42, 1, 'SK260LC-10', 'sk260lc-10', 27, 29),
 (60, 43, 1, 'DX300LCA', 'dx300lca', 29, 31),
 -- Lastikli Ekskavatör (kat 13)
 (61, 1, 13, 'M318', 'm318', 18, 20),
 (62, 5, 13, 'EW210E', 'ew210e', 21, 22),
 (63, 43, 13, 'DX160W-7', 'dx160w-7', 16, 18),
 (64, 7, 13, 'HMK 145 W', 'hmk-145-w', 14, 16),
 -- Beko Loder (kat 2)
 (65, 6, 2, '4CX', '4cx', 8, 9),
 (66, 1, 2, '432', '432', 8, 9),
 (67, 13, 2, 'B110C', 'b110c', 7, 8),
 (68, 12, 2, '580 Super N', '580-super-n', 7, 8),
 (69, 7, 2, 'HMK 102 S', 'hmk-102-s', 8, 9),
 (70, 19, 2, 'M642', 'm642', 8, 9),
 -- Lastikli Yükleyici (kat 3)
 (71, 1, 3, '938M', '938m', 16, 18),
 (72, 5, 3, 'L90H', 'l90h', 14, 16),
 (73, 5, 3, 'L150H', 'l150h', 25, 27),
 (74, 2, 3, 'WA380', 'wa380', 18, 20),
 (75, 18, 3, 'LW500FN', 'lw500fn', 17, 19),
 (76, 44, 3, 'LG958L', 'lg958l', 17, 18),
 (77, 45, 3, 'SL50W', 'sl50w', 17, 18),
 -- Mini Ekskavatör (kat 9)
 (78, 11, 9, 'U35-4', 'u35-4', 3, 4),
 (79, 11, 9, 'KX080-4', 'kx080-4', 8, 9),
 (80, 8, 9, 'E26', 'e26', 2, 3),
 (81, 8, 9, 'E88', 'e88', 8, 9),
 (82, 26, 9, 'TB290', 'tb290', 9, 10),
 (83, 1, 9, '308', '308', 8, 9),
 (84, 47, 9, 'ViO50', 'vio50', 4, 5),
 (85, 47, 9, 'SV100', 'sv100', 9, 10),
 (86, 48, 9, 'ET65', 'et65', 7, 8),
 -- Mini Yükleyici (kat 14)
 (87, 8, 14, 'S450', 's450', 2, 3),
 (88, 8, 14, 'T590', 't590', 3, 4),
 (89, 1, 14, '242D', '242d', 3, 4),
 (90, 6, 14, '155', '155', 3, 4),
 (91, 12, 14, 'SV340', 'sv340', 4, 5),
 -- Dozer (kat 6)
 (92, 1, 6, 'D8', 'd8', 38, 40),
 (93, 2, 6, 'D65', 'd65', 20, 22),
 (94, 45, 6, 'SD22', 'sd22', 23, 24),
 (95, 45, 6, 'SD16', 'sd16', 17, 18),
 -- Greyder (kat 7)
 (96, 1, 7, '120', '120', 13, 15),
 (97, 2, 7, 'GD675', 'gd675', 16, 18),
 (98, 18, 7, 'GR215', 'gr215', 16, 18),
 -- Silindir (kat 8)
 (99, 50, 8, 'HC 120i', 'hc-120i', 12, 13),
 (100, 50, 8, 'HD+ 140i VV', 'hd-140i-vv', 13, 14),
 (101, 51, 8, 'ARS 130', 'ars-130', 12, 13),
 (102, 51, 8, 'ARX 90', 'arx-90', 9, 10),
 (103, 1, 8, 'CS11 GC', 'cs11-gc', 11, 12),
 (104, 1, 8, 'CB13', 'cb13', 13, 14),
 (105, 21, 8, 'BW 211 D-5', 'bw-211-d-5', 11, 12),
 (106, 5, 8, 'DD105', 'dd105', 10, 11),
 -- Finişer (kat 15)
 (107, 1, 15, 'AP555', 'ap555', 15, 16),
 (108, 5, 15, 'P6870D', 'p6870d', 19, 20),
 (109, 40, 15, 'Super 1300-3', 'super-1300-3', 8, 10),
 -- Forklift (kat 4)
 (110, 53, 4, 'FD25N', 'fd25n', NULL, NULL),
 (111, 15, 4, '8FD25', '8fd25', NULL, NULL),
 (112, 34, 4, 'H25', 'h25', NULL, NULL),
 (113, 16, 4, 'RX 60-25', 'rx-60-25', NULL, NULL),
 (114, 33, 4, 'DFG 425', 'dfg-425', NULL, NULL),
 (115, 54, 4, 'C25', 'c25', NULL, NULL),
 (116, 57, 4, 'EFL252', 'efl252', NULL, NULL),
 -- Vinç (kat 5)
 (117, 59, 5, 'GMK5150', 'gmk5150', NULL, NULL),
 (118, 59, 5, 'GMK4100L', 'gmk4100l', NULL, NULL),
 (119, 60, 5, 'ATF 90G-4', 'atf-90g-4', NULL, NULL),
 (120, 9, 5, 'LTM 1070', 'ltm-1070', NULL, NULL),
 (121, 61, 5, 'NK-250', 'nk-250', NULL, NULL),
 (122, 36, 5, 'MCT 85', 'mct-85', NULL, NULL),
 -- Telehandler (kat 10)
 (123, 6, 10, '531-70', '531-70', NULL, NULL),
 (124, 6, 10, '540-170', '540-170', NULL, NULL),
 (125, 14, 10, 'MT 1440', 'mt-1440', NULL, NULL),
 (126, 14, 10, 'MRT 2145', 'mrt-2145', NULL, NULL),
 (127, 64, 10, 'Icarus 40.17', 'icarus-40-17', NULL, NULL),
 (128, 65, 10, 'RTH 6.21', 'rth-6-21', NULL, NULL),
 -- Sepetli Platform (kat 16)
 (129, 22, 16, 'GS-1932', 'gs-1932', NULL, NULL),
 (130, 22, 16, 'Z-45', 'z-45', NULL, NULL),
 (131, 38, 16, '600S', '600s', NULL, NULL),
 (132, 38, 16, '450AJ', '450aj', NULL, NULL),
 (133, 66, 16, 'SJIII 3219', 'sjiii-3219', NULL, NULL),
 (134, 37, 16, 'Compact 12', 'compact-12', NULL, NULL),
 (135, 68, 16, 'GTJZ1012', 'gtjz1012', NULL, NULL),
 -- Transmikser (kat 17)
 (136, 71, 17, 'Arocs', 'arocs', NULL, NULL),
 (137, 72, 17, 'TGS', 'tgs', NULL, NULL),
 (138, 74, 17, '10 m³', 'imer-10m3', NULL, NULL),
 (139, 75, 17, 'AM 9 C', 'am-9-c', NULL, NULL),
 -- Beton Pompası (kat 18)
 (140, 75, 18, 'S 52 SX', 's-52-sx', NULL, NULL),
 (141, 76, 18, 'K47H', 'k47h', NULL, NULL),
 (142, 17, 18, 'SY5530THB', 'sy5530thb', NULL, NULL),
 (143, 77, 18, 'H58-7RZ', 'h58-7rz', NULL, NULL),
 (144, 78, 18, 'ECP56CS', 'ecp56cs', NULL, NULL),
 -- Beton Santrali (kat 11)
 (145, 79, 11, 'Elkomix-120', 'elkomix-120', NULL, NULL),
 (146, 79, 11, 'Mobile Master-60', 'mobile-master-60', NULL, NULL),
 (147, 80, 11, 'M120', 'm120', NULL, NULL),
 (148, 81, 11, '100S4', '100s4', NULL, NULL),
 (149, 82, 11, 'Constmach-120', 'constmach-120', NULL, NULL),
 -- Kırıcı / kırma-eleme (kat 12)
 (150, 84, 12, 'Lokotrack LT106', 'lokotrack-lt106', NULL, NULL),
 (151, 84, 12, 'Lokotrack LT200HPX', 'lokotrack-lt200hpx', NULL, NULL),
 (152, 85, 12, 'QJ341', 'qj341', NULL, NULL),
 (153, 86, 12, 'MC 110 EVO2', 'mc-110-evo2', NULL, NULL),
 (154, 87, 12, 'Premiertrak 450', 'premiertrak-450', NULL, NULL),
 (155, 88, 12, 'MCC-200', 'mcc-200', NULL, NULL),
 (156, 88, 12, 'MJC', 'mjc', NULL, NULL),
 -- Gap fill + Damper/Freze/Lonking
 (157, 10, 1, 'DX225LC', 'dx225lc', 22, 24),
 (158, 10, 1, 'DX300LC', 'dx300lc', 30, 32),
 (159, 10, 13, 'DX140W', 'dx140w', 14, 16),
 (160, 10, 3, 'DL250', 'dl250', 13, 15),
 (161, 10, 9, 'DX85R-3', 'dx85r-3', 8, 9),
 (162, 24, 5, 'ZTC250V', 'ztc250v', NULL, NULL),
 (163, 24, 17, 'ZLJ5318GJB', 'zlj5318gjb', NULL, NULL),
 (164, 24, 18, '56X-6RZ', '56x-6rz', NULL, NULL),
 (165, 24, 11, 'HZS120', 'hzs120', NULL, NULL),
 (166, 24, 12, 'PE600x900', 'pe600x900', NULL, NULL),
 (167, 30, 4, 'CPCD30', 'hc-cpcd30', NULL, NULL),
 (168, 32, 4, 'GDP25VX', 'gdp25vx', NULL, NULL),
 (169, 55, 4, 'DX25', 'uc-dx25', NULL, NULL),
 (170, 56, 4, 'FC 4525', 'fc-4525', NULL, NULL),
 (171, 58, 4, 'KBD25', 'kbd25', NULL, NULL),
 (172, 52, 8, 'SV512D', 'sv512d', 11, 12),
 (173, 52, 8, 'TW350', 'tw350', 3, 4),
 (174, 46, 3, '656D', 'sem-656d', 17, 19),
 (175, 49, 2, '860', 'terex-860', 8, 9),
 (176, 62, 5, 'PK 23500', 'pk-23500', NULL, NULL),
 (177, 63, 5, 'MLC300', 'mlc300', NULL, NULL),
 (178, 67, 16, '1932E', '1932e', NULL, NULL),
 (179, 69, 16, 'S3219E', 's3219e', NULL, NULL),
 (180, 70, 16, 'ForSte 20D', 'forste-20d', NULL, NULL),
 (181, 73, 17, '1848T', 'ford-1848t', NULL, NULL),
 (182, 41, 12, 'Powercrusher PC6', 'powercrusher-pc6', NULL, NULL),
 (183, 83, 11, 'MMM 3000', 'mmm-3000', NULL, NULL),
 (184, 89, 12, 'GNR-120', 'gnr-120', NULL, NULL),
 (185, 18, 1, 'XE215C', 'xe215c', 22, 24),
 (186, 28, 1, 'CLG922E', 'clg922e', 22, 24),
 (187, 7, 3, 'HMK 640 WL', 'hmk-640-wl', 14, 16),
 (188, 12, 3, '821G', '821g', 15, 17),
 (189, 19, 3, 'M544', 'm544', 17, 19),
 (190, 25, 3, '888', 'cukurova-888', 16, 18),
 (191, 1, 5, 'MH3022', 'mh3022', NULL, NULL),
 (192, 17, 5, 'SAC2200', 'sac2200', NULL, NULL),
 (193, 18, 5, 'QY25K5', 'qy25k5', NULL, NULL),
 (194, 5, 6, 'D6', 'volvo-d6', 18, 20),
 (195, 9, 6, 'PR 736', 'pr-736', 20, 22),
 (196, 12, 6, '2050M', '2050m', 20, 22),
 (197, 28, 6, 'CLG842', 'clg842', 18, 20),
 (198, 5, 7, 'G960', 'g960', 16, 18),
 (199, 7, 7, 'HMK 140 MG', 'hmk-140-mg', 14, 16),
 (200, 44, 7, 'G9190', 'g9190', 15, 17),
 (201, 45, 7, 'SG21-B', 'sg21-b', 15, 17),
 (202, 8, 8, 'CTR16', 'ctr16', 1, 2),
 (203, 12, 8, 'DV209', 'dv209', 9, 10),
 (204, 20, 8, 'CA2500D', 'ca2500d', 12, 13),
 (205, 20, 8, 'CC2200', 'cc2200', 8, 9),
 (206, 6, 9, '86C-1', '86c-1', 8, 9),
 (207, 12, 9, 'CX60C', 'cx60c', 6, 7),
 (208, 8, 10, 'TL43.80HF', 'tl43-80hf', NULL, NULL),
 (209, 13, 10, 'TH7.42', 'th7-42', NULL, NULL),
 (210, 38, 10, '1055', 'jlg-1055', NULL, NULL),
 (211, 1, 11, 'CBS-125', 'cbs-125', NULL, NULL),
 (212, 17, 11, 'HZS90V8', 'hzs90v8', NULL, NULL),
 (213, 18, 11, 'HZS120G', 'hzs120g', NULL, NULL),
 (214, 1, 12, 'Mobile Crusher 320', 'mobile-crusher-320', NULL, NULL),
 (215, 17, 12, 'SY900H', 'sy900h', NULL, NULL),
 (216, 18, 12, 'XPE900', 'xpe900', NULL, NULL),
 (217, 80, 12, 'MJ-90', 'meka-mj-90', NULL, NULL),
 (218, 82, 12, 'CJ-90', 'constmach-cj-90', NULL, NULL),
 (219, 2, 13, 'PW160', 'pw160', 16, 18),
 (220, 4, 13, 'HW140', 'hw140', 14, 16),
 (221, 19, 13, 'M200W', 'm200w', 18, 20),
 (222, 28, 13, 'CLG915W', 'clg915w', 14, 16),
 (223, 13, 14, 'L218', 'l218', 2, 3),
 (224, 26, 14, 'TL12V2', 'tl12v2', 3, 4),
 (225, 48, 14, 'SW17', 'sw17', 2, 3),
 (226, 17, 15, 'SAP60C', 'sap60c', 16, 18),
 (227, 18, 15, 'RP953', 'rp953', 18, 20),
 (228, 27, 15, 'HA60W', 'ha60w', 15, 17),
 (229, 6, 16, 'S1930E', 's1930e', NULL, NULL),
 (230, 14, 16, '170 AETJ', '170-aetj', NULL, NULL),
 (231, 18, 16, 'GTJZ1212', 'xcmg-gtjz1212', NULL, NULL),
 (232, 17, 17, 'SY309C', 'sy309c', NULL, NULL),
 (233, 18, 17, 'G12K', 'g12k', NULL, NULL),
 (234, 76, 17, 'SLY 10', 'sly-10', NULL, NULL),
 (235, 9, 18, 'THP 140 H', 'thp-140-h', NULL, NULL),
 (236, 18, 18, 'HB67V', 'hb67v', NULL, NULL),
 (237, 6, 4, 'Teletruk TLT30', 'tlt30', NULL, NULL),
 (238, 14, 4, 'MI 25 D', 'mi-25-d', NULL, NULL),
 (239, 1, 6, 'D5', 'd5', 12, 14),
 (240, 2, 6, 'D85', 'd85', 24, 26),
 (241, 1, 7, '160', 'cat-160', 17, 19),
 (242, 7, 7, 'HMK 600 MG', 'hmk-600-mg', 18, 20),
 (243, 5, 7, 'G930', 'g930', 14, 16),
 (244, 1, 19, '770G', '770g', NULL, NULL),
 (245, 2, 19, 'HD465', 'hd465', NULL, NULL),
 (246, 5, 19, 'A40G', 'a40g', NULL, NULL),
 (247, 18, 19, 'XDA45', 'xda45', NULL, NULL),
 (248, 17, 19, 'SKT90S', 'skt90s', NULL, NULL),
 (249, 71, 19, 'Arocs 4142', 'arocs-4142', NULL, NULL),
 (250, 72, 19, 'TGS 41.400', 'tgs-41400', NULL, NULL),
 (251, 73, 19, '4142D', 'ford-4142d', NULL, NULL),
 (252, 91, 19, 'Tuğra 8x4', 'bmc-tugra-8x4', NULL, NULL),
 (253, 92, 19, 'Trakker 410', 'iveco-trakker-410', NULL, NULL),
 (254, 93, 19, 'G 500 XT', 'scania-g500', NULL, NULL),
 (255, 94, 19, 'CF 480', 'daf-cf-480', NULL, NULL),
 (256, 95, 19, 'B40E', 'bell-b40e', NULL, NULL),
 (257, 49, 19, 'TA400', 'ta400', NULL, NULL),
 (258, 39, 20, 'W 100 CFi', 'w-100-cfi', 18, 20),
 (259, 39, 20, 'W 210 Fi', 'w-210-fi', 30, 35),
 (260, 1, 20, 'PM620', 'pm620', 28, 32),
 (261, 21, 20, 'BM 2000/75', 'bm-2000-75', 28, 32),
 (262, 50, 20, 'HM 3000', 'hm-3000', 25, 28),
 (263, 90, 1, 'CDM6225', 'cdm6225', 22, 24),
 (264, 90, 3, 'CDM856', 'cdm856', 17, 19),
 -- Sprint 1–2 model gap fill (TR ilan / OEM)
 (265, 1, 1, '320GC', '320gc', 20, 22),
 (266, 1, 1, '330GC', '330gc', 28, 31),
 (267, 1, 1, '340', '340', 38, 41),
 (268, 1, 1, '326', '326', 25, 28),
 (269, 1, 3, '966M', '966m', 23, 26),
 (270, 1, 3, '972M', '972m', 25, 28),
 (271, 1, 6, 'D7', 'd7', 25, 28),
 (272, 1, 13, 'M320', 'm320', 19, 21),
 (273, 1, 2, '428', '428', 8, 9),
 (274, 1, 19, '730', '730', NULL, NULL),
 (275, 1, 14, '259D3', '259d3', 4, 5),
 (276, 2, 1, 'PC220LC', 'pc220lc', 23, 25),
 (277, 2, 1, 'PC200', 'pc200', 20, 22),
 (278, 2, 3, 'WA320', 'wa320', 14, 16),
 (279, 2, 3, 'WA470', 'wa470', 23, 26),
 (280, 2, 6, 'D61EX', 'd61ex', 18, 20),
 (281, 3, 1, 'ZX250LC', 'zx250lc', 25, 27),
 (282, 3, 1, 'ZX400LCH', 'zx400lch', 40, 43),
 (283, 3, 1, 'ZX135US', 'zx135us', 14, 16),
 (284, 4, 1, 'HX220A', 'hx220a', 22, 25),
 (285, 4, 1, 'HX260A', 'hx260a', 26, 28),
 (286, 4, 1, 'HX210A', 'hx210a', 21, 23),
 (287, 4, 3, 'HL955', 'hl955', 16, 18),
 (288, 23, 18, 'M36', 'm36', NULL, NULL),
 (289, 23, 18, 'M42-5', 'm42-5', NULL, NULL),
 (290, 6, 10, '535-95', '535-95', NULL, NULL),
 (291, 6, 10, '541-70', '541-70', NULL, NULL),
 (292, 6, 1, 'JS130', 'js130', 13, 15),
 (293, 6, 1, 'JS200', 'js200', 20, 22),
 (294, 7, 1, 'HMK 230 LC', 'hmk-230-lc', 23, 25),
 (295, 7, 1, 'HMK 310 LC', 'hmk-310-lc', 31, 33),
 (296, 7, 1, 'HMK 500 LCHD', 'hmk-500-lchd', 50, 54),
 (297, 7, 2, 'HMK 102B Alpha', 'hmk-102b-alpha', 8, 9),
 (298, 7, 8, 'HMK 110 TS', 'hmk-110-ts', 11, 12),
 (299, 7, 10, 'HTB 4014', 'htb-4014', NULL, NULL),
 (300, 5, 1, 'EC250E', 'ec250e', 25, 28),
 (301, 5, 1, 'EC380E', 'ec380e', 37, 41),
 (302, 5, 3, 'L110H', 'l110h', 18, 21),
 (303, 5, 3, 'L180H', 'l180h', 26, 29),
 (304, 5, 19, 'A30G', 'a30g', NULL, NULL),
 (305, 8, 9, 'E50', 'e50', 5, 6),
 (306, 8, 9, 'E55', 'e55', 5, 6),
 (307, 8, 14, 'S570', 's570', 3, 4),
 (308, 8, 14, 'T76', 't76', 4, 5),
 (309, 22, 16, 'GS-2646', 'gs-2646', NULL, NULL),
 (310, 22, 16, 'Z-60', 'z-60', NULL, NULL),
 (311, 22, 16, 'S-85', 's-85', NULL, NULL),
 (312, 9, 1, 'R938', 'r938', 36, 39),
 (313, 9, 1, 'R956', 'r956', 50, 55),
 (314, 9, 5, 'LTM 1090', 'ltm-1090', NULL, NULL),
 (315, 59, 5, 'RT540E', 'rt540e', NULL, NULL),
 (316, 59, 5, 'RT770E', 'rt770e', NULL, NULL),
 (317, 60, 5, 'GR-500EX', 'gr-500ex', NULL, NULL),
 (318, 60, 5, 'GR-1000EX', 'gr-1000ex', NULL, NULL),
 (319, 11, 9, 'U17', 'u17', 1, 2),
 (320, 11, 9, 'U48-5', 'u48-5', 4, 5),
 (321, 12, 2, '580ST', '580st', 8, 9),
 (322, 12, 1, 'CX210D', 'cx210d', 21, 23),
 (323, 42, 1, 'SK350LC', 'sk350lc', 35, 37),
 (324, 42, 1, 'SK135SR', 'sk135sr', 14, 16),
 (325, 19, 1, 'M220 LC', 'm220-lc', 22, 24),
 (326, 19, 1, 'M300 LC', 'm300-lc', 30, 32),
 (327, 15, 4, '8FG25', '8fg25', NULL, NULL),
 (328, 15, 4, '8FD30', '8fd30', NULL, NULL),
 (329, 40, 15, 'Vision 520i', 'vision-520i', 18, 20),
 (330, 40, 15, 'Super 1900-3i', 'super-1900-3i', 19, 21),
 (331, 39, 20, 'W 150 CF', 'w-150-cf', 20, 24),
 (332, 50, 8, 'HD+ 90i VV', 'hd-90i-vv', 9, 10),
 (333, 20, 8, 'CA3500D', 'ca3500d', 14, 16),
 (334, 14, 10, 'MT1335', 'mt1335', NULL, NULL),
 (335, 14, 10, 'MRT 2550', 'mrt-2550', NULL, NULL),
 (336, 17, 1, 'SY365C', 'sy365c', 36, 38),
 (337, 18, 5, 'XCT55', 'xct55', NULL, NULL),
 (338, 43, 1, 'DX225LCA', 'dx225lca', 22, 24),
 (339, 43, 1, 'DX340LCA', 'dx340lca', 34, 36),
 (340, 4, 1, 'HX480A', 'hx480a', 48, 52),
 (341, 1, 7, '150', 'cat-150', 16, 18),
 (342, 5, 8, 'DD120', 'dd120', 12, 13),
 (343, 24, 5, 'ZTC550V', 'ztc550v', NULL, NULL),
 (344, 36, 5, 'MDT 259', 'mdt-259', NULL, NULL),
 -- Complete remaining brand/category gaps
 (345, 4, 4, '25D-7E', '25d-7e', NULL, NULL),
 (346, 4, 4, '30D-9', '30d-9', NULL, NULL),
 (347, 16, 4, 'RX 20-20', 'rx-20-20', NULL, NULL),
 (348, 29, 4, 'CPCD35', 'heli-cpcd35', NULL, NULL),
 (349, 30, 4, 'CPD30', 'hc-cpd30', NULL, NULL),
 (350, 31, 4, 'H3.0XT', 'h30xt', NULL, NULL),
 (351, 32, 4, 'GLP25VX', 'glp25vx', NULL, NULL),
 (352, 33, 4, 'EFG 425', 'efg-425', NULL, NULL),
 (353, 34, 4, 'E25', 'linde-e25', NULL, NULL),
 (354, 53, 4, 'FD30N', 'fd30n', NULL, NULL),
 (355, 54, 4, 'C30', 'clark-c30', NULL, NULL),
 (356, 55, 4, 'DX30', 'uc-dx30', NULL, NULL),
 (357, 56, 4, 'SC 6020', 'sc-6020', NULL, NULL),
 (358, 57, 4, 'CPD30L1', 'ep-cpd30', NULL, NULL),
 (359, 58, 4, 'KBD30', 'kbd30', NULL, NULL),
 (360, 35, 10, 'TF50.8', 'tf50-8', NULL, NULL),
 (361, 64, 10, 'Icarus 40.14', 'icarus-40-14', NULL, NULL),
 (362, 65, 10, 'RTH 5.21', 'rth-5-21', NULL, NULL),
 (363, 66, 16, 'SJIII 3226', 'sjiii-3226', NULL, NULL),
 (364, 67, 16, 'AB14EJ', 'ab14ej', NULL, NULL),
 (365, 68, 16, 'GTJZ0808', 'gtjz0808', NULL, NULL),
 (366, 69, 16, 'A46JRT', 'a46jrt', NULL, NULL),
 (367, 70, 16, 'ForSte 15A', 'forste-15a', NULL, NULL),
 (368, 37, 16, 'HA20 RTJ', 'ha20-rtj', NULL, NULL),
 (369, 38, 16, '860SJ', '860sj', NULL, NULL),
 (370, 61, 5, 'NK-500E', 'nk-500e', NULL, NULL),
 (371, 62, 5, 'PK 18500', 'pk-18500', NULL, NULL),
 (372, 63, 5, 'MLC650', 'mlc650', NULL, NULL),
 (373, 36, 5, 'MCT 88', 'mct-88', NULL, NULL),
 (374, 77, 18, 'H43-5RZ', 'h43-5rz', NULL, NULL),
 (375, 78, 18, 'ECP42CX', 'ecp42cx', NULL, NULL),
 (376, 23, 18, 'M38-5', 'm38-5', NULL, NULL),
 (377, 75, 18, 'S 36 X', 's-36-x', NULL, NULL),
 (378, 76, 18, 'K41L', 'k41l', NULL, NULL),
 (379, 9, 18, 'THP 160 H', 'thp-160-h', NULL, NULL),
 (380, 74, 17, '12 m³', 'imer-12m3', NULL, NULL),
 (381, 75, 17, 'AM 10 C', 'am-10-c', NULL, NULL),
 (382, 76, 17, 'SLY 12', 'sly-12', NULL, NULL),
 (383, 71, 17, 'Actros', 'actros-mixer', NULL, NULL),
 (384, 72, 17, 'TGX', 'tgx-mixer', NULL, NULL),
 (385, 73, 17, '1833D', 'ford-1833d', NULL, NULL),
 (386, 91, 19, 'Professional 8x4', 'bmc-professional-8x4', NULL, NULL),
 (387, 92, 19, 'X-Way 480', 'iveco-xway-480', NULL, NULL),
 (388, 93, 19, 'R 500', 'scania-r500', NULL, NULL),
 (389, 94, 19, 'XF 480', 'daf-xf-480', NULL, NULL),
 (390, 95, 19, 'B30E', 'bell-b30e', NULL, NULL),
 (391, 71, 19, 'Arocs 3342', 'arocs-3342', NULL, NULL),
 (392, 72, 19, 'TGS 33.360', 'tgs-33360', NULL, NULL),
 (393, 73, 19, '3542D', 'ford-3542d', NULL, NULL),
 (394, 5, 19, 'A25G', 'a25g', NULL, NULL),
 (395, 1, 19, '745', '745', NULL, NULL),
 (396, 2, 19, 'HM300', 'hm300', NULL, NULL),
 (397, 81, 11, '60S3', '60s3', NULL, NULL),
 (398, 83, 11, 'Beto batch 90', 'beto-batch-90', NULL, NULL),
 (399, 79, 11, 'Elkomix-60', 'elkomix-60', NULL, NULL),
 (400, 80, 11, 'MB-60', 'meka-mb-60', NULL, NULL),
 (401, 82, 11, 'Constmach-60', 'constmach-60', NULL, NULL),
 (402, 41, 12, 'Powercrusher PC4', 'powercrusher-pc4', NULL, NULL),
 (403, 84, 12, 'Lokotrack LT1213S', 'lokotrack-lt1213s', NULL, NULL),
 (404, 85, 12, 'QI442', 'qi442', NULL, NULL),
 (405, 86, 12, 'MR 110 EVO2', 'mr-110-evo2', NULL, NULL),
 (406, 87, 12, 'Warrior 1400X', 'warrior-1400x', NULL, NULL),
 (407, 88, 12, 'MCK-90', 'mck-90', NULL, NULL),
 (408, 89, 12, 'GNR-90', 'gnr-90', NULL, NULL),
 (409, 80, 12, 'MJ-110', 'meka-mj-110', NULL, NULL),
 (410, 46, 3, '668C', 'sem-668c', 19, 21),
 (411, 44, 3, 'LG956L', 'lg956l', 16, 18),
 (412, 45, 3, 'SL30W', 'sl30w', 10, 12),
 (413, 28, 3, 'CLG856H', 'clg856h', 17, 19),
 (414, 25, 3, '890', 'cukurova-890', 17, 19),
 (415, 25, 2, '885', 'cukurova-885', 8, 9),
 (416, 19, 3, 'M300', 'mst-m300', 12, 14),
 (417, 19, 2, 'M644', 'm644', 8, 9),
 (418, 1, 13, 'M322', 'm322', 21, 23),
 (419, 2, 13, 'PW180', 'pw180', 17, 19),
 (420, 4, 13, 'HW210', 'hw210', 20, 22),
 (421, 7, 13, 'HMK 200 W', 'hmk-200-w', 19, 21),
 (422, 10, 13, 'DX190W', 'dx190w', 18, 20),
 (423, 5, 13, 'EW180E', 'ew180e', 17, 19),
 (424, 1, 7, '140', 'cat-140', 15, 17),
 (425, 2, 7, 'GD555', 'gd555', 14, 16),
 (426, 5, 7, 'G940', 'g940', 15, 17),
 (427, 7, 7, 'HMK 300 MG', 'hmk-300-mg', 15, 17),
 (428, 18, 7, 'GR180', 'gr180', 14, 16),
 (429, 44, 7, 'G9138', 'g9138', 13, 15),
 (430, 45, 7, 'SG16-3', 'sg16-3', 13, 15),
 (431, 1, 6, 'D4', 'd4', 12, 14),
 (432, 1, 6, 'D9', 'd9', 48, 52),
 (433, 2, 6, 'D51', 'd51', 12, 14),
 (434, 5, 6, 'D8', 'volvo-d8', 35, 38),
 (435, 9, 6, 'PR 766', 'pr-766', 45, 50),
 (436, 12, 6, '1650M', '1650m', 17, 19),
 (437, 45, 6, 'SD32', 'sd32', 35, 38),
 (438, 1, 9, '303.5', '303-5', 3, 4),
 (439, 1, 9, '307.5', '307-5', 7, 8),
 (440, 8, 9, 'E10', 'e10', 1, 2),
 (441, 8, 9, 'E20', 'e20', 2, 3),
 (442, 6, 9, '67C-1', '67c-1', 6, 7),
 (443, 11, 9, 'KX057-5', 'kx057-5', 5, 6),
 (444, 26, 9, 'TB240', 'tb240', 4, 5),
 (445, 47, 9, 'ViO80', 'vio80', 8, 9),
 (446, 48, 9, 'ET90', 'et90', 9, 10),
 (447, 10, 9, 'DX60R', 'dx60r', 6, 7),
 (448, 1, 14, '262D3', '262d3', 3, 4),
 (449, 1, 14, '289D3', '289d3', 4, 5),
 (450, 8, 14, 'S770', 's770', 4, 5),
 (451, 8, 14, 'T66', 't66', 3, 4),
 (452, 6, 14, '205', 'jcb-205', 3, 4),
 (453, 12, 14, 'SV280', 'sv280', 3, 4),
 (454, 13, 14, 'L230', 'l230', 3, 4),
 (455, 26, 14, 'TL10V2', 'tl10v2', 3, 4),
 (456, 48, 14, 'SW21', 'sw21', 3, 4),
 (457, 1, 8, 'CS12 GC', 'cs12-gc', 12, 13),
 (458, 1, 8, 'CB10', 'cb10', 10, 11),
 (459, 5, 8, 'SD110B', 'sd110b', 11, 12),
 (460, 21, 8, 'BW 213 D-5', 'bw-213-d-5', 13, 14),
 (461, 21, 8, 'BW 161 AD-5', 'bw-161-ad-5', 9, 10),
 (462, 20, 8, 'CC4200', 'cc4200', 10, 12),
 (463, 50, 8, 'HD+ 120i VV', 'hd-120i-vv', 12, 13),
 (464, 51, 8, 'ARS 200', 'ars-200', 18, 20),
 (465, 51, 8, 'ARX 110', 'arx-110', 10, 11),
 (466, 52, 8, 'SW850', 'sw850', 8, 9),
 (467, 8, 8, 'CTR24', 'ctr24', 2, 3),
 (468, 12, 8, 'DV36', 'dv36', 3, 4),
 (469, 7, 8, 'HMK 135 TS', 'hmk-135-ts', 13, 14),
 (470, 1, 15, 'AP655', 'ap655', 17, 19),
 (471, 5, 15, 'P4820D', 'p4820d', 15, 17),
 (472, 21, 15, 'BF 600', 'bf-600', 16, 18),
 (473, 20, 15, 'SD2500CS', 'sd2500cs', 16, 18),
 (474, 18, 15, 'RP903', 'rp903', 16, 18),
 (475, 17, 15, 'SAP45C', 'sap45c', 12, 14),
 (476, 27, 15, 'HA90C', 'ha90c', 18, 20),
 (477, 39, 20, 'W 200 Fi', 'w-200-fi', 28, 32),
 (478, 1, 20, 'PM310', 'pm310', 18, 22),
 (479, 21, 20, 'BM 500/15', 'bm-500-15', 12, 15),
 (480, 50, 20, 'HM 2200', 'hm-2200', 20, 24),
 (481, 1, 1, '315', '315', 15, 17),
 (482, 1, 1, '352', '352', 50, 54),
 (483, 2, 1, 'PC138US', 'pc138us', 14, 16),
 (484, 2, 1, 'PC490', 'pc490', 48, 52),
 (485, 3, 1, 'ZX85USB', 'zx85usb', 8, 9),
 (486, 3, 1, 'ZX490LCH', 'zx490lch', 48, 52),
 (487, 5, 1, 'EC140E', 'ec140e', 14, 16),
 (488, 5, 1, 'EC480E', 'ec480e', 48, 52),
 (489, 6, 1, 'JS160', 'js160', 16, 18),
 (490, 6, 1, 'JS370', 'js370', 36, 39),
 (491, 7, 1, 'HMK 140 LC', 'hmk-140-lc', 14, 16),
 (492, 10, 1, 'DX140LC', 'dx140lc', 14, 16),
 (493, 10, 1, 'DX420LC', 'dx420lc', 42, 45),
 (494, 17, 1, 'SY135C', 'sy135c', 13, 15),
 (495, 17, 1, 'SY500H', 'sy500h', 48, 52),
 (496, 18, 1, 'XE370C', 'xe370c', 36, 38),
 (497, 24, 1, 'ZE215E', 'ze215e', 21, 23),
 (498, 28, 1, 'CLG936E', 'clg936e', 35, 37),
 (499, 42, 1, 'SK75', 'sk75', 7, 8),
 (500, 43, 1, 'DX140LCA', 'dx140lca', 14, 16),
 (501, 43, 13, 'DX210W', 'dx210w', 20, 22),
 (502, 90, 1, 'CDM6485', 'cdm6485', 48, 52),
 (503, 1, 3, '926M', '926m', 13, 15),
 (504, 1, 3, '980M', '980m', 30, 33),
 (505, 2, 3, 'WA200', 'wa200', 11, 13),
 (506, 2, 3, 'WA500', 'wa500', 30, 33),
 (507, 5, 3, 'L60H', 'l60h', 11, 13),
 (508, 5, 3, 'L220H', 'l220h', 32, 35),
 (509, 7, 3, 'HMK 102 WL', 'hmk-102-wl', 8, 10),
 (510, 10, 3, 'DL420', 'dl420', 22, 25),
 (511, 12, 3, '921G', '921g', 13, 15),
 (512, 12, 3, '1121G', '1121g', 22, 25),
 (513, 18, 3, 'LW300FN', 'lw300fn', 10, 12),
 (514, 90, 3, 'CDM835', 'cdm835', 11, 13),
 (515, 6, 2, '3CX Super', '3cx-super', 8, 9),
 (516, 6, 2, '1CX', '1cx', 2, 3),
 (517, 1, 2, '444', '444', 9, 10),
 (518, 7, 2, 'HMK 102B Plus', 'hmk-102b-plus', 8, 9),
 (519, 12, 2, '590ST', '590st', 9, 10),
 (520, 13, 2, 'B115B', 'b115b', 8, 9),
 (521, 49, 2, '880', 'terex-880', 8, 9),
 (522, 6, 10, '540-140', '540-140', NULL, NULL),
 (523, 6, 10, '533-105', '533-105', NULL, NULL),
 (524, 14, 10, 'MT933', 'mt933', NULL, NULL),
 (525, 8, 10, 'TL30.70', 'tl30-70', NULL, NULL),
 (526, 13, 10, 'TH9.35', 'th9-35', NULL, NULL),
 (527, 38, 10, '1255', 'jlg-1255', NULL, NULL),
 (528, 1, 5, 'GHG145', 'ghg145', NULL, NULL),
 (529, 17, 5, 'STC500', 'stc500', NULL, NULL),
 (530, 18, 5, 'QY50KD', 'qy50kd', NULL, NULL),
 (531, 24, 5, 'ZTC300V', 'ztc300v', NULL, NULL),
 (532, 59, 5, 'GMK6300L', 'gmk6300l', NULL, NULL),
 (533, 9, 5, 'LTM 1300', 'ltm-1300', NULL, NULL),
 (534, 22, 16, 'GS-3246', 'gs-3246', NULL, NULL),
 (535, 22, 16, 'Z-30/20N', 'z-30-20n', NULL, NULL),
 (536, 37, 16, 'Compact 8', 'compact-8', NULL, NULL),
 (537, 38, 16, '1930ES', '1930es', NULL, NULL),
 (538, 13, 10, 'TH5.26', 'th5-26', NULL, NULL),
 (539, 27, 1, 'SH350-6', 'sh350-6', 34, 36),
 (540, 26, 9, 'TB216', 'tb216', 1, 2)
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
 (6, 'Ripper', 'ripper'),
 (7, 'Süpürge', 'sweeper'),
 (8, 'Auger', 'auger'),
 (9, 'Pulverizer', 'pulverizer'),
 (10, 'Makas', 'shear'),
 (11, 'Mıknatıs', 'magnet'),
 (12, 'Mulcher', 'mulcher'),
 (13, 'Beton Kovası', 'concrete-bucket'),
 (14, 'Palet Çatalı', 'pallet-forks'),
 (15, 'Başparmak', 'thumb'),
 (16, 'İskelet Kepçe', 'skeleton-bucket'),
 (17, 'Şev Kepçesi', 'ditching-bucket'),
 (18, 'Eleme Kepçesi', 'screening-bucket'),
 (19, 'Midye Kepçe', 'clamshell'),
 (20, 'Tomruk Grapple', 'log-grapple'),
 (21, 'Kar Bıçağı', 'snow-blade'),
 (22, 'Trencher', 'trencher'),
 (23, 'Plaka Kompaktör', 'compactor-plate'),
 (24, 'Asfalt Freze Ataşmanı', 'cold-planer'),
 (25, 'Personel Sepeti', 'man-basket'),
 (26, 'Vinç Jibi', 'crane-jib'),
 (27, 'Matkap', 'rock-drill'),
 (28, 'Çim Biçme / Flail', 'flail-mower'),
 (29, 'Karıştırıcı Kepçe', 'mixing-bucket'),
 (30, 'Dozer Bıçağı', 'dozer-blade')
ON CONFLICT (id) DO UPDATE SET name=EXCLUDED.name, slug=EXCLUDED.slug;

SELECT setval(pg_get_serial_sequence('attachments','id'), GREATEST((SELECT MAX(id) FROM attachments),1));

-- Drop redundant / superseded attribute keys (re-seeded below)
DELETE FROM category_attributes
 WHERE (category_id, key) IN (
   (15, 'max_paving_width_m'),
   (16, 'platform_height_m')
 );

-- Category attributes
INSERT INTO category_attributes (category_id, key, label, data_type, unit, is_filterable, is_required, sort_order, enum_options) VALUES
 -- Paletli Ekskavatör (1)
 (1, 'undercarriage', 'Yürüyüş', 'enum', NULL, true, false, 1, '["steel_track","rubber_track"]'::jsonb),
 (1, 'tail_swing', 'Kuyruk dönüşü', 'enum', NULL, true, false, 2, '["standard","reduced","zero"]'::jsonb),
 (1, 'breaker_circuit', 'Kırıcı tesisatı', 'bool', NULL, true, false, 3, NULL),
 (1, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 4, NULL),
 (1, 'bucket_volume_m3', 'Kepçe hacmi', 'number', 'm³', false, false, 5, NULL),
 (1, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 6, NULL),
 (1, 'digging_depth_m', 'Kazma derinliği', 'number', 'm', true, false, 7, NULL),
 (1, 'quick_coupler', 'Hızlı kova bağlantısı', 'bool', NULL, true, false, 8, NULL),
 (1, 'long_reach', 'Uzun erişimli (LR)', 'bool', NULL, true, false, 9, NULL),
 (1, 'boom_type', 'Bom tipi', 'enum', NULL, true, false, 10, '["mono","two_piece"]'::jsonb),
 (1, 'gps_grade', 'GPS / eğim kontrolü', 'bool', NULL, true, false, 11, NULL),
 -- Lastikli Ekskavatör (13)
 (13, 'breaker_circuit', 'Kırıcı tesisatı', 'bool', NULL, true, false, 1, NULL),
 (13, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 2, NULL),
 (13, 'bucket_volume_m3', 'Kepçe hacmi', 'number', 'm³', false, false, 3, NULL),
 (13, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 4, NULL),
 (13, 'quick_coupler', 'Hızlı kova bağlantısı', 'bool', NULL, true, false, 5, NULL),
 (13, 'outriggers', 'Stabilizatör ayak', 'bool', NULL, true, false, 6, NULL),
 (13, 'digging_depth_m', 'Kazma derinliği', 'number', 'm', true, false, 7, NULL),
 (13, 'four_wd', '4x4', 'bool', NULL, true, false, 8, NULL),
 (13, 'blade', 'Ön bıçak', 'bool', NULL, true, false, 9, NULL),
 -- Mini Ekskavatör (9)
 (9, 'undercarriage', 'Yürüyüş', 'enum', NULL, true, false, 1, '["steel_track","rubber_track"]'::jsonb),
 (9, 'tail_swing', 'Kuyruk dönüşü', 'enum', NULL, true, false, 2, '["standard","reduced","zero"]'::jsonb),
 (9, 'breaker_circuit', 'Kırıcı tesisatı', 'bool', NULL, true, false, 3, NULL),
 (9, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 4, NULL),
 (9, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 5, NULL),
 (9, 'quick_coupler', 'Hızlı kova bağlantısı', 'bool', NULL, true, false, 6, NULL),
 (9, 'cabin_type', 'Kabin tipi', 'enum', NULL, true, false, 7, '["canopy","closed_cabin"]'::jsonb),
 (9, 'bucket_volume_m3', 'Kepçe hacmi', 'number', 'm³', false, false, 8, NULL),
 (9, 'digging_depth_m', 'Kazma derinliği', 'number', 'm', true, false, 9, NULL),
 (9, 'dozer_blade', 'Dozer bıçağı', 'bool', NULL, true, false, 10, NULL),
 -- Beko Loder (2)
 (2, 'four_wd', '4x4', 'bool', NULL, true, false, 1, NULL),
 (2, 'extendable_dipper', 'Uzatmalı dipper', 'bool', NULL, true, false, 2, NULL),
 (2, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 3, NULL),
 (2, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 4, NULL),
 (2, 'side_shift', 'Yana kayar kazıcı', 'bool', NULL, true, false, 5, NULL),
 (2, 'front_bucket_m3', 'Ön kova hacmi', 'number', 'm³', true, false, 6, NULL),
 (2, 'rear_bucket_m3', 'Arka kova hacmi', 'number', 'm³', true, false, 7, NULL),
 (2, 'quick_coupler', 'Hızlı kova bağlantısı', 'bool', NULL, true, false, 8, NULL),
 (2, 'transmission', 'Şanzıman', 'enum', NULL, true, false, 9, '["powershift","synchroshuttle","hydrostatic"]'::jsonb),
 -- Lastikli Yükleyici (3)
 (3, 'bucket_volume_m3', 'Kova hacmi', 'number', 'm³', true, false, 1, NULL),
 (3, 'articulated', 'Mafsallı', 'bool', NULL, true, false, 2, NULL),
 (3, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 3, NULL),
 (3, 'lift_capacity_kg', 'Kaldırma kapasitesi', 'number', 'kg', true, false, 4, NULL),
 (3, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 5, NULL),
 (3, 'quick_coupler', 'Hızlı kova bağlantısı', 'bool', NULL, true, false, 6, NULL),
 (3, 'linkage', 'Kaldırma kolu', 'enum', NULL, true, false, 7, '["z_bar","parallel","torque_parallel"]'::jsonb),
 (3, 'differential_lock', 'Diferansiyel kilidi', 'bool', NULL, true, false, 8, NULL),
 -- Mini Yükleyici (14)
 (14, 'rated_operating_capacity_kg', 'Kaldırma kapasitesi', 'number', 'kg', true, false, 1, NULL),
 (14, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 2, NULL),
 (14, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 3, NULL),
 (14, 'track_type', 'Yürüyüş tipi', 'enum', NULL, true, false, 4, '["wheel","track"]'::jsonb),
 (14, 'auxiliary_hydraulics', 'Ek hidrolik hat', 'bool', NULL, true, false, 5, NULL),
 (14, 'lift_type', 'Kaldırma tipi', 'enum', NULL, true, false, 6, '["radial","vertical"]'::jsonb),
 (14, 'high_flow', 'Yüksek debili hidrolik', 'bool', NULL, true, false, 7, NULL),
 (14, 'quick_coupler', 'Hızlı bağlantı', 'bool', NULL, true, false, 8, NULL),
 -- Dozer (6)
 (6, 'lgp', 'LGP (geniş palet)', 'bool', NULL, true, false, 1, NULL),
 (6, 'blade_width_m', 'Bıçak genişliği', 'number', 'm', false, false, 2, NULL),
 (6, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 3, NULL),
 (6, 'blade_type', 'Bıçak tipi', 'enum', NULL, true, false, 4, '["straight_s","semi_u","u_blade","angle_pat"]'::jsonb),
 (6, 'rear_ripper', 'Arka riper', 'bool', NULL, true, false, 5, NULL),
 (6, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 6, NULL),
 (6, 'blade_capacity_m3', 'Bıçak hacmi', 'number', 'm³', true, false, 7, NULL),
 (6, 'winch', 'Vinç', 'bool', NULL, true, false, 8, NULL),
 (6, 'gps_grade', 'GPS / eğim kontrolü', 'bool', NULL, true, false, 9, NULL),
 -- Greyder (7)
 (7, 'blade_width_m', 'Bıçak genişliği', 'number', 'm', true, false, 1, NULL),
 (7, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 2, NULL),
 (7, 'awd', 'Tüm tekerlek tahrik', 'bool', NULL, true, false, 3, NULL),
 (7, 'front_scarifier', 'Ön riper/scarifier', 'bool', NULL, true, false, 4, NULL),
 (7, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 5, NULL),
 (7, 'moldboard_length_m', 'Bıçak uzunluğu', 'number', 'm', true, false, 6, NULL),
 (7, 'rear_ripper', 'Arka riper', 'bool', NULL, true, false, 7, NULL),
 (7, 'gps_grade', 'GPS / eğim kontrolü', 'bool', NULL, true, false, 8, NULL),
 (7, 'articulation', 'Mafsallı şasi', 'bool', NULL, true, false, 9, NULL),
 -- Damper (19)
 (19, 'dump_type', 'Damper tipi', 'enum', NULL, true, true, 1, '["rigid","articulated","truck_tipper"]'::jsonb),
 (19, 'payload_t', 'Yük kapasitesi', 'number', 't', true, false, 2, NULL),
 (19, 'axle_config', 'Aks konfigürasyonu', 'enum', NULL, true, false, 3, '["4x2","6x4","8x4","6x6"]'::jsonb),
 (19, 'body_volume_m3', 'Kasa hacmi', 'number', 'm³', true, false, 4, NULL),
 (19, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 5, NULL),
 (19, 'bed_length_m', 'Kasa uzunluğu', 'number', 'm', false, false, 6, NULL),
 (19, 'heated_body', 'Isıtmalı kasa', 'bool', NULL, true, false, 7, NULL),
 (19, 'tailgate', 'Arka kapak', 'enum', NULL, true, false, 8, '["none","manual","hydraulic"]'::jsonb),
 -- Silindir (8)
 (8, 'drum_width_m', 'Tambur genişliği', 'number', 'm', true, false, 1, NULL),
 (8, 'vibration', 'Titreşimli', 'bool', NULL, true, false, 2, NULL),
 (8, 'roller_type', 'Silindir tipi', 'enum', NULL, true, false, 3, '["single_drum_soil","tandem_asphalt","pneumatic","combi"]'::jsonb),
 (8, 'padfoot', 'Keçiayağı tambur', 'bool', NULL, true, false, 4, NULL),
 (8, 'oscillation', 'Osilasyonlu tambur', 'bool', NULL, true, false, 5, NULL),
 (8, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 6, NULL),
 (8, 'water_spray', 'Su püskürtme', 'bool', NULL, true, false, 7, NULL),
 (8, 'drum_diameter_m', 'Tambur çapı', 'number', 'm', false, false, 8, NULL),
 -- Finişer (15)
 (15, 'paving_width_m', 'Serme genişliği', 'number', 'm', true, false, 1, NULL),
 (15, 'undercarriage', 'Yürüyüş', 'enum', NULL, true, false, 2, '["track","wheel"]'::jsonb),
 (15, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 3, NULL),
 (15, 'hopper_capacity_t', 'Hazne kapasitesi', 'number', 't', true, false, 4, NULL),
 (15, 'max_paving_thickness_mm', 'Maks. serme kalınlığı', 'number', 'mm', true, false, 5, NULL),
 (15, 'screed_type', 'Tabla tipi (screed)', 'enum', NULL, true, false, 6, '["tamper","vibrator","high_compaction"]'::jsonb),
 (15, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 7, NULL),
 -- Asfalt Frezesi (20)
 (20, 'milling_width_m', 'Freze genişliği', 'number', 'm', true, true, 1, NULL),
 (20, 'milling_depth_mm', 'Freze derinliği', 'number', 'mm', true, false, 2, NULL),
 (20, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 3, NULL),
 (20, 'undercarriage', 'Yürüyüş', 'enum', NULL, true, false, 4, '["track","wheel"]'::jsonb),
 (20, 'conveyor', 'Konveyör', 'bool', NULL, true, false, 5, NULL),
 (20, 'loading_side', 'Yükleme yönü', 'enum', NULL, true, false, 6, '["front","rear","side"]'::jsonb),
 -- Forklift (4)
 (4, 'fuel', 'Yakıt', 'enum', NULL, true, true, 1, '["diesel","lpg","electric"]'::jsonb),
 (4, 'mast_type', 'Mast tipi', 'enum', NULL, true, false, 2, '["simplex","duplex","triplex"]'::jsonb),
 (4, 'lift_height_m', 'Kaldırma yüksekliği', 'number', 'm', true, false, 3, NULL),
 (4, 'tire_type', 'Lastik tipi', 'enum', NULL, true, false, 4, '["pneumatic","superelastic","cushion"]'::jsonb),
 (4, 'free_lift', 'Serbest kaldırma', 'bool', NULL, true, false, 5, NULL),
 (4, 'capacity_kg', 'Kaldırma kapasitesi', 'number', 'kg', true, false, 6, NULL),
 (4, 'side_shift', 'Yana kaydırma', 'bool', NULL, true, false, 7, NULL),
 (4, 'cabin', 'Kabin', 'bool', NULL, true, false, 8, NULL),
 (4, 'load_center_mm', 'Yük merkezi', 'number', 'mm', false, false, 9, NULL),
 -- Vinç (5)
 (5, 'crane_type', 'Vinç tipi', 'enum', NULL, true, true, 1, '["mobile","crawler","tower"]'::jsonb),
 (5, 'boom_length_m', 'Bom uzunluğu', 'number', 'm', true, false, 2, NULL),
 (5, 'max_lift_height_m', 'Maksimum kaldırma yüksekliği', 'number', 'm', true, false, 3, NULL),
 (5, 'axle_count', 'Aks sayısı', 'number', NULL, true, false, 4, NULL),
 (5, 'jib', 'Uzatma bomu (jib)', 'bool', NULL, true, false, 5, NULL),
 (5, 'capacity_t', 'Kaldırma kapasitesi', 'number', 't', true, false, 6, NULL),
 (5, 'outriggers', 'Stabilizatör ayak', 'bool', NULL, true, false, 7, NULL),
 (5, 'all_terrain', 'Arazi tipi (all-terrain)', 'bool', NULL, true, false, 8, NULL),
 (5, 'counterweight_t', 'Karşı ağırlık', 'number', 't', false, false, 9, NULL),
 -- Telehandler (10)
 (10, 'lift_height_m', 'Kaldırma yüksekliği', 'number', 'm', true, false, 1, NULL),
 (10, 'fuel', 'Yakıt', 'enum', NULL, true, false, 2, '["diesel","lpg","electric"]'::jsonb),
 (10, 'rotating', 'Döner (roto)', 'bool', NULL, true, false, 3, NULL),
 (10, 'max_reach_m', 'Maksimum erişim', 'number', 'm', true, false, 4, NULL),
 (10, 'four_wd', '4x4', 'bool', NULL, true, false, 5, NULL),
 (10, 'lift_capacity_kg', 'Kaldırma kapasitesi', 'number', 'kg', true, false, 6, NULL),
 (10, 'outriggers', 'Stabilizatör ayak', 'bool', NULL, true, false, 7, NULL),
 (10, 'ac_cabin', 'Klimalı kabin', 'bool', NULL, true, false, 8, NULL),
 (10, 'frame_leveling', 'Şasi dengeleme', 'bool', NULL, true, false, 9, NULL),
 -- Sepetli Platform (16)
 (16, 'working_height_m', 'Çalışma yüksekliği', 'number', 'm', true, true, 1, NULL),
 (16, 'fuel', 'Yakıt', 'enum', NULL, true, false, 2, '["diesel","electric","hybrid"]'::jsonb),
 (16, 'platform_type', 'Platform tipi', 'enum', NULL, true, false, 3, '["scissor","articulating","telescopic","vertical_mast"]'::jsonb),
 (16, 'four_wd', '4x4', 'bool', NULL, true, false, 4, NULL),
 (16, 'horizontal_outreach_m', 'Yatay erişim', 'number', 'm', true, false, 5, NULL),
 (16, 'platform_capacity_kg', 'Platform kapasitesi', 'number', 'kg', true, false, 6, NULL),
 (16, 'terrain', 'Zemin', 'enum', NULL, true, false, 7, '["indoor_slab","rough_terrain"]'::jsonb),
 (16, 'stowed_width_m', 'Kapalı genişlik', 'number', 'm', true, false, 8, NULL),
 -- Transmikser (17)
 (17, 'drum_volume_m3', 'Tambur hacmi', 'number', 'm³', true, true, 1, NULL),
 (17, 'chassis_brand', 'Şasi markası', 'enum', NULL, true, false, 2, '["mercedes","man","ford","iveco","scania","daf","bmc"]'::jsonb),
 (17, 'axle_config', 'Aks konfigürasyonu', 'enum', NULL, true, false, 3, '["6x4","8x4","10x4"]'::jsonb),
 (17, 'system_type', 'Sistem', 'enum', NULL, true, false, 4, '["wet","dry"]'::jsonb),
 (17, 'water_tank_l', 'Su tankı', 'number', 'L', false, false, 5, NULL),
 (17, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 6, NULL),
 -- Beton Pompası (18)
 (18, 'pump_output_m3h', 'Pompa debisi', 'number', 'm³/h', true, false, 1, NULL),
 (18, 'vertical_reach_m', 'Dikey erişim', 'number', 'm', true, false, 2, NULL),
 (18, 'pump_type', 'Pompa tipi', 'enum', NULL, true, false, 3, '["truck_mounted","stationary_trailer","mixer_pump","placing_boom"]'::jsonb),
 (18, 'horizontal_reach_m', 'Yatay erişim', 'number', 'm', true, false, 4, NULL),
 (18, 'boom_sections', 'Bom bölümü', 'number', NULL, true, false, 5, NULL),
 (18, 'chassis_brand', 'Şasi markası', 'enum', NULL, true, false, 6, '["mercedes","man","ford","iveco","scania","daf"]'::jsonb),
 (18, 'axle_config', 'Aks konfigürasyonu', 'enum', NULL, true, false, 7, '["3_axle","4_axle","5_axle","6_axle"]'::jsonb),
 (18, 'outriggers', 'Stabilizatör ayak', 'bool', NULL, true, false, 8, NULL),
 -- Beton Santrali (11)
 (11, 'plant_type', 'Santral tipi', 'enum', NULL, true, true, 1, '["stationary","mobile","compact"]'::jsonb),
 (11, 'capacity_m3h', 'Kapasite', 'number', 'm³/h', true, false, 2, NULL),
 (11, 'mixer_type', 'Mikser tipi', 'enum', NULL, true, false, 3, '["twin_shaft","pan","planetary"]'::jsonb),
 (11, 'silo_capacity_t', 'Silo kapasitesi', 'number', 't', false, false, 4, NULL),
 (11, 'aggregate_bins', 'Agrega bölmesi', 'number', NULL, true, false, 5, NULL),
 (11, 'cement_silos', 'Çimento silosu adedi', 'number', NULL, true, false, 6, NULL),
 (11, 'automation', 'Otomasyon', 'enum', NULL, true, false, 7, '["manual","semi_auto","full_auto"]'::jsonb),
 -- Kırıcı (12)
 (12, 'crusher_type', 'Kırıcı tipi', 'enum', NULL, true, true, 1, '["jaw","cone","impact","vsi"]'::jsonb),
 (12, 'mobility', 'Yapı', 'enum', NULL, true, false, 2, '["mobile_tracked","mobile_wheeled","stationary"]'::jsonb),
 (12, 'capacity_tph', 'Kapasite', 'number', 't/h', true, false, 3, NULL),
 (12, 'stage', 'Kırma kademesi', 'enum', NULL, true, false, 4, '["primary","secondary","tertiary"]'::jsonb),
 (12, 'feed_opening_mm', 'Ağız açıklığı', 'number', 'mm', true, false, 5, NULL),
 (12, 'engine_power_hp', 'Motor gücü', 'number', 'HP', true, false, 6, NULL),
 (12, 'magnet', 'Konveyör mıknatısı', 'bool', NULL, true, false, 7, NULL),
 (12, 'screen_decks', 'Elek katmanı', 'number', NULL, true, false, 8, NULL)
ON CONFLICT (category_id, key) DO UPDATE SET
  label=EXCLUDED.label, data_type=EXCLUDED.data_type, unit=EXCLUDED.unit,
  is_filterable=EXCLUDED.is_filterable, is_required=EXCLUDED.is_required,
  sort_order=EXCLUDED.sort_order, enum_options=EXCLUDED.enum_options;

-- ---------------------------------------------------------------------------
-- Category ↔ attachment mapping
-- ---------------------------------------------------------------------------
-- Earthmoving excavators / loaders / dozers
INSERT INTO category_attachments (category_id, attachment_id)
SELECT c.id, a.id
FROM categories c
CROSS JOIN attachments a
WHERE c.id IN (1, 13, 2, 9) -- excavators + backhoe + mini excavator
  AND a.id IN (1, 2, 3, 5, 6, 8, 9, 10, 15, 16, 17, 18, 19, 20, 27, 30)
ON CONFLICT DO NOTHING;

INSERT INTO category_attachments (category_id, attachment_id)
SELECT c.id, a.id
FROM categories c
CROSS JOIN attachments a
WHERE c.id IN (3, 14) -- loaders / skid
  AND a.id IN (2, 4, 5, 7, 8, 14, 15, 21, 23, 28, 30)
ON CONFLICT DO NOTHING;

INSERT INTO category_attachments (category_id, attachment_id)
SELECT c.id, a.id
FROM categories c
CROSS JOIN attachments a
WHERE c.id IN (6, 7) -- dozer / grader
  AND a.id IN (6, 21, 30)
ON CONFLICT DO NOTHING;

INSERT INTO category_attachments (category_id, attachment_id)
SELECT c.id, a.id
FROM categories c
CROSS JOIN attachments a
WHERE c.id IN (8, 15, 20) -- roller / paver / milling
  AND a.id IN (7, 21, 23, 24)
ON CONFLICT DO NOTHING;

INSERT INTO category_attachments (category_id, attachment_id)
SELECT c.id, a.id
FROM categories c
CROSS JOIN attachments a
WHERE c.id IN (4, 10) -- forklift / telehandler
  AND a.id IN (4, 14, 25, 26)
ON CONFLICT DO NOTHING;

INSERT INTO category_attachments (category_id, attachment_id)
SELECT c.id, a.id
FROM categories c
CROSS JOIN attachments a
WHERE c.id IN (5, 16) -- crane / platform
  AND a.id IN (25, 26)
ON CONFLICT DO NOTHING;

INSERT INTO category_attachments (category_id, attachment_id)
SELECT c.id, a.id
FROM categories c
CROSS JOIN attachments a
WHERE c.id IN (17, 18, 11, 12) -- concrete / crusher
  AND a.id IN (13, 29, 1, 9)
ON CONFLICT DO NOTHING;

-- Fallback: categories without mapping get all attachments via app query UNION;
-- seed a few generic ones for damper
INSERT INTO category_attachments (category_id, attachment_id)
SELECT 19, a.id FROM attachments a WHERE a.id IN (2, 13)
ON CONFLICT DO NOTHING;


-- ---------------------------------------------------------------------------
-- OEM model specs (generated by scripts/generate_model_specs.py)
-- BEGIN OEM_MODEL_SPECS
-- ---------------------------------------------------------------------------
-- Model updates
-- ---------------------------------------------------------------------------
-- 320D
UPDATE equipment_models SET
    horsepower = 138,
    typical_weight_min_t = 20.3,
    typical_weight_max_t = 21.6
WHERE id = 1;

-- 336
UPDATE equipment_models SET
    horsepower = 273,
    typical_weight_min_t = 35.8,
    typical_weight_max_t = 38.6
WHERE id = 2;

-- 312D
UPDATE equipment_models SET
    horsepower = 94,
    typical_weight_min_t = 12.0,
    typical_weight_max_t = 14.0
WHERE id = 3;

-- PC210
UPDATE equipment_models SET
    horsepower = 158,
    typical_weight_min_t = 21.5,
    typical_weight_max_t = 23.5
WHERE id = 4;

-- PC360
UPDATE equipment_models SET
    horsepower = 257,
    typical_weight_min_t = 35.0,
    typical_weight_max_t = 38.0
WHERE id = 5;

-- ZX210
UPDATE equipment_models SET
    horsepower = 159,
    typical_weight_min_t = 21.0,
    typical_weight_max_t = 23.0
WHERE id = 6;

-- 220LC-9S
UPDATE equipment_models SET
    horsepower = 158,
    typical_weight_min_t = 21.7,
    typical_weight_max_t = 23.0
WHERE id = 7;

-- HMK 102B
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 9.0
WHERE id = 8;

-- 3CX
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 7.5,
    typical_weight_max_t = 8.5
WHERE id = 9;

-- 950M
UPDATE equipment_models SET
    horsepower = 230,
    typical_weight_min_t = 19.0,
    typical_weight_max_t = 20.5
WHERE id = 10;

-- L120H
UPDATE equipment_models SET
    horsepower = 255,
    typical_weight_min_t = 18.5,
    typical_weight_max_t = 21.0
WHERE id = 11;

-- 8FBE15
UPDATE equipment_models SET
    capacity_kg = 1500,
    capacity_t = 1.5,
    default_specs = '{"payload_t": 1.5}'::jsonb
WHERE id = 12;

-- S175
UPDATE equipment_models SET
    horsepower = 46,
    typical_weight_min_t = 2.5,
    typical_weight_max_t = 3.0
WHERE id = 13;

-- LTM 1100
UPDATE equipment_models SET
    capacity_t = 100.0,
    default_specs = '{"max_lift_capacity_t": 100.0}'::jsonb
WHERE id = 14;

-- D6T
UPDATE equipment_models SET
    horsepower = 215,
    typical_weight_min_t = 18.0,
    typical_weight_max_t = 22.0
WHERE id = 15;

-- 140M
UPDATE equipment_models SET
    horsepower = 183,
    typical_weight_min_t = 18.5,
    typical_weight_max_t = 20.3
WHERE id = 16;

-- SD115B
UPDATE equipment_models SET
    horsepower = 130,
    typical_weight_min_t = 11.0,
    typical_weight_max_t = 12.5
WHERE id = 17;

-- E35
UPDATE equipment_models SET
    horsepower = 25,
    typical_weight_min_t = 3.4,
    typical_weight_max_t = 3.8
WHERE id = 18;

-- U55-4
UPDATE equipment_models SET
    horsepower = 47,
    typical_weight_min_t = 5.3,
    typical_weight_max_t = 5.7
WHERE id = 19;

-- MT1840
UPDATE equipment_models SET
    horsepower = 75,
    capacity_kg = 4000,
    capacity_t = 4.0,
    default_specs = '{"payload_t": 4.0, "lift_height_m": 17.55}'::jsonb
WHERE id = 20;

-- 305.5E2
UPDATE equipment_models SET
    horsepower = 45,
    typical_weight_min_t = 5.4,
    typical_weight_max_t = 5.6
WHERE id = 21;

-- HMK 220 LC
UPDATE equipment_models SET
    horsepower = 160,
    typical_weight_min_t = 21.5,
    typical_weight_max_t = 24.0
WHERE id = 22;

-- HMK 140 W
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 14.0,
    typical_weight_max_t = 16.0
WHERE id = 23;

-- EW160E
UPDATE equipment_models SET
    horsepower = 150,
    typical_weight_min_t = 16.5,
    typical_weight_max_t = 18.0
WHERE id = 24;

-- S650
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 3.6,
    typical_weight_max_t = 4.0
WHERE id = 25;

-- 190
UPDATE equipment_models SET
    horsepower = 68,
    typical_weight_min_t = 2.5,
    typical_weight_max_t = 3.0
WHERE id = 26;

-- S-65
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 19.8}'::jsonb
WHERE id = 29;

-- M330 LC
UPDATE equipment_models SET
    horsepower = 228,
    typical_weight_min_t = 32.0,
    typical_weight_max_t = 35.0
WHERE id = 32;

-- TB260
UPDATE equipment_models SET
    horsepower = 47,
    typical_weight_min_t = 5.5,
    typical_weight_max_t = 6.3
WHERE id = 34;

-- SH210-6
UPDATE equipment_models SET
    horsepower = 158,
    typical_weight_min_t = 21.5,
    typical_weight_max_t = 23.0
WHERE id = 35;

-- 856H
UPDATE equipment_models SET
    horsepower = 220,
    typical_weight_min_t = 17.5,
    typical_weight_max_t = 19.5
WHERE id = 36;

-- CPCD30
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 37;

-- H2.5XT
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 38;

-- TF42.7
UPDATE equipment_models SET
    capacity_kg = 4200,
    capacity_t = 4.2,
    default_specs = '{"payload_t": 4.2, "lift_height_m": 7.0}'::jsonb
WHERE id = 39;

-- HA16 RTJ
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 16.0}'::jsonb
WHERE id = 41;

-- 320
UPDATE equipment_models SET
    horsepower = 162,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 25.0
WHERE id = 43;

-- 323
UPDATE equipment_models SET
    horsepower = 162,
    typical_weight_min_t = 24.0,
    typical_weight_max_t = 26.0
WHERE id = 44;

-- 330
UPDATE equipment_models SET
    horsepower = 273,
    typical_weight_min_t = 29.9,
    typical_weight_max_t = 32.5
WHERE id = 45;

-- 349
UPDATE equipment_models SET
    horsepower = 424,
    typical_weight_min_t = 46.0,
    typical_weight_max_t = 49.5
WHERE id = 46;

-- PC130-8
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 13.0,
    typical_weight_max_t = 14.4
WHERE id = 47;

-- PC300LC
UPDATE equipment_models SET
    horsepower = 246,
    typical_weight_min_t = 32.0,
    typical_weight_max_t = 34.5
WHERE id = 48;

-- ZX130
UPDATE equipment_models SET
    horsepower = 93,
    typical_weight_min_t = 13.0,
    typical_weight_max_t = 14.8
WHERE id = 49;

-- ZX350
UPDATE equipment_models SET
    horsepower = 271,
    typical_weight_min_t = 34.5,
    typical_weight_max_t = 36.5
WHERE id = 50;

-- HX300
UPDATE equipment_models SET
    horsepower = 225,
    typical_weight_min_t = 30.0,
    typical_weight_max_t = 32.0
WHERE id = 51;

-- EC220E
UPDATE equipment_models SET
    horsepower = 173,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 24.5
WHERE id = 52;

-- EC300E
UPDATE equipment_models SET
    horsepower = 226,
    typical_weight_min_t = 30.0,
    typical_weight_max_t = 32.5
WHERE id = 53;

-- HMK 300 LC
UPDATE equipment_models SET
    horsepower = 228,
    typical_weight_min_t = 30.0,
    typical_weight_max_t = 33.0
WHERE id = 54;

-- HMK 370 LC
UPDATE equipment_models SET
    horsepower = 271,
    typical_weight_min_t = 37.5,
    typical_weight_max_t = 40.0
WHERE id = 55;

-- JS220
UPDATE equipment_models SET
    horsepower = 162,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 24.0
WHERE id = 56;

-- SY215C
UPDATE equipment_models SET
    horsepower = 158,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 24.0
WHERE id = 57;

-- SK210LC-10
UPDATE equipment_models SET
    horsepower = 158,
    typical_weight_min_t = 21.5,
    typical_weight_max_t = 23.0
WHERE id = 58;

-- SK260LC-10
UPDATE equipment_models SET
    horsepower = 187,
    typical_weight_min_t = 27.0,
    typical_weight_max_t = 29.0
WHERE id = 59;

-- DX300LCA
UPDATE equipment_models SET
    horsepower = 229,
    typical_weight_min_t = 29.5,
    typical_weight_max_t = 31.5
WHERE id = 60;

-- M318
UPDATE equipment_models SET
    horsepower = 148,
    typical_weight_min_t = 18.3,
    typical_weight_max_t = 20.0
WHERE id = 61;

-- EW210E
UPDATE equipment_models SET
    horsepower = 173,
    typical_weight_min_t = 21.0,
    typical_weight_max_t = 22.5
WHERE id = 62;

-- DX160W-7
UPDATE equipment_models SET
    horsepower = 122,
    typical_weight_min_t = 16.0,
    typical_weight_max_t = 18.0
WHERE id = 63;

-- HMK 145 W
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 64;

-- 4CX
UPDATE equipment_models SET
    horsepower = 109,
    typical_weight_min_t = 8.5,
    typical_weight_max_t = 9.5
WHERE id = 65;

-- 432
UPDATE equipment_models SET
    horsepower = 101,
    typical_weight_min_t = 8.5,
    typical_weight_max_t = 9.5
WHERE id = 66;

-- B110C
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 7.5,
    typical_weight_max_t = 8.0
WHERE id = 67;

-- 580 Super N
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 7.5,
    typical_weight_max_t = 8.5
WHERE id = 68;

-- HMK 102 S
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 9.0
WHERE id = 69;

-- 938M
UPDATE equipment_models SET
    horsepower = 188,
    typical_weight_min_t = 16.5,
    typical_weight_max_t = 18.0
WHERE id = 71;

-- L90H
UPDATE equipment_models SET
    horsepower = 173,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 72;

-- L150H
UPDATE equipment_models SET
    horsepower = 300,
    typical_weight_min_t = 25.0,
    typical_weight_max_t = 27.5
WHERE id = 73;

-- WA380
UPDATE equipment_models SET
    horsepower = 213,
    typical_weight_min_t = 18.5,
    typical_weight_max_t = 20.3
WHERE id = 74;

-- SL50W
UPDATE equipment_models SET
    horsepower = 213,
    typical_weight_min_t = 17.0,
    typical_weight_max_t = 18.5
WHERE id = 77;

-- U35-4
UPDATE equipment_models SET
    horsepower = 24,
    typical_weight_min_t = 3.5,
    typical_weight_max_t = 3.9
WHERE id = 78;

-- KX080-4
UPDATE equipment_models SET
    horsepower = 66,
    typical_weight_min_t = 8.2,
    typical_weight_max_t = 8.9
WHERE id = 79;

-- E26
UPDATE equipment_models SET
    horsepower = 20,
    typical_weight_min_t = 2.6,
    typical_weight_max_t = 2.9
WHERE id = 80;

-- E88
UPDATE equipment_models SET
    horsepower = 66,
    typical_weight_min_t = 8.5,
    typical_weight_max_t = 9.0
WHERE id = 81;

-- TB290
UPDATE equipment_models SET
    horsepower = 66,
    typical_weight_min_t = 9.0,
    typical_weight_max_t = 10.0
WHERE id = 82;

-- 308
UPDATE equipment_models SET
    horsepower = 66,
    typical_weight_min_t = 8.2,
    typical_weight_max_t = 9.0
WHERE id = 83;

-- ViO50
UPDATE equipment_models SET
    horsepower = 38,
    typical_weight_min_t = 4.7,
    typical_weight_max_t = 5.3
WHERE id = 84;

-- SV100
UPDATE equipment_models SET
    horsepower = 72,
    typical_weight_min_t = 9.5,
    typical_weight_max_t = 10.5
WHERE id = 85;

-- ET65
UPDATE equipment_models SET
    horsepower = 56,
    typical_weight_min_t = 6.5,
    typical_weight_max_t = 7.5
WHERE id = 86;

-- S450
UPDATE equipment_models SET
    horsepower = 46,
    typical_weight_min_t = 2.3,
    typical_weight_max_t = 2.6
WHERE id = 87;

-- T590
UPDATE equipment_models SET
    horsepower = 68,
    typical_weight_min_t = 3.5,
    typical_weight_max_t = 3.9
WHERE id = 88;

-- 242D
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 3.5,
    typical_weight_max_t = 3.8
WHERE id = 89;

-- 155
UPDATE equipment_models SET
    horsepower = 68,
    typical_weight_min_t = 3.0,
    typical_weight_max_t = 3.5
WHERE id = 90;

-- D8
UPDATE equipment_models SET
    horsepower = 354,
    typical_weight_min_t = 38.0,
    typical_weight_max_t = 40.0
WHERE id = 92;

-- D65
UPDATE equipment_models SET
    horsepower = 205,
    typical_weight_min_t = 20.5,
    typical_weight_max_t = 22.0
WHERE id = 93;

-- SD22
UPDATE equipment_models SET
    horsepower = 240,
    typical_weight_min_t = 23.5,
    typical_weight_max_t = 24.5
WHERE id = 94;

-- SD16
UPDATE equipment_models SET
    horsepower = 175,
    typical_weight_min_t = 17.0,
    typical_weight_max_t = 18.5
WHERE id = 95;

-- 120
UPDATE equipment_models SET
    horsepower = 145,
    typical_weight_min_t = 13.5,
    typical_weight_max_t = 15.2
WHERE id = 96;

-- GD675
UPDATE equipment_models SET
    horsepower = 218,
    typical_weight_min_t = 16.5,
    typical_weight_max_t = 18.0
WHERE id = 97;

-- GR215
UPDATE equipment_models SET
    horsepower = 218,
    typical_weight_min_t = 16.0,
    typical_weight_max_t = 18.0
WHERE id = 98;

-- CS11 GC
UPDATE equipment_models SET
    horsepower = 130,
    typical_weight_min_t = 11.3,
    typical_weight_max_t = 12.0
WHERE id = 103;

-- CB13
UPDATE equipment_models SET
    horsepower = 100,
    typical_weight_min_t = 12.7,
    typical_weight_max_t = 13.5
WHERE id = 104;

-- DD105
UPDATE equipment_models SET
    horsepower = 100,
    typical_weight_min_t = 10.0,
    typical_weight_max_t = 11.0
WHERE id = 106;

-- AP555
UPDATE equipment_models SET
    horsepower = 174,
    typical_weight_min_t = 15.5,
    typical_weight_max_t = 16.5
WHERE id = 107;

-- FD25N
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 110;

-- 8FD25
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 111;

-- RX 60-25
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 113;

-- DFG 425
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 114;

-- C25
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 115;

-- EFL252
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 116;

-- GMK5150
UPDATE equipment_models SET
    capacity_t = 150.0,
    default_specs = '{"max_lift_capacity_t": 150.0}'::jsonb
WHERE id = 117;

-- GMK4100L
UPDATE equipment_models SET
    capacity_t = 100.0,
    default_specs = '{"max_lift_capacity_t": 100.0}'::jsonb
WHERE id = 118;

-- ATF 90G-4
UPDATE equipment_models SET
    capacity_t = 90.0,
    default_specs = '{"max_lift_capacity_t": 90.0}'::jsonb
WHERE id = 119;

-- LTM 1070
UPDATE equipment_models SET
    capacity_t = 70.0,
    default_specs = '{"max_lift_capacity_t": 70.0}'::jsonb
WHERE id = 120;

-- NK-250
UPDATE equipment_models SET
    capacity_t = 25.0,
    default_specs = '{"max_lift_capacity_t": 25.0}'::jsonb
WHERE id = 121;

-- 531-70
UPDATE equipment_models SET
    capacity_kg = 3100,
    capacity_t = 3.1,
    default_specs = '{"payload_t": 3.1, "lift_height_m": 7.0}'::jsonb
WHERE id = 123;

-- 540-170
UPDATE equipment_models SET
    capacity_kg = 4000,
    capacity_t = 4.0,
    default_specs = '{"payload_t": 4.0, "lift_height_m": 17.0}'::jsonb
WHERE id = 124;

-- MT 1440
UPDATE equipment_models SET
    horsepower = 75,
    capacity_kg = 4000,
    capacity_t = 4.0,
    default_specs = '{"payload_t": 4.0, "lift_height_m": 13.53}'::jsonb
WHERE id = 125;

-- MRT 2145
UPDATE equipment_models SET
    horsepower = 116,
    capacity_kg = 4500,
    capacity_t = 4.5,
    default_specs = '{"payload_t": 4.5, "lift_height_m": 20.6}'::jsonb
WHERE id = 126;

-- Icarus 40.17
UPDATE equipment_models SET
    capacity_kg = 4000,
    capacity_t = 4.0,
    default_specs = '{"payload_t": 4.0, "lift_height_m": 17.0}'::jsonb
WHERE id = 127;

-- RTH 6.21
UPDATE equipment_models SET
    capacity_kg = 6000,
    capacity_t = 6.0,
    default_specs = '{"payload_t": 6.0, "lift_height_m": 21.0}'::jsonb
WHERE id = 128;

-- GS-1932
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 5.8}'::jsonb
WHERE id = 129;

-- Z-45
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 13.7}'::jsonb
WHERE id = 130;

-- 600S
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 182.9}'::jsonb
WHERE id = 131;

-- 450AJ
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 137.2}'::jsonb
WHERE id = 132;

-- SJIII 3219
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 9.8}'::jsonb
WHERE id = 133;

-- Compact 12
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 12.0}'::jsonb
WHERE id = 134;

-- GTJZ1012
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 10.0}'::jsonb
WHERE id = 135;

-- 10 m³
UPDATE equipment_models SET
    default_specs = '{"drum_volume_m3": 10}'::jsonb
WHERE id = 138;

-- AM 9 C
UPDATE equipment_models SET
    default_specs = '{"drum_volume_m3": 9}'::jsonb
WHERE id = 139;

-- S 52 SX
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 52}'::jsonb
WHERE id = 140;

-- K47H
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 47}'::jsonb
WHERE id = 141;

-- H58-7RZ
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 58}'::jsonb
WHERE id = 143;

-- ECP56CS
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 56}'::jsonb
WHERE id = 144;

-- Elkomix-120
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 120}'::jsonb
WHERE id = 145;

-- Mobile Master-60
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 60}'::jsonb
WHERE id = 146;

-- M120
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 120}'::jsonb
WHERE id = 147;

-- 100S4
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 100}'::jsonb
WHERE id = 148;

-- Constmach-120
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 120}'::jsonb
WHERE id = 149;

-- DX225LC
UPDATE equipment_models SET
    horsepower = 165,
    typical_weight_min_t = 22.5,
    typical_weight_max_t = 24.0
WHERE id = 157;

-- DX300LC
UPDATE equipment_models SET
    horsepower = 229,
    typical_weight_min_t = 30.0,
    typical_weight_max_t = 32.0
WHERE id = 158;

-- DX140W
UPDATE equipment_models SET
    horsepower = 105,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 159;

-- DL250
UPDATE equipment_models SET
    horsepower = 160,
    typical_weight_min_t = 13.5,
    typical_weight_max_t = 15.0
WHERE id = 160;

-- DX85R-3
UPDATE equipment_models SET
    horsepower = 65,
    typical_weight_min_t = 8.3,
    typical_weight_max_t = 9.0
WHERE id = 161;

-- ZTC250V
UPDATE equipment_models SET
    capacity_t = 25.0,
    default_specs = '{"max_lift_capacity_t": 25.0}'::jsonb
WHERE id = 162;

-- 56X-6RZ
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 56}'::jsonb
WHERE id = 164;

-- HZS120
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 120}'::jsonb
WHERE id = 165;

-- CPCD30
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 167;

-- GDP25VX
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 168;

-- DX25
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 169;

-- FC 4525
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 170;

-- KBD25
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 171;

-- MLC300
UPDATE equipment_models SET
    capacity_t = 300.0,
    default_specs = '{"max_lift_capacity_t": 300.0}'::jsonb
WHERE id = 177;

-- 1932E
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 5.8}'::jsonb
WHERE id = 178;

-- S3219E
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 9.8}'::jsonb
WHERE id = 179;

-- ForSte 20D
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 20.0}'::jsonb
WHERE id = 180;

-- MMM 3000
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 300}'::jsonb
WHERE id = 183;

-- XE215C
UPDATE equipment_models SET
    horsepower = 162,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 24.0
WHERE id = 185;

-- CLG922E
UPDATE equipment_models SET
    horsepower = 162,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 24.0
WHERE id = 186;

-- HMK 640 WL
UPDATE equipment_models SET
    horsepower = 150,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 187;

-- 821G
UPDATE equipment_models SET
    horsepower = 172,
    typical_weight_min_t = 15.5,
    typical_weight_max_t = 17.0
WHERE id = 188;

-- SAC2200
UPDATE equipment_models SET
    capacity_t = 220.0,
    default_specs = '{"max_lift_capacity_t": 220.0}'::jsonb
WHERE id = 192;

-- QY25K5
UPDATE equipment_models SET
    capacity_t = 25.0,
    default_specs = '{"max_lift_capacity_t": 25.0}'::jsonb
WHERE id = 193;

-- PR 736
UPDATE equipment_models SET
    horsepower = 228,
    typical_weight_min_t = 20.0,
    typical_weight_max_t = 22.5
WHERE id = 195;

-- 2050M
UPDATE equipment_models SET
    horsepower = 179,
    typical_weight_min_t = 20.0,
    typical_weight_max_t = 22.5
WHERE id = 196;

-- CLG842
UPDATE equipment_models SET
    horsepower = 170,
    typical_weight_min_t = 18.0,
    typical_weight_max_t = 20.0
WHERE id = 197;

-- G960
UPDATE equipment_models SET
    horsepower = 204,
    typical_weight_min_t = 16.5,
    typical_weight_max_t = 18.0
WHERE id = 198;

-- HMK 140 MG
UPDATE equipment_models SET
    horsepower = 145,
    typical_weight_min_t = 14.0,
    typical_weight_max_t = 16.0
WHERE id = 199;

-- SG21-B
UPDATE equipment_models SET
    horsepower = 213,
    typical_weight_min_t = 15.5,
    typical_weight_max_t = 17.0
WHERE id = 201;

-- 86C-1
UPDATE equipment_models SET
    horsepower = 66,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 9.0
WHERE id = 206;

-- TL43.80HF
UPDATE equipment_models SET
    capacity_kg = 4300,
    capacity_t = 4.3,
    default_specs = '{"payload_t": 4.3, "lift_height_m": 8.0}'::jsonb
WHERE id = 208;

-- TH7.42
UPDATE equipment_models SET
    capacity_kg = 4200,
    capacity_t = 4.2,
    default_specs = '{"payload_t": 4.2, "lift_height_m": 7.0}'::jsonb
WHERE id = 209;

-- CBS-125
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 125}'::jsonb
WHERE id = 211;

-- HZS90V8
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 90}'::jsonb
WHERE id = 212;

-- HZS120G
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 120}'::jsonb
WHERE id = 213;

-- PW160
UPDATE equipment_models SET
    horsepower = 122,
    typical_weight_min_t = 16.0,
    typical_weight_max_t = 18.0
WHERE id = 219;

-- HW140
UPDATE equipment_models SET
    horsepower = 115,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 220;

-- S1930E
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 5.8}'::jsonb
WHERE id = 229;

-- 170 AETJ
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 17.0}'::jsonb
WHERE id = 230;

-- GTJZ1212
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 12.0}'::jsonb
WHERE id = 231;

-- SY309C
UPDATE equipment_models SET
    default_specs = '{"drum_volume_m3": 9}'::jsonb
WHERE id = 232;

-- G12K
UPDATE equipment_models SET
    default_specs = '{"drum_volume_m3": 12}'::jsonb
WHERE id = 233;

-- SLY 10
UPDATE equipment_models SET
    default_specs = '{"drum_volume_m3": 10}'::jsonb
WHERE id = 234;

-- HB67V
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 67}'::jsonb
WHERE id = 236;

-- Teletruk TLT30
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 237;

-- MI 25 D
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 238;

-- D5
UPDATE equipment_models SET
    horsepower = 130,
    typical_weight_min_t = 12.0,
    typical_weight_max_t = 14.0
WHERE id = 239;

-- D85
UPDATE equipment_models SET
    horsepower = 264,
    typical_weight_min_t = 24.5,
    typical_weight_max_t = 26.5
WHERE id = 240;

-- 160
UPDATE equipment_models SET
    horsepower = 254,
    typical_weight_min_t = 17.5,
    typical_weight_max_t = 19.5
WHERE id = 241;

-- HMK 600 MG
UPDATE equipment_models SET
    horsepower = 175,
    typical_weight_min_t = 18.0,
    typical_weight_max_t = 20.5
WHERE id = 242;

-- G930
UPDATE equipment_models SET
    horsepower = 173,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 243;

-- 770G
UPDATE equipment_models SET
    horsepower = 532
WHERE id = 244;

-- HD465
UPDATE equipment_models SET
    horsepower = 553
WHERE id = 245;

-- A40G
UPDATE equipment_models SET
    horsepower = 408
WHERE id = 246;

-- B40E
UPDATE equipment_models SET
    horsepower = 380
WHERE id = 256;

-- TA400
UPDATE equipment_models SET
    horsepower = 454
WHERE id = 257;

-- CDM6225
UPDATE equipment_models SET
    horsepower = 158,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 24.0
WHERE id = 263;

-- CDM856
UPDATE equipment_models SET
    horsepower = 220,
    typical_weight_min_t = 17.0,
    typical_weight_max_t = 19.0
WHERE id = 264;

-- 320GC
UPDATE equipment_models SET
    horsepower = 121,
    typical_weight_min_t = 20.4,
    typical_weight_max_t = 22.2
WHERE id = 265;

-- 330GC
UPDATE equipment_models SET
    horsepower = 204,
    typical_weight_min_t = 28.5,
    typical_weight_max_t = 31.2
WHERE id = 266;

-- 340
UPDATE equipment_models SET
    horsepower = 316,
    typical_weight_min_t = 38.0,
    typical_weight_max_t = 41.5
WHERE id = 267;

-- 326
UPDATE equipment_models SET
    horsepower = 186,
    typical_weight_min_t = 25.5,
    typical_weight_max_t = 28.0
WHERE id = 268;

-- 966M
UPDATE equipment_models SET
    horsepower = 275,
    typical_weight_min_t = 23.0,
    typical_weight_max_t = 26.0
WHERE id = 269;

-- 972M
UPDATE equipment_models SET
    horsepower = 310,
    typical_weight_min_t = 25.8,
    typical_weight_max_t = 28.5
WHERE id = 270;

-- D7
UPDATE equipment_models SET
    horsepower = 235,
    typical_weight_min_t = 25.0,
    typical_weight_max_t = 28.5
WHERE id = 271;

-- M320
UPDATE equipment_models SET
    horsepower = 162,
    typical_weight_min_t = 19.5,
    typical_weight_max_t = 21.0
WHERE id = 272;

-- 428
UPDATE equipment_models SET
    horsepower = 93,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 8.9
WHERE id = 273;

-- 730
UPDATE equipment_models SET
    horsepower = 370
WHERE id = 274;

-- 259D3
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 4.0,
    typical_weight_max_t = 4.5
WHERE id = 275;

-- PC220LC
UPDATE equipment_models SET
    horsepower = 165,
    typical_weight_min_t = 23.2,
    typical_weight_max_t = 25.0
WHERE id = 276;

-- PC200
UPDATE equipment_models SET
    horsepower = 155,
    typical_weight_min_t = 20.0,
    typical_weight_max_t = 22.0
WHERE id = 277;

-- WA320
UPDATE equipment_models SET
    horsepower = 148,
    typical_weight_min_t = 14.3,
    typical_weight_max_t = 16.0
WHERE id = 278;

-- WA470
UPDATE equipment_models SET
    horsepower = 290,
    typical_weight_min_t = 23.0,
    typical_weight_max_t = 26.0
WHERE id = 279;

-- D61EX
UPDATE equipment_models SET
    horsepower = 168,
    typical_weight_min_t = 18.0,
    typical_weight_max_t = 20.0
WHERE id = 280;

-- ZX250LC
UPDATE equipment_models SET
    horsepower = 185,
    typical_weight_min_t = 25.0,
    typical_weight_max_t = 27.0
WHERE id = 281;

-- ZX400LCH
UPDATE equipment_models SET
    horsepower = 350,
    typical_weight_min_t = 40.5,
    typical_weight_max_t = 43.0
WHERE id = 282;

-- ZX135US
UPDATE equipment_models SET
    horsepower = 93,
    typical_weight_min_t = 14.0,
    typical_weight_max_t = 16.0
WHERE id = 283;

-- HX220A
UPDATE equipment_models SET
    horsepower = 178,
    typical_weight_min_t = 22.5,
    typical_weight_max_t = 25.0
WHERE id = 284;

-- HX260A
UPDATE equipment_models SET
    horsepower = 192,
    typical_weight_min_t = 26.0,
    typical_weight_max_t = 28.5
WHERE id = 285;

-- HX210A
UPDATE equipment_models SET
    horsepower = 178,
    typical_weight_min_t = 21.5,
    typical_weight_max_t = 23.5
WHERE id = 286;

-- HL955
UPDATE equipment_models SET
    horsepower = 173,
    typical_weight_min_t = 16.5,
    typical_weight_max_t = 18.5
WHERE id = 287;

-- M36
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 36}'::jsonb
WHERE id = 288;

-- M42-5
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 42}'::jsonb
WHERE id = 289;

-- 535-95
UPDATE equipment_models SET
    capacity_kg = 3500,
    capacity_t = 3.5,
    default_specs = '{"payload_t": 3.5, "lift_height_m": 9.5}'::jsonb
WHERE id = 290;

-- 541-70
UPDATE equipment_models SET
    capacity_kg = 4100,
    capacity_t = 4.1,
    default_specs = '{"payload_t": 4.1, "lift_height_m": 7.0}'::jsonb
WHERE id = 291;

-- JS130
UPDATE equipment_models SET
    horsepower = 93,
    typical_weight_min_t = 13.5,
    typical_weight_max_t = 15.0
WHERE id = 292;

-- JS200
UPDATE equipment_models SET
    horsepower = 148,
    typical_weight_min_t = 20.0,
    typical_weight_max_t = 22.0
WHERE id = 293;

-- HMK 230 LC
UPDATE equipment_models SET
    horsepower = 168,
    typical_weight_min_t = 23.0,
    typical_weight_max_t = 25.0
WHERE id = 294;

-- HMK 310 LC
UPDATE equipment_models SET
    horsepower = 228,
    typical_weight_min_t = 31.0,
    typical_weight_max_t = 33.0
WHERE id = 295;

-- HMK 500 LCHD
UPDATE equipment_models SET
    horsepower = 380,
    typical_weight_min_t = 50.0,
    typical_weight_max_t = 54.0
WHERE id = 296;

-- HMK 102B Alpha
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 9.0
WHERE id = 297;

-- HTB 4014
UPDATE equipment_models SET
    capacity_kg = 4000,
    capacity_t = 4.0,
    default_specs = '{"payload_t": 4.0, "lift_height_m": 14.0}'::jsonb
WHERE id = 299;

-- EC250E
UPDATE equipment_models SET
    horsepower = 192,
    typical_weight_min_t = 25.5,
    typical_weight_max_t = 28.0
WHERE id = 300;

-- EC380E
UPDATE equipment_models SET
    horsepower = 309,
    typical_weight_min_t = 37.5,
    typical_weight_max_t = 41.0
WHERE id = 301;

-- L110H
UPDATE equipment_models SET
    horsepower = 220,
    typical_weight_min_t = 18.0,
    typical_weight_max_t = 20.5
WHERE id = 302;

-- L180H
UPDATE equipment_models SET
    horsepower = 340,
    typical_weight_min_t = 27.0,
    typical_weight_max_t = 29.5
WHERE id = 303;

-- A30G
UPDATE equipment_models SET
    horsepower = 360
WHERE id = 304;

-- E50
UPDATE equipment_models SET
    horsepower = 38,
    typical_weight_min_t = 5.0,
    typical_weight_max_t = 5.5
WHERE id = 305;

-- E55
UPDATE equipment_models SET
    horsepower = 38,
    typical_weight_min_t = 5.3,
    typical_weight_max_t = 5.8
WHERE id = 306;

-- S570
UPDATE equipment_models SET
    horsepower = 61,
    typical_weight_min_t = 2.8,
    typical_weight_max_t = 3.2
WHERE id = 307;

-- T76
UPDATE equipment_models SET
    horsepower = 92,
    typical_weight_min_t = 4.5,
    typical_weight_max_t = 5.0
WHERE id = 308;

-- GS-2646
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 7.9}'::jsonb
WHERE id = 309;

-- Z-60
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 18.3}'::jsonb
WHERE id = 310;

-- S-85
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 25.9}'::jsonb
WHERE id = 311;

-- R938
UPDATE equipment_models SET
    horsepower = 271,
    typical_weight_min_t = 36.0,
    typical_weight_max_t = 39.0
WHERE id = 312;

-- R956
UPDATE equipment_models SET
    horsepower = 375,
    typical_weight_min_t = 50.0,
    typical_weight_max_t = 55.0
WHERE id = 313;

-- LTM 1090
UPDATE equipment_models SET
    capacity_t = 90.0,
    default_specs = '{"max_lift_capacity_t": 90.0}'::jsonb
WHERE id = 314;

-- RT540E
UPDATE equipment_models SET
    capacity_t = 40.0,
    default_specs = '{"max_lift_capacity_t": 40.0}'::jsonb
WHERE id = 315;

-- RT770E
UPDATE equipment_models SET
    capacity_t = 70.0,
    default_specs = '{"max_lift_capacity_t": 70.0}'::jsonb
WHERE id = 316;

-- GR-500EX
UPDATE equipment_models SET
    capacity_t = 50.0,
    default_specs = '{"max_lift_capacity_t": 50.0}'::jsonb
WHERE id = 317;

-- GR-1000EX
UPDATE equipment_models SET
    capacity_t = 100.0,
    default_specs = '{"max_lift_capacity_t": 100.0}'::jsonb
WHERE id = 318;

-- U17
UPDATE equipment_models SET
    horsepower = 16,
    typical_weight_min_t = 1.7,
    typical_weight_max_t = 1.9
WHERE id = 319;

-- U48-5
UPDATE equipment_models SET
    horsepower = 42,
    typical_weight_min_t = 4.5,
    typical_weight_max_t = 5.0
WHERE id = 320;

-- 580ST
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 7.8,
    typical_weight_max_t = 8.5
WHERE id = 321;

-- CX210D
UPDATE equipment_models SET
    horsepower = 160,
    typical_weight_min_t = 21.5,
    typical_weight_max_t = 23.5
WHERE id = 322;

-- SK350LC
UPDATE equipment_models SET
    horsepower = 264,
    typical_weight_min_t = 35.5,
    typical_weight_max_t = 37.5
WHERE id = 323;

-- SK135SR
UPDATE equipment_models SET
    horsepower = 93,
    typical_weight_min_t = 14.0,
    typical_weight_max_t = 16.0
WHERE id = 324;

-- M220 LC
UPDATE equipment_models SET
    horsepower = 160,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 24.0
WHERE id = 325;

-- M300 LC
UPDATE equipment_models SET
    horsepower = 228,
    typical_weight_min_t = 30.0,
    typical_weight_max_t = 32.0
WHERE id = 326;

-- 8FG25
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 327;

-- 8FD30
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 328;

-- MT1335
UPDATE equipment_models SET
    horsepower = 75,
    capacity_kg = 3500,
    capacity_t = 3.5,
    default_specs = '{"payload_t": 3.5, "lift_height_m": 12.55}'::jsonb
WHERE id = 334;

-- MRT 2550
UPDATE equipment_models SET
    horsepower = 156,
    capacity_kg = 4999,
    capacity_t = 5.0,
    default_specs = '{"payload_t": 5.0, "lift_height_m": 24.7}'::jsonb
WHERE id = 335;

-- SY365C
UPDATE equipment_models SET
    horsepower = 268,
    typical_weight_min_t = 36.0,
    typical_weight_max_t = 38.5
WHERE id = 336;

-- XCT55
UPDATE equipment_models SET
    capacity_t = 55.0,
    default_specs = '{"max_lift_capacity_t": 55.0}'::jsonb
WHERE id = 337;

-- DX225LCA
UPDATE equipment_models SET
    horsepower = 165,
    typical_weight_min_t = 22.5,
    typical_weight_max_t = 24.5
WHERE id = 338;

-- DX340LCA
UPDATE equipment_models SET
    horsepower = 268,
    typical_weight_min_t = 34.0,
    typical_weight_max_t = 36.5
WHERE id = 339;

-- HX480A
UPDATE equipment_models SET
    horsepower = 378,
    typical_weight_min_t = 48.0,
    typical_weight_max_t = 52.0
WHERE id = 340;

-- 150
UPDATE equipment_models SET
    horsepower = 216,
    typical_weight_min_t = 16.5,
    typical_weight_max_t = 18.5
WHERE id = 341;

-- DD120
UPDATE equipment_models SET
    horsepower = 130,
    typical_weight_min_t = 12.0,
    typical_weight_max_t = 13.0
WHERE id = 342;

-- ZTC550V
UPDATE equipment_models SET
    capacity_t = 55.0,
    default_specs = '{"max_lift_capacity_t": 55.0}'::jsonb
WHERE id = 343;

-- 25D-7E
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 345;

-- 30D-9
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 346;

-- RX 20-20
UPDATE equipment_models SET
    capacity_kg = 2000,
    capacity_t = 2.0,
    default_specs = '{"payload_t": 2.0}'::jsonb
WHERE id = 347;

-- CPCD35
UPDATE equipment_models SET
    capacity_kg = 3500,
    capacity_t = 3.5,
    default_specs = '{"payload_t": 3.5}'::jsonb
WHERE id = 348;

-- CPD30
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 349;

-- H3.0XT
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 350;

-- GLP25VX
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 351;

-- EFG 425
UPDATE equipment_models SET
    capacity_kg = 2500,
    capacity_t = 2.5,
    default_specs = '{"payload_t": 2.5}'::jsonb
WHERE id = 352;

-- FD30N
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 354;

-- C30
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 355;

-- DX30
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 356;

-- SC 6020
UPDATE equipment_models SET
    capacity_kg = 2000,
    capacity_t = 2.0,
    default_specs = '{"payload_t": 2.0}'::jsonb
WHERE id = 357;

-- CPD30L1
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 358;

-- KBD30
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0}'::jsonb
WHERE id = 359;

-- TF50.8
UPDATE equipment_models SET
    capacity_kg = 5000,
    capacity_t = 5.0,
    default_specs = '{"payload_t": 5.0, "lift_height_m": 8.0}'::jsonb
WHERE id = 360;

-- Icarus 40.14
UPDATE equipment_models SET
    capacity_kg = 4000,
    capacity_t = 4.0,
    default_specs = '{"payload_t": 4.0, "lift_height_m": 14.0}'::jsonb
WHERE id = 361;

-- RTH 5.21
UPDATE equipment_models SET
    capacity_kg = 5000,
    capacity_t = 5.0,
    default_specs = '{"payload_t": 5.0, "lift_height_m": 21.0}'::jsonb
WHERE id = 362;

-- SJIII 3226
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 9.8}'::jsonb
WHERE id = 363;

-- AB14EJ
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 14.0}'::jsonb
WHERE id = 364;

-- GTJZ0808
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 8.0}'::jsonb
WHERE id = 365;

-- A46JRT
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 14.0}'::jsonb
WHERE id = 366;

-- ForSte 15A
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 15.0}'::jsonb
WHERE id = 367;

-- HA20 RTJ
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 20.0}'::jsonb
WHERE id = 368;

-- 860SJ
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 262.1}'::jsonb
WHERE id = 369;

-- NK-500E
UPDATE equipment_models SET
    capacity_t = 50.0,
    default_specs = '{"max_lift_capacity_t": 50.0}'::jsonb
WHERE id = 370;

-- MLC650
UPDATE equipment_models SET
    capacity_t = 650.0,
    default_specs = '{"max_lift_capacity_t": 650.0}'::jsonb
WHERE id = 372;

-- H43-5RZ
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 43}'::jsonb
WHERE id = 374;

-- ECP42CX
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 42}'::jsonb
WHERE id = 375;

-- M38-5
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 38}'::jsonb
WHERE id = 376;

-- S 36 X
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 36}'::jsonb
WHERE id = 377;

-- K41L
UPDATE equipment_models SET
    default_specs = '{"boom_length_m": 41}'::jsonb
WHERE id = 378;

-- 12 m³
UPDATE equipment_models SET
    default_specs = '{"drum_volume_m3": 12}'::jsonb
WHERE id = 380;

-- AM 10 C
UPDATE equipment_models SET
    default_specs = '{"drum_volume_m3": 10}'::jsonb
WHERE id = 381;

-- SLY 12
UPDATE equipment_models SET
    default_specs = '{"drum_volume_m3": 12}'::jsonb
WHERE id = 382;

-- B30E
UPDATE equipment_models SET
    horsepower = 326
WHERE id = 390;

-- A25G
UPDATE equipment_models SET
    horsepower = 326
WHERE id = 394;

-- 745
UPDATE equipment_models SET
    horsepower = 456
WHERE id = 395;

-- HM300
UPDATE equipment_models SET
    horsepower = 326
WHERE id = 396;

-- 60S3
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 60}'::jsonb
WHERE id = 397;

-- Beto batch 90
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 90}'::jsonb
WHERE id = 398;

-- Elkomix-60
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 60}'::jsonb
WHERE id = 399;

-- MB-60
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 60}'::jsonb
WHERE id = 400;

-- Constmach-60
UPDATE equipment_models SET
    default_specs = '{"plant_capacity_m3h": 60}'::jsonb
WHERE id = 401;

-- SL30W
UPDATE equipment_models SET
    horsepower = 130,
    typical_weight_min_t = 10.5,
    typical_weight_max_t = 12.0
WHERE id = 412;

-- CLG856H
UPDATE equipment_models SET
    horsepower = 220,
    typical_weight_min_t = 17.5,
    typical_weight_max_t = 19.5
WHERE id = 413;

-- M322
UPDATE equipment_models SET
    horsepower = 175,
    typical_weight_min_t = 21.0,
    typical_weight_max_t = 23.0
WHERE id = 418;

-- PW180
UPDATE equipment_models SET
    horsepower = 141,
    typical_weight_min_t = 17.0,
    typical_weight_max_t = 19.0
WHERE id = 419;

-- HW210
UPDATE equipment_models SET
    horsepower = 158,
    typical_weight_min_t = 20.0,
    typical_weight_max_t = 22.0
WHERE id = 420;

-- HMK 200 W
UPDATE equipment_models SET
    horsepower = 148,
    typical_weight_min_t = 19.0,
    typical_weight_max_t = 21.0
WHERE id = 421;

-- DX190W
UPDATE equipment_models SET
    horsepower = 140,
    typical_weight_min_t = 18.5,
    typical_weight_max_t = 20.0
WHERE id = 422;

-- EW180E
UPDATE equipment_models SET
    horsepower = 173,
    typical_weight_min_t = 17.5,
    typical_weight_max_t = 19.5
WHERE id = 423;

-- 140
UPDATE equipment_models SET
    horsepower = 183,
    typical_weight_min_t = 15.5,
    typical_weight_max_t = 17.5
WHERE id = 424;

-- GD555
UPDATE equipment_models SET
    horsepower = 162,
    typical_weight_min_t = 14.0,
    typical_weight_max_t = 16.0
WHERE id = 425;

-- G940
UPDATE equipment_models SET
    horsepower = 173,
    typical_weight_min_t = 15.0,
    typical_weight_max_t = 17.0
WHERE id = 426;

-- HMK 300 MG
UPDATE equipment_models SET
    horsepower = 145,
    typical_weight_min_t = 15.0,
    typical_weight_max_t = 17.0
WHERE id = 427;

-- GR180
UPDATE equipment_models SET
    horsepower = 180,
    typical_weight_min_t = 14.0,
    typical_weight_max_t = 16.0
WHERE id = 428;

-- SG16-3
UPDATE equipment_models SET
    horsepower = 175,
    typical_weight_min_t = 13.5,
    typical_weight_max_t = 15.0
WHERE id = 430;

-- D4
UPDATE equipment_models SET
    horsepower = 104,
    typical_weight_min_t = 10.0,
    typical_weight_max_t = 11.5
WHERE id = 431;

-- D9
UPDATE equipment_models SET
    horsepower = 436,
    typical_weight_min_t = 48.0,
    typical_weight_max_t = 52.5
WHERE id = 432;

-- D51
UPDATE equipment_models SET
    horsepower = 130,
    typical_weight_min_t = 12.0,
    typical_weight_max_t = 14.0
WHERE id = 433;

-- PR 766
UPDATE equipment_models SET
    horsepower = 395,
    typical_weight_min_t = 45.0,
    typical_weight_max_t = 50.0
WHERE id = 435;

-- 1650M
UPDATE equipment_models SET
    horsepower = 145,
    typical_weight_min_t = 17.0,
    typical_weight_max_t = 19.0
WHERE id = 436;

-- SD32
UPDATE equipment_models SET
    horsepower = 382,
    typical_weight_min_t = 35.5,
    typical_weight_max_t = 38.0
WHERE id = 437;

-- 303.5
UPDATE equipment_models SET
    horsepower = 24,
    typical_weight_min_t = 3.6,
    typical_weight_max_t = 3.8
WHERE id = 438;

-- 307.5
UPDATE equipment_models SET
    horsepower = 56,
    typical_weight_min_t = 7.5,
    typical_weight_max_t = 7.9
WHERE id = 439;

-- E10
UPDATE equipment_models SET
    horsepower = 10,
    typical_weight_min_t = 1.1,
    typical_weight_max_t = 1.3
WHERE id = 440;

-- E20
UPDATE equipment_models SET
    horsepower = 15,
    typical_weight_min_t = 2.0,
    typical_weight_max_t = 2.3
WHERE id = 441;

-- 67C-1
UPDATE equipment_models SET
    horsepower = 49,
    typical_weight_min_t = 6.0,
    typical_weight_max_t = 7.0
WHERE id = 442;

-- KX057-5
UPDATE equipment_models SET
    horsepower = 47,
    typical_weight_min_t = 5.3,
    typical_weight_max_t = 5.7
WHERE id = 443;

-- TB240
UPDATE equipment_models SET
    horsepower = 31,
    typical_weight_min_t = 4.3,
    typical_weight_max_t = 4.8
WHERE id = 444;

-- ViO80
UPDATE equipment_models SET
    horsepower = 59,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 8.8
WHERE id = 445;

-- ET90
UPDATE equipment_models SET
    horsepower = 72,
    typical_weight_min_t = 9.0,
    typical_weight_max_t = 10.0
WHERE id = 446;

-- DX60R
UPDATE equipment_models SET
    horsepower = 48,
    typical_weight_min_t = 6.0,
    typical_weight_max_t = 6.7
WHERE id = 447;

-- 262D3
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 3.7,
    typical_weight_max_t = 4.2
WHERE id = 448;

-- 289D3
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 4.5,
    typical_weight_max_t = 5.0
WHERE id = 449;

-- S770
UPDATE equipment_models SET
    horsepower = 92,
    typical_weight_min_t = 4.2,
    typical_weight_max_t = 4.7
WHERE id = 450;

-- T66
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 3.5,
    typical_weight_max_t = 3.9
WHERE id = 451;

-- 205
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 3.5,
    typical_weight_max_t = 4.0
WHERE id = 452;

-- CS12 GC
UPDATE equipment_models SET
    horsepower = 130,
    typical_weight_min_t = 12.0,
    typical_weight_max_t = 13.0
WHERE id = 457;

-- CB10
UPDATE equipment_models SET
    horsepower = 74,
    typical_weight_min_t = 10.0,
    typical_weight_max_t = 10.5
WHERE id = 458;

-- SD110B
UPDATE equipment_models SET
    horsepower = 130,
    typical_weight_min_t = 11.0,
    typical_weight_max_t = 12.5
WHERE id = 459;

-- AP655
UPDATE equipment_models SET
    horsepower = 217,
    typical_weight_min_t = 17.0,
    typical_weight_max_t = 19.0
WHERE id = 470;

-- 315
UPDATE equipment_models SET
    horsepower = 105,
    typical_weight_min_t = 15.5,
    typical_weight_max_t = 17.0
WHERE id = 481;

-- 352
UPDATE equipment_models SET
    horsepower = 469,
    typical_weight_min_t = 51.4,
    typical_weight_max_t = 54.0
WHERE id = 482;

-- PC138US
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 14.2,
    typical_weight_max_t = 15.5
WHERE id = 483;

-- PC490
UPDATE equipment_models SET
    horsepower = 359,
    typical_weight_min_t = 48.0,
    typical_weight_max_t = 52.0
WHERE id = 484;

-- ZX85USB
UPDATE equipment_models SET
    horsepower = 65,
    typical_weight_min_t = 8.3,
    typical_weight_max_t = 9.0
WHERE id = 485;

-- ZX490LCH
UPDATE equipment_models SET
    horsepower = 394,
    typical_weight_min_t = 48.0,
    typical_weight_max_t = 52.0
WHERE id = 486;

-- EC140E
UPDATE equipment_models SET
    horsepower = 105,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 487;

-- EC480E
UPDATE equipment_models SET
    horsepower = 375,
    typical_weight_min_t = 48.5,
    typical_weight_max_t = 52.5
WHERE id = 488;

-- JS160
UPDATE equipment_models SET
    horsepower = 114,
    typical_weight_min_t = 16.5,
    typical_weight_max_t = 18.5
WHERE id = 489;

-- JS370
UPDATE equipment_models SET
    horsepower = 271,
    typical_weight_min_t = 36.0,
    typical_weight_max_t = 39.0
WHERE id = 490;

-- HMK 140 LC
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 14.0,
    typical_weight_max_t = 16.0
WHERE id = 491;

-- DX140LC
UPDATE equipment_models SET
    horsepower = 105,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 492;

-- DX420LC
UPDATE equipment_models SET
    horsepower = 319,
    typical_weight_min_t = 42.0,
    typical_weight_max_t = 45.0
WHERE id = 493;

-- SY135C
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 13.5,
    typical_weight_max_t = 15.0
WHERE id = 494;

-- SY500H
UPDATE equipment_models SET
    horsepower = 371,
    typical_weight_min_t = 48.0,
    typical_weight_max_t = 52.0
WHERE id = 495;

-- XE370C
UPDATE equipment_models SET
    horsepower = 271,
    typical_weight_min_t = 36.0,
    typical_weight_max_t = 38.5
WHERE id = 496;

-- ZE215E
UPDATE equipment_models SET
    horsepower = 158,
    typical_weight_min_t = 21.5,
    typical_weight_max_t = 23.0
WHERE id = 497;

-- CLG936E
UPDATE equipment_models SET
    horsepower = 257,
    typical_weight_min_t = 35.0,
    typical_weight_max_t = 37.5
WHERE id = 498;

-- SK75
UPDATE equipment_models SET
    horsepower = 56,
    typical_weight_min_t = 7.5,
    typical_weight_max_t = 8.3
WHERE id = 499;

-- DX140LCA
UPDATE equipment_models SET
    horsepower = 105,
    typical_weight_min_t = 14.5,
    typical_weight_max_t = 16.0
WHERE id = 500;

-- DX210W
UPDATE equipment_models SET
    horsepower = 160,
    typical_weight_min_t = 20.0,
    typical_weight_max_t = 22.0
WHERE id = 501;

-- CDM6485
UPDATE equipment_models SET
    horsepower = 350,
    typical_weight_min_t = 48.0,
    typical_weight_max_t = 52.0
WHERE id = 502;

-- 926M
UPDATE equipment_models SET
    horsepower = 153,
    typical_weight_min_t = 13.8,
    typical_weight_max_t = 15.2
WHERE id = 503;

-- 980M
UPDATE equipment_models SET
    horsepower = 370,
    typical_weight_min_t = 30.5,
    typical_weight_max_t = 33.5
WHERE id = 504;

-- WA200
UPDATE equipment_models SET
    horsepower = 108,
    typical_weight_min_t = 11.0,
    typical_weight_max_t = 13.0
WHERE id = 505;

-- WA500
UPDATE equipment_models SET
    horsepower = 362,
    typical_weight_min_t = 30.5,
    typical_weight_max_t = 33.5
WHERE id = 506;

-- L60H
UPDATE equipment_models SET
    horsepower = 124,
    typical_weight_min_t = 11.0,
    typical_weight_max_t = 13.0
WHERE id = 507;

-- L220H
UPDATE equipment_models SET
    horsepower = 390,
    typical_weight_min_t = 32.5,
    typical_weight_max_t = 35.0
WHERE id = 508;

-- DL420
UPDATE equipment_models SET
    horsepower = 270,
    typical_weight_min_t = 22.0,
    typical_weight_max_t = 25.0
WHERE id = 510;

-- 921G
UPDATE equipment_models SET
    horsepower = 173,
    typical_weight_min_t = 13.5,
    typical_weight_max_t = 15.0
WHERE id = 511;

-- 1121G
UPDATE equipment_models SET
    horsepower = 248,
    typical_weight_min_t = 22.5,
    typical_weight_max_t = 25.0
WHERE id = 512;

-- CDM835
UPDATE equipment_models SET
    horsepower = 120,
    typical_weight_min_t = 11.0,
    typical_weight_max_t = 13.0
WHERE id = 514;

-- 3CX Super
UPDATE equipment_models SET
    horsepower = 92,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 9.0
WHERE id = 515;

-- 1CX
UPDATE equipment_models SET
    horsepower = 49,
    typical_weight_min_t = 2.5,
    typical_weight_max_t = 3.0
WHERE id = 516;

-- 444
UPDATE equipment_models SET
    horsepower = 110,
    typical_weight_min_t = 9.5,
    typical_weight_max_t = 10.5
WHERE id = 517;

-- HMK 102B Plus
UPDATE equipment_models SET
    horsepower = 97,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 9.0
WHERE id = 518;

-- 590ST
UPDATE equipment_models SET
    horsepower = 110,
    typical_weight_min_t = 9.0,
    typical_weight_max_t = 10.0
WHERE id = 519;

-- B115B
UPDATE equipment_models SET
    horsepower = 110,
    typical_weight_min_t = 8.0,
    typical_weight_max_t = 9.0
WHERE id = 520;

-- 540-140
UPDATE equipment_models SET
    capacity_kg = 4000,
    capacity_t = 4.0,
    default_specs = '{"payload_t": 4.0, "lift_height_m": 14.0}'::jsonb
WHERE id = 522;

-- 533-105
UPDATE equipment_models SET
    capacity_kg = 3300,
    capacity_t = 3.3,
    default_specs = '{"payload_t": 3.3, "lift_height_m": 10.5}'::jsonb
WHERE id = 523;

-- MT933
UPDATE equipment_models SET
    horsepower = 75,
    capacity_kg = 3300,
    capacity_t = 3.3,
    default_specs = '{"payload_t": 3.3, "lift_height_m": 9.07}'::jsonb
WHERE id = 524;

-- TL30.70
UPDATE equipment_models SET
    capacity_kg = 3000,
    capacity_t = 3.0,
    default_specs = '{"payload_t": 3.0, "lift_height_m": 7.0}'::jsonb
WHERE id = 525;

-- TH9.35
UPDATE equipment_models SET
    capacity_kg = 3500,
    capacity_t = 3.5,
    default_specs = '{"payload_t": 3.5, "lift_height_m": 9.0}'::jsonb
WHERE id = 526;

-- STC500
UPDATE equipment_models SET
    capacity_t = 50.0,
    default_specs = '{"max_lift_capacity_t": 50.0}'::jsonb
WHERE id = 529;

-- QY50KD
UPDATE equipment_models SET
    capacity_t = 50.0,
    default_specs = '{"max_lift_capacity_t": 50.0}'::jsonb
WHERE id = 530;

-- ZTC300V
UPDATE equipment_models SET
    capacity_t = 30.0,
    default_specs = '{"max_lift_capacity_t": 30.0}'::jsonb
WHERE id = 531;

-- GMK6300L
UPDATE equipment_models SET
    capacity_t = 300.0,
    default_specs = '{"max_lift_capacity_t": 300.0}'::jsonb
WHERE id = 532;

-- LTM 1300
UPDATE equipment_models SET
    capacity_t = 300.0,
    default_specs = '{"max_lift_capacity_t": 300.0}'::jsonb
WHERE id = 533;

-- GS-3246
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 9.8}'::jsonb
WHERE id = 534;

-- Z-30/20N
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 9.1}'::jsonb
WHERE id = 535;

-- Compact 8
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 8.0}'::jsonb
WHERE id = 536;

-- 1930ES
UPDATE equipment_models SET
    default_specs = '{"platform_height_m": 5.8}'::jsonb
WHERE id = 537;

-- TH5.26
UPDATE equipment_models SET
    capacity_kg = 2600,
    capacity_t = 2.6,
    default_specs = '{"payload_t": 2.6, "lift_height_m": 5.0}'::jsonb
WHERE id = 538;

-- SH350-6
UPDATE equipment_models SET
    horsepower = 259,
    typical_weight_min_t = 34.0,
    typical_weight_max_t = 36.0
WHERE id = 539;

-- TB216
UPDATE equipment_models SET
    horsepower = 14,
    typical_weight_min_t = 1.7,
    typical_weight_max_t = 1.9
WHERE id = 540;

-- END OEM_MODEL_SPECS

COMMIT;
