#!/usr/bin/env python3
"""
Patch OEM model metrics into database/05_equipment_catalog.sql
(between BEGIN/END OEM_MODEL_SPECS markers) from /tmp/models_export.csv.
"""

import csv
import json
import re
import sys
from pathlib import Path
from typing import Optional

CSV_PATH = "/tmp/models_export.csv"
CATALOG_SQL_PATH = "/Users/gaffarulutas/Desktop/aracparki.com/database/05_equipment_catalog.sql"
BEGIN_MARKER = "-- BEGIN OEM_MODEL_SPECS"
END_MARKER = "-- END OEM_MODEL_SPECS"

# ---------------------------------------------------------------------------
# Category IDs (from CSV)
# ---------------------------------------------------------------------------
CAT_EXCAVATOR = 1       # Paletli Ekskavatör
CAT_BACKHOE = 2         # Beko Loder
CAT_LOADER = 3          # Lastikli Yükleyici
CAT_FORKLIFT = 4        # Forklift
CAT_CRANE = 5           # Vinç
CAT_DOZER = 6           # Dozer
CAT_GRADER = 7          # Greyder
CAT_ROLLER = 8          # Silindir
CAT_MINI_EXC = 9        # Mini Ekskavatör
CAT_TELEHANDLER = 10    # Telehandler
CAT_CONCRETE_PLANT = 11 # Beton Santrali
CAT_CRUSHER = 12        # Kırıcı
CAT_WHEELED_EXC = 13    # Lastikli Ekskavatör
CAT_SKID_STEER = 14     # Mini Yükleyici
CAT_PAVER = 15          # Finişer
CAT_AERIAL = 16         # Sepetli Platform
CAT_MIXER = 17          # Transmikser
CAT_PUMP = 18           # Beton Pompası
CAT_DUMPER = 19         # Damper
CAT_MILLING = 20        # Asfalt Frezesi


# ---------------------------------------------------------------------------
# Explicit OEM HP database  (net HP where possible)
# key = (brand_id, model_name_upper) or just model_name_upper for some
# ---------------------------------------------------------------------------
OEM_HP: dict[tuple[int, str], int] = {}
OEM_WEIGHT: dict[tuple[int, str], tuple[Optional[float], Optional[float]]] = {}

def _add_oem(brand_id: int, name: str, hp: Optional[int] = None,
             wmin: Optional[float] = None, wmax: Optional[float] = None):
    key = (brand_id, name.upper())
    if hp is not None:
        OEM_HP[key] = hp
    if wmin is not None or wmax is not None:
        OEM_WEIGHT[key] = (wmin, wmax)

# --- Caterpillar (brand 1) ---
_add_oem(1, "320D", 138, 20.3, 21.6)
_add_oem(1, "312D", 94, 12.0, 14.0)
_add_oem(1, "336", 273, 35.8, 38.6)
_add_oem(1, "320", 162, 22.0, 25.0)
_add_oem(1, "320GC", 121, 20.4, 22.2)
_add_oem(1, "323", 162, 24.0, 26.0)
_add_oem(1, "330", 273, 29.9, 32.5)
_add_oem(1, "330GC", 204, 28.5, 31.2)
_add_oem(1, "340", 316, 38.0, 41.5)
_add_oem(1, "326", 186, 25.5, 28.0)
_add_oem(1, "349", 424, 46.0, 49.5)
_add_oem(1, "315", 105, 15.5, 17.0)
_add_oem(1, "352", 469, 51.4, 54.0)
_add_oem(1, "305.5E2", 45, 5.4, 5.6)
_add_oem(1, "308", 66, 8.2, 9.0)
_add_oem(1, "303.5", 24, 3.6, 3.8)
_add_oem(1, "307.5", 56, 7.5, 7.9)
_add_oem(1, "950M", 230, 19.0, 20.5)
_add_oem(1, "938M", 188, 16.5, 18.0)
_add_oem(1, "966M", 275, 23.0, 26.0)
_add_oem(1, "972M", 310, 25.8, 28.5)
_add_oem(1, "926M", 153, 13.8, 15.2)
_add_oem(1, "980M", 370, 30.5, 33.5)
_add_oem(1, "D6T", 215, 18.0, 22.0)
_add_oem(1, "D8", 354, 38.0, 40.0)
_add_oem(1, "D5", 130, 12.0, 14.0)
_add_oem(1, "D7", 235, 25.0, 28.5)
_add_oem(1, "D4", 104, 10.0, 11.5)
_add_oem(1, "D9", 436, 48.0, 52.5)
_add_oem(1, "140M", 183, 18.5, 20.3)
_add_oem(1, "120", 145, 13.5, 15.2)
_add_oem(1, "140", 183, 15.5, 17.5)
_add_oem(1, "150", 216, 16.5, 18.5)
_add_oem(1, "160", 254, 17.5, 19.5)
_add_oem(1, "432", 101, 8.5, 9.5)
_add_oem(1, "428", 93, 8.0, 8.9)
_add_oem(1, "444", 110, 9.5, 10.5)
_add_oem(1, "M318", 148, 18.3, 20.0)
_add_oem(1, "M320", 162, 19.5, 21.0)
_add_oem(1, "M322", 175, 21.0, 23.0)
_add_oem(1, "242D", 74, 3.5, 3.8)
_add_oem(1, "259D3", 74, 4.0, 4.5)
_add_oem(1, "262D3", 74, 3.7, 4.2)
_add_oem(1, "289D3", 74, 4.5, 5.0)
_add_oem(1, "CS11 GC", 130, 11.3, 12.0)
_add_oem(1, "CB13", 100, 12.7, 13.5)
_add_oem(1, "CS12 GC", 130, 12.0, 13.0)
_add_oem(1, "CB10", 74, 10.0, 10.5)

# --- Komatsu (brand 2) ---
_add_oem(2, "PC210", 158, 21.5, 23.5)
_add_oem(2, "PC360", 257, 35.0, 38.0)
_add_oem(2, "PC130-8", 97, 13.0, 14.4)
_add_oem(2, "PC300LC", 246, 32.0, 34.5)
_add_oem(2, "PC220LC", 165, 23.2, 25.0)
_add_oem(2, "PC200", 155, 20.0, 22.0)
_add_oem(2, "PC138US", 97, 14.2, 15.5)
_add_oem(2, "PC490", 359, 48.0, 52.0)
_add_oem(2, "WA380", 213, 18.5, 20.3)
_add_oem(2, "WA320", 148, 14.3, 16.0)
_add_oem(2, "WA470", 290, 23.0, 26.0)
_add_oem(2, "WA200", 108, 11.0, 13.0)
_add_oem(2, "WA500", 362, 30.5, 33.5)
_add_oem(2, "D65", 205, 20.5, 22.0)
_add_oem(2, "D85", 264, 24.5, 26.5)
_add_oem(2, "D51", 130, 12.0, 14.0)
_add_oem(2, "D61EX", 168, 18.0, 20.0)
_add_oem(2, "GD675", 218, 16.5, 18.0)
_add_oem(2, "GD555", 162, 14.0, 16.0)
_add_oem(2, "PW160", 122, 16.0, 18.0)
_add_oem(2, "PW180", 141, 17.0, 19.0)

# --- Hitachi (brand 3) ---
_add_oem(3, "ZX210", 159, 21.0, 23.0)
_add_oem(3, "ZX350", 271, 34.5, 36.5)
_add_oem(3, "ZX130", 93, 13.0, 14.8)
_add_oem(3, "ZX250LC", 185, 25.0, 27.0)
_add_oem(3, "ZX400LCH", 350, 40.5, 43.0)
_add_oem(3, "ZX135US", 93, 14.0, 16.0)
_add_oem(3, "ZX85USB", 65, 8.3, 9.0)
_add_oem(3, "ZX490LCH", 394, 48.0, 52.0)

# --- Hyundai (brand 4) ---
_add_oem(4, "220LC-9S", 158, 21.7, 23.0)
_add_oem(4, "HX300", 225, 30.0, 32.0)
_add_oem(4, "HX220A", 178, 22.5, 25.0)
_add_oem(4, "HX260A", 192, 26.0, 28.5)
_add_oem(4, "HX210A", 178, 21.5, 23.5)
_add_oem(4, "HX480A", 378, 48.0, 52.0)
_add_oem(4, "HW140", 115, 14.5, 16.0)
_add_oem(4, "HW210", 158, 20.0, 22.0)
_add_oem(4, "HL955", 173, 16.5, 18.5)

# --- Volvo (brand 5) ---
_add_oem(5, "EC220E", 173, 22.0, 24.5)
_add_oem(5, "EC300E", 226, 30.0, 32.5)
_add_oem(5, "EC250E", 192, 25.5, 28.0)
_add_oem(5, "EC380E", 309, 37.5, 41.0)
_add_oem(5, "EC140E", 105, 14.5, 16.0)
_add_oem(5, "EC480E", 375, 48.5, 52.5)
_add_oem(5, "EW160E", 150, 16.5, 18.0)
_add_oem(5, "EW210E", 173, 21.0, 22.5)
_add_oem(5, "EW180E", 173, 17.5, 19.5)
_add_oem(5, "L120H", 255, 18.5, 21.0)
_add_oem(5, "L90H", 173, 14.5, 16.0)
_add_oem(5, "L150H", 300, 25.0, 27.5)
_add_oem(5, "L110H", 220, 18.0, 20.5)
_add_oem(5, "L180H", 340, 27.0, 29.5)
_add_oem(5, "L60H", 124, 11.0, 13.0)
_add_oem(5, "L220H", 390, 32.5, 35.0)
_add_oem(5, "G960", 204, 16.5, 18.0)
_add_oem(5, "G930", 173, 14.5, 16.0)
_add_oem(5, "G940", 173, 15.0, 17.0)
_add_oem(5, "SD115B", 130, 11.0, 12.5)
_add_oem(5, "DD105", 100, 10.0, 11.0)
_add_oem(5, "SD110B", 130, 11.0, 12.5)
_add_oem(5, "DD120", 130, 12.0, 13.0)

# --- JCB (brand 6) ---
_add_oem(6, "3CX", 74, 7.5, 8.5)
_add_oem(6, "3CX Super", 92, 8.0, 9.0)
_add_oem(6, "4CX", 109, 8.5, 9.5)
_add_oem(6, "1CX", 49, 2.5, 3.0)
_add_oem(6, "JS220", 162, 22.0, 24.0)
_add_oem(6, "JS130", 93, 13.5, 15.0)
_add_oem(6, "JS200", 148, 20.0, 22.0)
_add_oem(6, "JS160", 114, 16.5, 18.5)
_add_oem(6, "JS370", 271, 36.0, 39.0)
_add_oem(6, "86C-1", 66, 8.0, 9.0)
_add_oem(6, "67C-1", 49, 6.0, 7.0)
_add_oem(6, "190", 68, 2.5, 3.0)
_add_oem(6, "155", 68, 3.0, 3.5)
_add_oem(6, "205", 74, 3.5, 4.0)

# --- Hidromek (brand 7) ---
_add_oem(7, "HMK 220 LC", 160, 21.5, 24.0)
_add_oem(7, "HMK 300 LC", 228, 30.0, 33.0)
_add_oem(7, "HMK 370 LC", 271, 37.5, 40.0)
_add_oem(7, "HMK 230 LC", 168, 23.0, 25.0)
_add_oem(7, "HMK 310 LC", 228, 31.0, 33.0)
_add_oem(7, "HMK 500 LCHD", 380, 50.0, 54.0)
_add_oem(7, "HMK 140 LC", 97, 14.0, 16.0)
_add_oem(7, "HMK 140 W", 97, 14.0, 16.0)
_add_oem(7, "HMK 145 W", 97, 14.5, 16.0)
_add_oem(7, "HMK 200 W", 148, 19.0, 21.0)
_add_oem(7, "HMK 102B", 97, 8.0, 9.0)
_add_oem(7, "HMK 102 S", 97, 8.0, 9.0)
_add_oem(7, "HMK 102B Alpha", 97, 8.0, 9.0)
_add_oem(7, "HMK 102B Plus", 97, 8.0, 9.0)
_add_oem(7, "HMK 640 WL", 150, 14.5, 16.0)
_add_oem(7, "HMK 140 MG", 145, 14.0, 16.0)
_add_oem(7, "HMK 300 MG", 145, 15.0, 17.0)
_add_oem(7, "HMK 600 MG", 175, 18.0, 20.5)

# --- Bobcat (brand 8) ---
_add_oem(8, "E35", 25, 3.4, 3.8)
_add_oem(8, "E26", 20, 2.6, 2.9)
_add_oem(8, "E88", 66, 8.5, 9.0)
_add_oem(8, "E50", 38, 5.0, 5.5)
_add_oem(8, "E55", 38, 5.3, 5.8)
_add_oem(8, "E10", 10, 1.1, 1.3)
_add_oem(8, "E20", 15, 2.0, 2.3)
_add_oem(8, "S175", 46, 2.5, 3.0)
_add_oem(8, "S650", 74, 3.6, 4.0)
_add_oem(8, "S450", 46, 2.3, 2.6)
_add_oem(8, "T590", 68, 3.5, 3.9)
_add_oem(8, "S570", 61, 2.8, 3.2)
_add_oem(8, "S770", 92, 4.2, 4.7)
_add_oem(8, "T76", 92, 4.5, 5.0)
_add_oem(8, "T66", 74, 3.5, 3.9)

# --- Liebherr (brand 9) ---
_add_oem(9, "R938", 271, 36.0, 39.0)
_add_oem(9, "R956", 375, 50.0, 55.0)
_add_oem(9, "PR 736", 228, 20.0, 22.5)
_add_oem(9, "PR 766", 395, 45.0, 50.0)

# --- Doosan/Develon (brand 10) ---
_add_oem(10, "DX225LC", 165, 22.5, 24.0)
_add_oem(10, "DX300LC", 229, 30.0, 32.0)
_add_oem(10, "DX140LC", 105, 14.5, 16.0)
_add_oem(10, "DX420LC", 319, 42.0, 45.0)
_add_oem(10, "DX140W", 105, 14.5, 16.0)
_add_oem(10, "DX190W", 140, 18.5, 20.0)
_add_oem(10, "DX85R-3", 65, 8.3, 9.0)
_add_oem(10, "DX60R", 48, 6.0, 6.7)
_add_oem(10, "DL250", 160, 13.5, 15.0)
_add_oem(10, "DL420", 270, 22.0, 25.0)

# --- Kubota (brand 11) ---
_add_oem(11, "U55-4", 47, 5.3, 5.7)
_add_oem(11, "U35-4", 24, 3.5, 3.9)
_add_oem(11, "KX080-4", 66, 8.2, 8.9)
_add_oem(11, "U17", 16, 1.7, 1.9)
_add_oem(11, "U48-5", 42, 4.5, 5.0)
_add_oem(11, "KX057-5", 47, 5.3, 5.7)

# --- Case (brand 12) ---
_add_oem(12, "580 Super N", 97, 7.5, 8.5)
_add_oem(12, "580ST", 97, 7.8, 8.5)
_add_oem(12, "590ST", 110, 9.0, 10.0)
_add_oem(12, "CX210D", 160, 21.5, 23.5)
_add_oem(12, "821G", 172, 15.5, 17.0)
_add_oem(12, "921G", 173, 13.5, 15.0)
_add_oem(12, "1121G", 248, 22.5, 25.0)
_add_oem(12, "2050M", 179, 20.0, 22.5)
_add_oem(12, "1650M", 145, 17.0, 19.0)

# --- New Holland (brand 13) ---
_add_oem(13, "B110C", 97, 7.5, 8.0)
_add_oem(13, "B115B", 110, 8.0, 9.0)

# --- Manitou (brand 14) ---
# (telehandler capacity handled by parser below)

# --- Toyota (brand 15) ---
# (forklift capacity handled by parser below)

# --- Sany (brand 17) ---
_add_oem(17, "SY215C", 158, 22.0, 24.0)
_add_oem(17, "SY365C", 268, 36.0, 38.5)
_add_oem(17, "SY135C", 97, 13.5, 15.0)
_add_oem(17, "SY500H", 371, 48.0, 52.0)

# --- XCMG (brand 18) ---
_add_oem(18, "XE215C", 162, 22.0, 24.0)
_add_oem(18, "XE370C", 271, 36.0, 38.5)
_add_oem(18, "GR215", 218, 16.0, 18.0)
_add_oem(18, "GR180", 180, 14.0, 16.0)

# --- MST (brand 19) ---
_add_oem(19, "M330 LC", 228, 32.0, 35.0)
_add_oem(19, "M220 LC", 160, 22.0, 24.0)
_add_oem(19, "M300 LC", 228, 30.0, 32.0)

# --- Zoomlion (brand 24) ---
_add_oem(24, "ZE215E", 158, 21.5, 23.0)

# --- Takeuchi (brand 26) ---
_add_oem(26, "TB260", 47, 5.5, 6.3)
_add_oem(26, "TB290", 66, 9.0, 10.0)
_add_oem(26, "TB240", 31, 4.3, 4.8)
_add_oem(26, "TB216", 14, 1.7, 1.9)

# --- Sumitomo (brand 27) ---
_add_oem(27, "SH210-6", 158, 21.5, 23.0)
_add_oem(27, "SH350-6", 259, 34.0, 36.0)

# --- LiuGong (brand 28) ---
_add_oem(28, "CLG922E", 162, 22.0, 24.0)
_add_oem(28, "CLG936E", 257, 35.0, 37.5)
_add_oem(28, "856H", 220, 17.5, 19.5)
_add_oem(28, "CLG856H", 220, 17.5, 19.5)
_add_oem(28, "CLG842", 170, 18.0, 20.0)

# --- Kobelco (brand 42) ---
_add_oem(42, "SK210LC-10", 158, 21.5, 23.0)
_add_oem(42, "SK260LC-10", 187, 27.0, 29.0)
_add_oem(42, "SK350LC", 264, 35.5, 37.5)
_add_oem(42, "SK135SR", 93, 14.0, 16.0)
_add_oem(42, "SK75", 56, 7.5, 8.3)

# --- Develon (brand 43) ---
_add_oem(43, "DX300LCA", 229, 29.5, 31.5)
_add_oem(43, "DX225LCA", 165, 22.5, 24.5)
_add_oem(43, "DX340LCA", 268, 34.0, 36.5)
_add_oem(43, "DX140LCA", 105, 14.5, 16.0)
_add_oem(43, "DX160W-7", 122, 16.0, 18.0)
_add_oem(43, "DX210W", 160, 20.0, 22.0)

# --- Shantui (brand 45) ---
_add_oem(45, "SD22", 240, 23.5, 24.5)
_add_oem(45, "SD16", 175, 17.0, 18.5)
_add_oem(45, "SD32", 382, 35.5, 38.0)
_add_oem(45, "SL50W", 213, 17.0, 18.5)
_add_oem(45, "SL30W", 130, 10.5, 12.0)
_add_oem(45, "SG21-B", 213, 15.5, 17.0)
_add_oem(45, "SG16-3", 175, 13.5, 15.0)

# --- Yanmar (brand 47) ---
_add_oem(47, "ViO50", 38, 4.7, 5.3)
_add_oem(47, "SV100", 72, 9.5, 10.5)
_add_oem(47, "ViO80", 59, 8.0, 8.8)

# --- Wacker Neuson (brand 48) ---
_add_oem(48, "ET65", 56, 6.5, 7.5)
_add_oem(48, "ET90", 72, 9.0, 10.0)

# --- Lonking (brand 90) ---
_add_oem(90, "CDM6225", 158, 22.0, 24.0)
_add_oem(90, "CDM6485", 350, 48.0, 52.0)
_add_oem(90, "CDM856", 220, 17.0, 19.0)
_add_oem(90, "CDM835", 120, 11.0, 13.0)

# --- Dumper OEM HP ---
_add_oem(1, "770G", 532, None, None)
_add_oem(1, "745", 456, None, None)
_add_oem(1, "730", 370, None, None)
_add_oem(2, "HD465", 553, None, None)
_add_oem(2, "HM300", 326, None, None)
_add_oem(5, "A40G", 408, None, None)
_add_oem(5, "A30G", 360, None, None)
_add_oem(5, "A25G", 326, None, None)
_add_oem(95, "B40E", 380, None, None)
_add_oem(95, "B30E", 326, None, None)
_add_oem(49, "TA400", 454, None, None)

# --- Paver OEM HP ---
_add_oem(1, "AP555", 174, 15.5, 16.5)
_add_oem(1, "AP655", 217, 17.0, 19.0)


# ---------------------------------------------------------------------------
# Forklift capacity parser (category 4)
# ---------------------------------------------------------------------------
def parse_forklift_capacity_kg(name: str) -> Optional[int]:
    n = name.upper().strip()
    # H2.5XT, H3.0XT style
    m = re.search(r'(\d+)[.,](\d+)\s*(?:XT|FT|CT|ET|D|T)?', n)
    if m and int(m.group(1)) < 30:
        tonnes = float(f"{m.group(1)}.{m.group(2)}")
        if 0.5 <= tonnes <= 25:
            return int(tonnes * 1000)

    # MI 25 D → 25 = 2500
    m = re.search(r'MI\s*(\d{2})\s*D', n)
    if m:
        return int(m.group(1)) * 100

    # CPCD30, CPD30 etc
    m = re.search(r'(?:CPCD|CPD|CPQD|CPC|CPG)\s*(\d{2,3})', n)
    if m:
        return int(m.group(1)) * 100

    # 8FBE15, 8FD25, 8FG25 → last 2 digits * 100
    m = re.search(r'\d*F[A-Z]*(\d{2})\b', n)
    if m:
        return int(m.group(1)) * 100

    # FD25N, FD30N
    m = re.search(r'FD\s*(\d{2,3})\s*[A-Z]?', n)
    if m:
        return int(m.group(1)) * 100

    # GDP25VX, GLP25VX
    m = re.search(r'G[A-Z]*P(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    # DFG 425, EFG 425 → last 2 digits: 25 → 2500
    m = re.search(r'[DE]FG\s*\d?(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    # C25, C30 (Clark)
    m = re.search(r'^C(\d{2,3})$', n)
    if m:
        return int(m.group(1)) * 100

    # EFL252 → 25 (take first 2 of 252)
    m = re.search(r'EFL(\d{2})\d?', n)
    if m:
        return int(m.group(1)) * 100

    # DX25, DX30 (UniCarriers)
    m = re.search(r'^DX\s*(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    # KBD25, KBD30
    m = re.search(r'KBD\s*(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    # RX 60-25, RX 20-20
    m = re.search(r'RX\s*\d+-(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    # FC 4525 → 25 (last 2 digits)
    m = re.search(r'FC\s*\d{2}(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    # SC 6020 → 20 (last 2 digits)
    m = re.search(r'SC\s*\d{2}(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    # CPD30L1
    m = re.search(r'CPD(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    # Hyundai 25D-7E, 30D-9
    m = re.search(r'^(\d{2})D-', n)
    if m:
        return int(m.group(1)) * 100

    # Teletruk TLT30 → 3000
    m = re.search(r'TLT\s*(\d{2})', n)
    if m:
        return int(m.group(1)) * 100

    return None


# ---------------------------------------------------------------------------
# Telehandler OEM overrides (datasheet values — prefer over naming heuristics)
# key = normalized model name (uppercase, spaces collapsed)
# value = (capacity_kg, lift_height_m, optional_hp)
# ---------------------------------------------------------------------------
OEM_TELEHANDLER: dict[str, tuple[int, float, Optional[int]]] = {
    # Manitou — manitou.com / official technical sheets
    "MT1840": (4000, 17.55, 75),
    "MT1440": (4000, 13.53, 75),
    "MT 1440": (4000, 13.53, 75),
    "MT1335": (3500, 12.55, 75),
    "MT933": (3300, 9.07, 75),
    "MRT2145": (4500, 20.60, 116),
    "MRT 2145": (4500, 20.60, 116),
    "MRT2550": (4999, 24.70, 156),
    "MRT 2550": (4999, 24.70, 156),
}


def _norm_model(name: str) -> str:
    return re.sub(r"\s+", " ", name.upper().strip())


# ---------------------------------------------------------------------------
# Telehandler parser (category 10)
# ---------------------------------------------------------------------------
def parse_telehandler(name: str, brand: str) -> dict:
    result: dict = {}
    n = _norm_model(name)

    oem = OEM_TELEHANDLER.get(n)
    if oem is None:
        # also try without spaces for MT1840 vs MT 1840
        oem = OEM_TELEHANDLER.get(n.replace(" ", ""))
    if oem is not None:
        result["capacity_kg"] = oem[0]
        result["lift_height_m"] = oem[1]
        if oem[2] is not None:
            result["hp"] = oem[2]
        return result

    # JCB aaa-bb: first digits = capacity*10 kg (531→3100), bb = height m
    m = re.match(r'^(\d{3})-(\d{2,3})$', n)
    if m and brand.upper() == "JCB":
        raw_cap = int(m.group(1))
        raw_h = int(m.group(2))
        cap_map = {531: 3100, 533: 3300, 535: 3500, 540: 4000, 541: 4100}
        result["capacity_kg"] = cap_map.get(raw_cap, raw_cap * 10)
        if raw_h < 100:
            result["lift_height_m"] = round(raw_h / 10, 1) if raw_h >= 10 else float(raw_h)
        else:
            result["lift_height_m"] = round(raw_h / 10, 1)
        return result

    # Manitou MT naming (fallback when OEM override missing):
    # capacity = last 2 digits / 10 t; height from name is approximate — prefer OEM.
    m = re.search(r'MT\s*(\d{2,4})', n)
    if m:
        digits = m.group(1)
        if len(digits) == 4:
            tenths = int(digits[2:])  # 40 → 4.0 t
            result["capacity_kg"] = tenths * 100
            # Do not invent lift_height_m from digits (OEM sheets differ, e.g. 1840→17.55 not 18)
        elif len(digits) == 3:
            tenths = int(digits[1:])
            result["capacity_kg"] = tenths * 100
        return result

    # Manitou MRT naming fallback: capacity only from last 2 digits
    m = re.search(r'MRT\s*(\d{4})', n)
    if m:
        digits = m.group(1)
        tenths = int(digits[2:])
        result["capacity_kg"] = tenths * 100
        return result

    # Merlo TF42.7, TF50.8 → capacity=42*100 kg, height=7m
    m = re.search(r'TF\s*(\d{2})[.,](\d)', n)
    if m:
        result["capacity_kg"] = int(m.group(1)) * 100
        result["lift_height_m"] = float(m.group(2))
        return result

    # Dieci Icarus 40.17
    m = re.search(r'(?:ICARUS|PEGASUS|RUNNER)\s*(\d{2,3})[.,](\d{2})', n)
    if m:
        result["capacity_kg"] = int(m.group(1)) * 100
        result["lift_height_m"] = float(m.group(2))
        return result

    # Magni RTH 6.21, RTH 5.21 → 6t / 21m
    m = re.search(r'RTH\s*(\d+)[.,](\d{2})', n)
    if m:
        result["capacity_kg"] = int(m.group(1)) * 1000
        result["lift_height_m"] = float(m.group(2))
        return result

    # Bobcat TL43.80HF, TL30.70
    m = re.search(r'TL\s*(\d{2})[.,](\d{2})', n)
    if m:
        result["capacity_kg"] = int(m.group(1)) * 100
        result["lift_height_m"] = round(int(m.group(2)) / 10, 1)
        return result

    # New Holland TH7.42 → height 7m, capacity 4.2t
    m = re.search(r'TH\s*(\d+)[.,](\d{2})', n)
    if m:
        result["lift_height_m"] = float(m.group(1))
        result["capacity_kg"] = int(m.group(2)) * 100
        return result

    # Hidromek HTB 4014 → 4.0t / 14m
    m = re.search(r'HTB\s*(\d{2})(\d{2})', n)
    if m:
        result["capacity_kg"] = int(m.group(1)) * 100
        result["lift_height_m"] = float(m.group(2))
        return result

    # JLG 1055, 1255 (US naming: capacity*100 lb / height ft) — skip if ambiguous
    return result


# ---------------------------------------------------------------------------
# Aerial platform parser (category 16)
# ---------------------------------------------------------------------------
def parse_aerial(name: str, brand: str) -> dict:
    result: dict = {}
    n = name.upper().strip()

    # Genie S-65, S-85
    m = re.match(r'S-?(\d{2,3})', n)
    if m and brand.upper() == "GENIE":
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # Genie Z-45, Z-60, Z-30/20N
    m = re.match(r'Z-?(\d{2,3})', n)
    if m and brand.upper() == "GENIE":
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # Genie GS-1932, GS-2646, GS-3246
    m = re.match(r'GS-?(\d{2})(\d{2})', n)
    if m and brand.upper() == "GENIE":
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # JLG 600S, 860SJ
    m = re.match(r'(\d{2,3})0?\s*S[J]?$', n)
    if m and brand.upper() == "JLG":
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # JLG 450AJ
    m = re.match(r'(\d{2,3})0?\s*AJ', n)
    if m and brand.upper() == "JLG":
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # JLG 1930ES
    m = re.match(r'(\d{2})(\d{2})ES', n)
    if m and brand.upper() == "JLG":
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # Haulotte HA16 RTJ, HA20 RTJ
    m = re.search(r'HA\s*(\d{2})\s*RTJ', n)
    if m:
        result["platform_height_m"] = float(m.group(1))
        return result

    # Haulotte Compact 12, Compact 8
    m = re.search(r'COMPACT\s*(\d{1,2})', n)
    if m:
        result["platform_height_m"] = float(m.group(1))
        return result

    # Skyjack SJIII 3219, SJIII 3226
    m = re.search(r'SJIII\s*(\d{2})(\d{2})', n)
    if m:
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # Sinoboom 1932E
    m = re.match(r'(\d{2})(\d{2})E$', n)
    if m:
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # Sinoboom AB14EJ → 14m
    m = re.search(r'AB(\d{2})EJ', n)
    if m:
        result["platform_height_m"] = float(m.group(1))
        return result

    # Snorkel S3219E
    m = re.match(r'S(\d{2})(\d{2})E$', n)
    if m:
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # Snorkel A46JRT → 46ft
    m = re.search(r'A(\d{2})JRT', n)
    if m:
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    # Socage ForSte 20D, ForSte 15A
    m = re.search(r'FORSTE\s*(\d{2})', n)
    if m:
        result["platform_height_m"] = float(m.group(1))
        return result

    # Dingli GTJZ1012, GTJZ0808
    m = re.search(r'GTJZ(\d{2})(\d{2})', n)
    if m:
        h_m = int(m.group(1))
        result["platform_height_m"] = float(h_m)
        return result

    # XCMG GTJZ1212
    m = re.search(r'GTJZ(\d{2})', n)
    if m:
        result["platform_height_m"] = float(m.group(1))
        return result

    # Manitou 170 AETJ → 17.0m
    m = re.search(r'^(\d{3})\s*AETJ', n)
    if m:
        result["platform_height_m"] = round(int(m.group(1)) / 10, 1)
        return result

    # JCB S1930E
    m = re.search(r'S(\d{2})(\d{2})E', n)
    if m:
        ft = int(m.group(1))
        result["platform_height_m"] = round(ft * 0.3048, 1)
        return result

    return result


# ---------------------------------------------------------------------------
# Crane parser (category 5)
# ---------------------------------------------------------------------------
def parse_crane(name: str, brand: str) -> dict:
    result: dict = {}
    n = name.upper().strip()

    # Liebherr LTM 1100, LTM 1070, LTM 1090, LTM 1300
    m = re.search(r'LTM\s*1(\d{3})', n)
    if m:
        result["capacity_t"] = float(m.group(1))
        return result

    # Grove GMK5150, GMK4100L, GMK6300L
    m = re.search(r'GMK\d(\d{3})', n)
    if m:
        result["capacity_t"] = float(m.group(1))
        return result

    # Grove RT540E → 40t, RT770E → 70t
    m = re.search(r'RT(\d)(\d{2})E', n)
    if m:
        result["capacity_t"] = float(m.group(2))
        return result

    # Kato NK-250 → 25t, NK-500E → 50t
    m = re.search(r'NK-?(\d{3})', n)
    if m:
        result["capacity_t"] = float(int(m.group(1)) / 10)
        return result

    # Tadano ATF 90G-4 → 90t
    m = re.search(r'ATF\s*(\d{2,3})', n)
    if m:
        result["capacity_t"] = float(m.group(1))
        return result

    # Tadano GR-500EX → 50t, GR-1000EX → 100t
    m = re.search(r'GR-?(\d{3,4})(?:EX)?', n)
    if m:
        val = int(m.group(1))
        result["capacity_t"] = float(val / 10)
        return result

    # Zoomlion ZTC250V → 25t, ZTC300V → 30t, ZTC550V → 55t
    m = re.search(r'ZTC(\d{3})', n)
    if m:
        result["capacity_t"] = float(int(m.group(1)) / 10)
        return result

    # Manitowoc MLC300 → 300t, MLC650 → 650t (crawler cranes, direct tonnage)
    m = re.search(r'MLC\s*(\d{3,4})', n)
    if m:
        result["capacity_t"] = float(m.group(1))
        return result

    # XCMG QY25K5 → 25t, QY50KD → 50t
    m = re.search(r'QY(\d{2,3})', n)
    if m:
        result["capacity_t"] = float(m.group(1))
        return result

    # XCMG XCT55 → 55t
    m = re.search(r'XCT(\d{2,3})', n)
    if m:
        result["capacity_t"] = float(m.group(1))
        return result

    # Sany SAC2200 → 220t, STC500 → 50t
    m = re.search(r'SAC(\d{3,4})', n)
    if m:
        result["capacity_t"] = float(int(m.group(1)) / 10)
        return result
    m = re.search(r'STC(\d{3})', n)
    if m:
        result["capacity_t"] = float(int(m.group(1)) / 10)
        return result

    # Palfinger PK 23500, PK 18500 → capacity in kg*m, not tonnage – skip crane tonnage
    if "PK " in n:
        return result

    # Potain tower cranes MCT 85, MCT 88, MDT 178, MDT 259 → max load tonnes
    m = re.search(r'MCT\s*(\d{2,3})', n)
    if m:
        return result  # tower crane tip load varies; skip

    m = re.search(r'MDT\s*(\d{2,3})', n)
    if m:
        return result  # tower crane, skip

    return result


# ---------------------------------------------------------------------------
# Concrete plant parser (category 11)
# ---------------------------------------------------------------------------
def parse_concrete_plant(name: str) -> dict:
    result: dict = {}
    n = name.upper().strip()

    # Elkomix-120, Constmach-120, M120, HZS120, HZS120G
    m = re.search(r'(\d{2,3})', n)
    if m:
        val = int(m.group(1))
        if 30 <= val <= 300:
            result["plant_capacity_m3h"] = val
    return result


# ---------------------------------------------------------------------------
# Mixer parser (category 17)
# ---------------------------------------------------------------------------
def parse_mixer(name: str) -> dict:
    result: dict = {}
    n = name.upper().strip()

    # "10 m³", "12 m³"
    m = re.search(r'(\d{1,2})\s*M[³3]', n)
    if m:
        result["drum_volume_m3"] = int(m.group(1))
        return result

    # AM 9 C, AM 10 C
    m = re.search(r'AM\s*(\d{1,2})\s*C', n)
    if m:
        result["drum_volume_m3"] = int(m.group(1))
        return result

    # SLY 10, SLY 12
    m = re.search(r'SLY\s*(\d{1,2})', n)
    if m:
        result["drum_volume_m3"] = int(m.group(1))
        return result

    # G12K → 12
    m = re.search(r'G(\d{1,2})K', n)
    if m:
        result["drum_volume_m3"] = int(m.group(1))
        return result

    # SY309C → 9 m³
    m = re.search(r'SY\d*0(\d)C', n)
    if m:
        result["drum_volume_m3"] = int(m.group(1))
        return result

    return result


# ---------------------------------------------------------------------------
# Pump parser (category 18)
# ---------------------------------------------------------------------------
def parse_pump(name: str) -> dict:
    result: dict = {}
    n = name.upper().strip()

    # S 52 SX, S 36 X → boom length
    m = re.search(r'^S\s*(\d{2})\s', n)
    if m:
        result["boom_length_m"] = int(m.group(1))
        return result

    # K47H, K41L
    m = re.search(r'^K(\d{2})', n)
    if m:
        result["boom_length_m"] = int(m.group(1))
        return result

    # H58-7RZ, H43-5RZ
    m = re.search(r'^H(\d{2})', n)
    if m:
        result["boom_length_m"] = int(m.group(1))
        return result

    # ECP56CS, ECP42CX
    m = re.search(r'ECP(\d{2})', n)
    if m:
        result["boom_length_m"] = int(m.group(1))
        return result

    # 56X-6RZ
    m = re.match(r'^(\d{2})X', n)
    if m:
        result["boom_length_m"] = int(m.group(1))
        return result

    # M36, M42-5, M38-5
    m = re.match(r'^M(\d{2})', n)
    if m:
        result["boom_length_m"] = int(m.group(1))
        return result

    # SY5530THB → 30 actually the boom ~55? This is a model code; skip unless clear
    # HB67V → 67m
    m = re.search(r'HB(\d{2})', n)
    if m:
        result["boom_length_m"] = int(m.group(1))
        return result

    # THP 140 H, THP 160 H → stationary pump, no boom; skip
    if n.startswith("THP"):
        return result

    # BSA 1409 D → stationary, no boom
    if n.startswith("BSA"):
        return result

    return result


# ---------------------------------------------------------------------------
# Main processing
# ---------------------------------------------------------------------------
def process_models():
    rows = []
    with open(CSV_PATH, newline="", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for row in reader:
            rows.append(row)

    updates: list[dict] = []
    stats = {"hp": 0, "cap_kg": 0, "cap_t": 0, "specs": 0, "weight_refined": 0}

    for row in rows:
        mid = int(row["id"])
        brand_id = int(row["brand_id"])
        brand = row["brand"]
        cat_id = int(row["category_id"])
        name = row["name"]
        wmin_csv = float(row["wmin"]) if row["wmin"] and row["wmin"] != "NULL" else None
        wmax_csv = float(row["wmax"]) if row["wmax"] and row["wmax"] != "NULL" else None

        hp: Optional[int] = None
        capacity_kg: Optional[int] = None
        capacity_t: Optional[float] = None
        wmin: Optional[float] = wmin_csv
        wmax: Optional[float] = wmax_csv
        default_specs: dict = {}

        # OEM HP lookup
        oem_key = (brand_id, name.upper())
        if oem_key in OEM_HP:
            hp = OEM_HP[oem_key]
        if oem_key in OEM_WEIGHT:
            ow = OEM_WEIGHT[oem_key]
            if ow[0] is not None:
                wmin = ow[0]
            if ow[1] is not None:
                wmax = ow[1]

        # Category-specific parsers
        if cat_id == CAT_FORKLIFT:
            # Bobcat Sxxx / Txxx are skid-steers; ignore if mis-seeded under forklift.
            if re.match(r'^[ST]\d{2,3}\b', name.upper()):
                pass
            else:
                cap = parse_forklift_capacity_kg(name)
                if cap is not None:
                    capacity_kg = cap
                    capacity_t = round(cap / 1000, 2)
                    default_specs["payload_t"] = capacity_t

        elif cat_id == CAT_TELEHANDLER:
            parsed = parse_telehandler(name, brand)
            if parsed.get("hp") is not None and hp is None:
                hp = int(parsed["hp"])
            if "capacity_kg" in parsed:
                capacity_kg = int(parsed["capacity_kg"])
                capacity_t = round(capacity_kg / 1000, 2)
                default_specs["payload_t"] = capacity_t
            if parsed.get("lift_height_m") is not None:
                default_specs["lift_height_m"] = parsed["lift_height_m"]

        elif cat_id == CAT_AERIAL:
            parsed = parse_aerial(name, brand)
            if "platform_height_m" in parsed:
                default_specs["platform_height_m"] = parsed["platform_height_m"]

        elif cat_id == CAT_CRANE:
            parsed = parse_crane(name, brand)
            if "capacity_t" in parsed:
                capacity_t = parsed["capacity_t"]
                # capacity_kg reserved for forklift/telehandler lift capacity on listings
                default_specs["max_lift_capacity_t"] = capacity_t

        elif cat_id == CAT_CONCRETE_PLANT:
            parsed = parse_concrete_plant(name)
            if "plant_capacity_m3h" in parsed:
                default_specs["plant_capacity_m3h"] = parsed["plant_capacity_m3h"]

        elif cat_id == CAT_MIXER:
            parsed = parse_mixer(name)
            if "drum_volume_m3" in parsed:
                default_specs["drum_volume_m3"] = parsed["drum_volume_m3"]

        elif cat_id == CAT_PUMP:
            parsed = parse_pump(name)
            if "boom_length_m" in parsed:
                default_specs["boom_length_m"] = parsed["boom_length_m"]

        has_data = (
            hp is not None
            or capacity_kg is not None
            or capacity_t is not None
            or len(default_specs) > 0
            or (wmin is not None and wmin != wmin_csv)
            or (wmax is not None and wmax != wmax_csv)
        )

        if has_data:
            updates.append({
                "id": mid,
                "hp": hp,
                "capacity_kg": capacity_kg,
                "capacity_t": capacity_t,
                "wmin": wmin,
                "wmax": wmax,
                "default_specs": default_specs,
                "name": name,
            })
            if hp is not None:
                stats["hp"] += 1
            if capacity_kg is not None:
                stats["cap_kg"] += 1
            if capacity_t is not None:
                stats["cap_t"] += 1
            if len(default_specs) > 0:
                stats["specs"] += 1
            if (wmin is not None and wmin != wmin_csv) or (wmax is not None and wmax != wmax_csv):
                stats["weight_refined"] += 1

    return updates, stats, len(rows)


def sql_val(v, is_numeric=False):
    if v is None:
        return "NULL"
    if is_numeric:
        return str(v)
    return str(int(v))


def write_sql(updates: list[dict]):
    """Patch the OEM block inside 05_equipment_catalog.sql (fresh-install seed)."""
    catalog = Path(CATALOG_SQL_PATH).read_text(encoding="utf-8")
    begin = catalog.find(BEGIN_MARKER)
    end = catalog.find(END_MARKER)
    if begin < 0 or end < 0 or end < begin:
        raise SystemExit(
            f"Markers {BEGIN_MARKER!r} / {END_MARKER!r} not found in {CATALOG_SQL_PATH}"
        )

    lines = [
        BEGIN_MARKER,
        "-- Regenerated by scripts/generate_model_specs.py — do not edit by hand.",
        "-- Sources: Caterpillar Specalog, Komatsu, Hitachi, Volvo CE, Hyundai CE,",
        "-- JCB, Liebherr, Grove, Tadano, Genie/JLG, Manitou datasheets, ISO forklift naming.",
        "",
    ]
    for u in updates:
        parts = []
        if u["hp"] is not None:
            parts.append(f"horsepower = {u['hp']}")
        if u["capacity_kg"] is not None:
            parts.append(f"capacity_kg = {u['capacity_kg']}")
        if u["capacity_t"] is not None:
            parts.append(f"capacity_t = {u['capacity_t']}")
        if u["wmin"] is not None:
            parts.append(f"typical_weight_min_t = {u['wmin']}")
        if u["wmax"] is not None:
            parts.append(f"typical_weight_max_t = {u['wmax']}")
        if u["default_specs"]:
            specs_json = json.dumps(u["default_specs"], ensure_ascii=False)
            parts.append(f"default_specs = '{specs_json}'::jsonb")

        if not parts:
            continue

        set_clause = ",\n    ".join(parts)
        lines.append(f"-- {u['name']}")
        lines.append(f"UPDATE equipment_models SET\n    {set_clause}\nWHERE id = {u['id']};")
        lines.append("")

    block = "\n".join(lines).rstrip() + "\n"
    new_catalog = catalog[:begin] + block + catalog[end:]
    Path(CATALOG_SQL_PATH).write_text(new_catalog, encoding="utf-8")


def main():
    updates, stats, total = process_models()
    write_sql(updates)

    print(f"Total models in CSV:     {total}")
    print(f"Models with updates:     {len(updates)}")
    print(f"  - horsepower set:      {stats['hp']}")
    print(f"  - capacity_kg set:     {stats['cap_kg']}")
    print(f"  - capacity_t set:      {stats['cap_t']}")
    print(f"  - default_specs set:   {stats['specs']}")
    print(f"  - weight refined:      {stats['weight_refined']}")
    print(f"\nOEM block patched in: {CATALOG_SQL_PATH}")

    # Category breakdown
    rows = []
    with open(CSV_PATH, newline="", encoding="utf-8") as fcsv:
        reader = csv.DictReader(fcsv)
        for row in reader:
            rows.append(row)

    update_ids = {u["id"] for u in updates}
    hp_ids = {u["id"] for u in updates if u["hp"] is not None}
    cap_ids = {u["id"] for u in updates if u["capacity_kg"] is not None}

    cat_totals: dict[str, int] = {}
    cat_hp: dict[str, int] = {}
    cat_cap: dict[str, int] = {}
    for row in rows:
        cat = row["category"]
        mid = int(row["id"])
        cat_totals[cat] = cat_totals.get(cat, 0) + 1
        if mid in hp_ids:
            cat_hp[cat] = cat_hp.get(cat, 0) + 1
        if mid in cap_ids:
            cat_cap[cat] = cat_cap.get(cat, 0) + 1

    print("\nCategory breakdown:")
    print(f"{'Category':<25} {'Total':>5} {'HP':>5} {'Cap':>5}")
    print("-" * 45)
    for cat in sorted(cat_totals.keys()):
        t = cat_totals[cat]
        h = cat_hp.get(cat, 0)
        c = cat_cap.get(cat, 0)
        print(f"{cat:<25} {t:>5} {h:>5} {c:>5}")


if __name__ == "__main__":
    main()
