using System.Globalization;
using System.Text.Json;

namespace AracParki.Application.Listings.Queries;

public static class SpecFilterBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public static (string? EqualityJson, string? MinJson) Build(IReadOnlyDictionary<string, string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return (null, null);
        }

        var equality = new Dictionary<string, object>(StringComparer.Ordinal);
        var mins = new Dictionary<string, decimal>(StringComparer.Ordinal);

        foreach (var (key, raw) in values)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var value = raw.Trim();
            if (bool.TryParse(value, out var flag))
            {
                equality[key] = flag;
            }
            else if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            {
                mins[key] = number;
            }
            else
            {
                equality[key] = value;
            }
        }

        return (
            equality.Count == 0 ? null : JsonSerializer.Serialize(equality, JsonOptions),
            mins.Count == 0 ? null : JsonSerializer.Serialize(mins, JsonOptions));
    }
}
