using System.Globalization;
using System.Text.Json;
using AracParki.Application.Catalog.Dtos;

namespace AracParki.Web.Pages.IlanVer;

/// <summary>
/// Applies OEM catalog defaults from equipment_models onto the wizard draft
/// so the seller does not re-enter HP / weight / capacity / known specs.
/// </summary>
public static class CatalogModelDefaults
{
    private static readonly Dictionary<string, string[]> SpecKeyAliases = new(StringComparer.Ordinal)
    {
        ["payload_t"] = ["lift_capacity_kg", "capacity_kg", "rated_operating_capacity_kg", "payload_t"],
        ["lift_height_m"] = ["lift_height_m"],
        ["platform_height_m"] = ["working_height_m", "platform_height_m"],
        ["max_lift_capacity_t"] = ["capacity_t", "max_lift_capacity_t"],
        ["boom_length_m"] = ["boom_length_m", "vertical_reach_m"],
        ["drum_volume_m3"] = ["drum_volume_m3"],
        ["plant_capacity_m3h"] = ["capacity_m3h", "plant_capacity_m3h"]
    };

    public static void Apply(
        WizardDraft draft,
        EquipmentModelOptionDto? model,
        string? capacityMetric,
        IReadOnlyList<CategoryAttributeDto>? attributes = null)
    {
        if (model is null)
        {
            ClearCatalogLocks(draft);
            return;
        }

        draft.CatalogLocked = true;

        if (model.Horsepower is > 0)
        {
            draft.Horsepower = model.Horsepower;
            draft.HorsepowerUnknown = false;
            draft.HorsepowerFromCatalog = true;
        }
        else
        {
            draft.HorsepowerFromCatalog = false;
        }

        if (model.CapacityKg is > 0)
        {
            draft.CapacityKg = model.CapacityKg;
            draft.CapacityKgFromCatalog = true;
        }
        else
        {
            draft.CapacityKgFromCatalog = false;
        }

        var metric = capacityMetric ?? draft.CapacityMetric;
        decimal? tons = null;
        if (string.Equals(metric, "capacity_t", StringComparison.Ordinal)
            && model.CapacityT is > 0)
        {
            tons = model.CapacityT;
        }
        else if (model.TypicalWeightMinT is > 0 || model.TypicalWeightMaxT is > 0)
        {
            var min = model.TypicalWeightMinT ?? model.TypicalWeightMaxT!.Value;
            var max = model.TypicalWeightMaxT ?? model.TypicalWeightMinT!.Value;
            tons = Math.Round((min + max) / 2m, 2, MidpointRounding.AwayFromZero);
        }
        else if (model.CapacityT is > 0)
        {
            tons = model.CapacityT;
        }

        if (tons is > 0)
        {
            draft.Tons = tons.Value;
            draft.TonsFromCatalog = true;
        }
        else
        {
            draft.TonsFromCatalog = false;
        }

        MergeDefaultSpecs(draft, model.DefaultSpecsJson, attributes, model.CapacityKg);
        PruneSpecs(draft, attributes);

        if (draft.HorsepowerFromCatalog && draft.Horsepower is > 0)
        {
            draft.Specs["engine_power_hp"] = draft.Horsepower.Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public static void ClearCatalogLocks(WizardDraft draft)
    {
        draft.CatalogLocked = false;
        draft.HorsepowerFromCatalog = false;
        draft.TonsFromCatalog = false;
        draft.CapacityKgFromCatalog = false;
    }

    /// <summary>Drops spec keys that are not in the current category attribute schema.</summary>
    public static void PruneSpecs(WizardDraft draft, IReadOnlyList<CategoryAttributeDto>? attributes)
    {
        if (attributes is null || attributes.Count == 0 || draft.Specs.Count == 0)
        {
            return;
        }

        var allowed = attributes.Select(a => a.Key).ToHashSet(StringComparer.Ordinal);
        draft.Specs = draft.Specs
            .Where(kv => allowed.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);
    }

    /// <summary>
    /// Spec attribute keys that must not be overwritten from the posted form
    /// when they were filled from the OEM catalog.
    /// </summary>
    public static HashSet<string> LockedSpecKeys(WizardDraft draft)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        if (!draft.CatalogLocked)
        {
            return keys;
        }

        if (draft.HorsepowerFromCatalog)
        {
            keys.Add("engine_power_hp");
        }

        if (draft.CapacityKgFromCatalog)
        {
            keys.Add("capacity_kg");
            keys.Add("lift_capacity_kg");
            keys.Add("rated_operating_capacity_kg");
        }

        foreach (var key in SpecKeyAliases.Values.SelectMany(static a => a))
        {
            if (draft.Specs.ContainsKey(key))
            {
                keys.Add(key);
            }
        }

        return keys;
    }

    private static void MergeDefaultSpecs(
        WizardDraft draft,
        string? defaultSpecsJson,
        IReadOnlyList<CategoryAttributeDto>? attributes,
        int? capacityKg)
    {
        var allowed = attributes?.Select(a => a.Key).ToHashSet(StringComparer.Ordinal)
                      ?? new HashSet<string>(StringComparer.Ordinal);

        void Put(string attrKey, string value)
        {
            if (allowed.Count == 0 || allowed.Contains(attrKey))
            {
                draft.Specs[attrKey] = value;
            }
        }

        if (capacityKg is > 0)
        {
            Put("capacity_kg", capacityKg.Value.ToString(CultureInfo.InvariantCulture));
            Put("lift_capacity_kg", capacityKg.Value.ToString(CultureInfo.InvariantCulture));
            Put("rated_operating_capacity_kg", capacityKg.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (string.IsNullOrWhiteSpace(defaultSpecsJson) || defaultSpecsJson is "{}")
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(defaultSpecsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var rawValue = FormatJsonValue(prop.Value);
                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    continue;
                }

                if (SpecKeyAliases.TryGetValue(prop.Name, out var aliases))
                {
                    foreach (var alias in aliases)
                    {
                        var value = rawValue;
                        // payload_t (tonnes) → *_kg attributes need kg
                        if (prop.Name == "payload_t"
                            && alias.EndsWith("_kg", StringComparison.Ordinal)
                            && decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var tonnes))
                        {
                            value = ((int)(tonnes * 1000m)).ToString(CultureInfo.InvariantCulture);
                        }

                        Put(alias, value);
                    }
                }
                else
                {
                    Put(prop.Name, rawValue);
                }
            }
        }
        catch (JsonException)
        {
            // ignore invalid catalog JSON
        }
    }

    private static string FormatJsonValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Number => value.TryGetDecimal(out var d)
            ? d.ToString(CultureInfo.InvariantCulture)
            : value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.String => value.GetString() ?? "",
        _ => value.GetRawText()
    };
}
