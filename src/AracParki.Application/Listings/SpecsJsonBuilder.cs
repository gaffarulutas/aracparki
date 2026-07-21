using System.Globalization;
using System.Text.Json;
using AracParki.Application.Catalog.Dtos;

namespace AracParki.Application.Listings;

/// <summary>
/// Builds typed specs JSON (bool/number/string) whitelisted against category attributes.
/// </summary>
public static class SpecsJsonBuilder
{
    public const int MaxValueLength = 200;
    public const int MaxJsonBytes = 8_192;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    public static (bool Ok, string? Error, string Json, Dictionary<string, string> StoredRaw) TryBuild(
        IReadOnlyDictionary<string, string>? raw,
        IReadOnlyList<CategoryAttributeDto> attributes)
    {
        var allowed = attributes.ToDictionary(a => a.Key, StringComparer.Ordinal);
        var typed = new Dictionary<string, object?>(StringComparer.Ordinal);
        var storedRaw = new Dictionary<string, string>(StringComparer.Ordinal);

        if (raw is not null && raw.Count > 0)
        {
            if (raw.Count > allowed.Count + 5)
            {
                return (false, "Çok fazla özellik gönderildi.", "{}", storedRaw);
            }

            foreach (var (keyRaw, valueRaw) in raw)
            {
                if (string.IsNullOrWhiteSpace(keyRaw) || string.IsNullOrWhiteSpace(valueRaw))
                {
                    continue;
                }

                var key = keyRaw.Trim();
                var value = valueRaw.Trim();
                if (value.Length > MaxValueLength)
                {
                    return (false, $"'{key}' değeri çok uzun.", "{}", storedRaw);
                }

                if (!allowed.TryGetValue(key, out var attr))
                {
                    return (false, "Geçersiz özellik alanı.", "{}", storedRaw);
                }

                object? typedValue;
                switch (attr.DataType)
                {
                    case "bool":
                        if (!TryParseBool(value, out var flag))
                        {
                            return (false, $"{attr.Label} için Evet/Hayır seç.", "{}", storedRaw);
                        }

                        typedValue = flag;
                        value = flag ? "true" : "false";
                        break;

                    case "number":
                        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
                            && !decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("tr-TR"), out number))
                        {
                            return (false, $"{attr.Label} sayı olmalı.", "{}", storedRaw);
                        }

                        typedValue = number;
                        value = number.ToString(CultureInfo.InvariantCulture);
                        break;

                    case "enum":
                        var options = ParseEnumOptions(attr.EnumOptionsJson);
                        if (options.Count > 0 && !options.Contains(value, StringComparer.Ordinal))
                        {
                            return (false, $"{attr.Label} için geçerli bir seçenek seç.", "{}", storedRaw);
                        }

                        typedValue = value;
                        break;

                    default:
                        typedValue = value;
                        break;
                }

                typed[key] = typedValue;
                storedRaw[key] = value;
            }
        }

        foreach (var attr in attributes.Where(a => a.IsRequired))
        {
            if (!typed.ContainsKey(attr.Key))
            {
                return (false, $"{attr.Label} zorunlu.", "{}", storedRaw);
            }
        }

        if (typed.Count == 0)
        {
            return (true, null, "{}", storedRaw);
        }

        var json = JsonSerializer.Serialize(typed, JsonOptions);
        if (System.Text.Encoding.UTF8.GetByteCount(json) > MaxJsonBytes)
        {
            return (false, "Özellik verisi çok büyük.", "{}", storedRaw);
        }

        return (true, null, json, storedRaw);
    }

    /// <summary>
    /// Re-validates an already-built specs JSON against the category attribute schema
    /// (publish / server-side defense in depth).
    /// </summary>
    public static (bool Ok, string? Error, string NormalizedJson) TryValidateJson(
        string? specsJson,
        IReadOnlyList<CategoryAttributeDto> attributes)
    {
        Dictionary<string, string>? raw = null;
        if (!string.IsNullOrWhiteSpace(specsJson) && specsJson.Trim() is not "{}")
        {
            try
            {
                using var doc = JsonDocument.Parse(specsJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return (false, "Özellik verisi geçersiz.", "{}");
                }

                raw = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var value = JsonValueToRawString(prop.Value);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        raw[prop.Name] = value;
                    }
                }
            }
            catch (JsonException)
            {
                return (false, "Özellik verisi geçersiz.", "{}");
            }
        }

        var (ok, error, json, _) = TryBuild(raw, attributes);
        return (ok, error, json);
    }

    private static string JsonValueToRawString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Number => value.TryGetDecimal(out var d)
            ? d.ToString(CultureInfo.InvariantCulture)
            : value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.String => value.GetString() ?? "",
        _ => value.GetRawText()
    };

    public static IReadOnlyList<SpecDisplayRow> ToDisplayRows(
        string? specsJson,
        IReadOnlyList<CategoryAttributeDto> attributes)
    {
        if (string.IsNullOrWhiteSpace(specsJson) || specsJson is "{}")
        {
            return [];
        }

        try
        {
            using var doc = JsonDocument.Parse(specsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return [];
            }

            var byKey = attributes.ToDictionary(a => a.Key, StringComparer.Ordinal);
            var rows = new List<SpecDisplayRow>();

            // Prefer catalog order (sort_order) so display matches the form / filters.
            foreach (var attr in attributes)
            {
                if (!doc.RootElement.TryGetProperty(attr.Key, out var value))
                {
                    continue;
                }

                rows.Add(new SpecDisplayRow(attr.Label, FormatValue(value, attr.Unit)));
            }

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (byKey.ContainsKey(prop.Name))
                {
                    continue;
                }

                rows.Add(new SpecDisplayRow(prop.Name, FormatValue(prop.Value, null)));
            }

            return rows;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string FormatValue(JsonElement value, string? unit)
    {
        var text = value.ValueKind switch
        {
            JsonValueKind.True => "Evet",
            JsonValueKind.False => "Hayır",
            JsonValueKind.Number => value.TryGetDecimal(out var d)
                ? d.ToString("0.##", CultureInfo.GetCultureInfo("tr-TR"))
                : value.ToString(),
            JsonValueKind.String => value.GetString() switch
            {
                "true" => "Evet",
                "false" => "Hayır",
                var s => SpecOptionLabels.For(s)
            },
            _ => value.ToString()
        };

        return string.IsNullOrWhiteSpace(unit) ? text : $"{text} {unit}";
    }

    private static bool TryParseBool(string value, out bool flag)
    {
        if (bool.TryParse(value, out flag))
        {
            return true;
        }

        if (value is "1" or "evet" or "Evet" or "yes")
        {
            flag = true;
            return true;
        }

        if (value is "0" or "hayır" or "Hayır" or "no")
        {
            flag = false;
            return true;
        }

        flag = false;
        return false;
    }

    private static IReadOnlyList<string> ParseEnumOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

public readonly record struct SpecDisplayRow(string Label, string Value);
