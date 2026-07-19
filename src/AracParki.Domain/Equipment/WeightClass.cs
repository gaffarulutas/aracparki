namespace AracParki.Domain.Equipment;

/// <summary>Operating-weight bands (ISO 6016 oriented) for excavator-class filters.</summary>
public static class WeightClass
{
    public static readonly (string Key, string Label, decimal? Min, decimal? Max)[] Presets =
    [
        ("mini", "0–6 t", 0m, 6m),
        ("midi", "6–10 t", 6m, 10m),
        ("standard", "10–25 t", 10m, 25m),
        ("large", "25–45 t", 25m, 45m),
        ("xlarge", "45+ t", 45m, null)
    ];
}
