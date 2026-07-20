namespace AracParki.Application.Listings;

/// <summary>
/// Maps stored enum option keys (e.g. steel_track) to Turkish labels for UI.
/// </summary>
public static class SpecOptionLabels
{
    public static string For(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return value switch
        {
            "steel_track" => "Çelik palet",
            "rubber_track" => "Kauçuk palet",
            "standard" => "Standart",
            "reduced" => "Kısaltılmış kuyruk",
            "zero" => "Sıfır kuyruk",
            "diesel" => "Dizel",
            "lpg" => "LPG",
            "electric" => "Elektrik",
            "hybrid" => "Hibrit",
            "simplex" => "Tek kademeli (simplex)",
            "duplex" => "Çift kademeli (duplex)",
            "triplex" => "Üç kademeli (triplex)",
            "mobile" => "Mobil",
            "crawler" => "Paletli",
            "tower" => "Kule vinç",
            "canopy" => "Açık kabin (kanopi)",
            "closed_cabin" => "Kapalı kabin",
            "wheel" => "Lastikli",
            "track" => "Paletli",
            "radial" => "Radyal kaldırma",
            "vertical" => "Dikey kaldırma",
            "straight_s" => "Düz bıçak (S)",
            "semi_u" => "Yarı U bıçak",
            "u_blade" => "U bıçak",
            "angle_pat" => "Açılı bıçak (PAT)",
            "single_drum_soil" => "Tek tambur — zemin",
            "tandem_asphalt" => "Tandem — asfalt",
            "pneumatic" => "Pnömatik lastik",
            "combi" => "Kombi silindir",
            "superelastic" => "Süperelastik lastik",
            "cushion" => "Cushion lastik",
            "scissor" => "Makaslı platform",
            "articulating" => "Mafsallı bom",
            "telescopic" => "Teleskopik bom",
            "vertical_mast" => "Dikey mast",
            "rigid" => "Rijit damper",
            "articulated" => "Mafsallı damper",
            "truck_tipper" => "Kamyon damper",
            "truck_mounted" => "Kamyon üzeri",
            "stationary_trailer" => "Sabit / römork",
            "mixer_pump" => "Mikser pompa",
            "placing_boom" => "Dağıtım bomu",
            "stationary" => "Sabit",
            "compact" => "Kompakt",
            "twin_shaft" => "Çift milli mikser",
            "pan" => "Pan mikser",
            "planetary" => "Planet mikser",
            "jaw" => "Çeneli kırıcı",
            "cone" => "Koni kırıcı",
            "impact" => "Darbeli kırıcı",
            "vsi" => "Dikey şaftlı (VSI)",
            "mobile_tracked" => "Mobil paletli",
            "mobile_wheeled" => "Mobil lastikli",
            "primary" => "Birincil kırma",
            "secondary" => "İkincil kırma",
            "tertiary" => "Üçüncül kırma",
            "wet" => "Yaş sistem",
            "dry" => "Kuru sistem",
            "mercedes" => "Mercedes-Benz",
            "man" => "MAN",
            "ford" => "Ford",
            "iveco" => "Iveco",
            "scania" => "Scania",
            "daf" => "DAF",
            "bmc" => "BMC",
            "mono" => "Tek parça bom",
            "two_piece" => "İki parçalı bom",
            "powershift" => "Powershift şanzıman",
            "synchroshuttle" => "Synchroshuttle şanzıman",
            "hydrostatic" => "Hidrostatik şanzıman",
            "z_bar" => "Z-bar kaldırma kolu",
            "parallel" => "Paralel kaldırma kolu",
            "torque_parallel" => "Torque paralel kol",
            "none" => "Yok",
            "manual" => "Manuel",
            "hydraulic" => "Hidrolik",
            "tamper" => "Tokmaklı tabla (tamper)",
            "vibrator" => "Vibratörlü tabla",
            "high_compaction" => "Yüksek kompaksiyon",
            "front" => "Öne yükleme",
            "rear" => "Arkaya yükleme",
            "side" => "Yana yükleme",
            "indoor_slab" => "İç mekan / düz zemin",
            "rough_terrain" => "Arazi tipi",
            "3_axle" => "3 aks",
            "4_axle" => "4 aks",
            "5_axle" => "5 aks",
            "6_axle" => "6 aks",
            "4x2" => "4x2",
            "6x4" => "6x4",
            "8x4" => "8x4",
            "6x6" => "6x6",
            "10x4" => "10x4",
            "semi_auto" => "Yarı otomatik",
            "full_auto" => "Tam otomatik",
            _ => HumanizeFallback(value)
        };
    }

    private static string HumanizeFallback(string value)
    {
        // Unknown keys: steel_track → "Steel track" style is still opaque;
        // show spaced Turkish-friendly tokens without underscores.
        var spaced = value.Replace('_', ' ').Trim();
        if (spaced.Length == 0)
        {
            return value;
        }

        return char.ToUpperInvariant(spaced[0]) + spaced[1..];
    }
}
