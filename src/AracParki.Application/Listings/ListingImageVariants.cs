namespace AracParki.Application.Listings;

/// <summary>
/// Builds Cloudflare media Worker variant URLs from a storage key.
/// Masters stay watermark-free; card/md/lg/xl/og apply a subtle centered logo watermark.
/// </summary>
public static class ListingImageVariants
{
    public const string Thumb = "thumb";
    public const string Card = "card";
    public const string Md = "md";
    public const string Lg = "lg";
    public const string Xl = "xl";
    public const string Og = "og";

    public static readonly IReadOnlyDictionary<string, int> Widths = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        [Thumb] = 160,
        [Card] = 480,
        [Md] = 768,
        [Lg] = 1280,
        [Xl] = 1920,
        [Og] = 1200
    };

    public static string DeliveryUrl(string publicBaseUrl, string storageKey, string variant)
    {
        var baseUrl = publicBaseUrl.TrimEnd('/');
        var key = storageKey.TrimStart('/');
        return $"{baseUrl}/m/{key}?v={Uri.EscapeDataString(variant)}";
    }

    public static IReadOnlyDictionary<string, string> All(string publicBaseUrl, string storageKey)
        => Widths.Keys.ToDictionary(
            v => v,
            v => DeliveryUrl(publicBaseUrl, storageKey, v),
            StringComparer.Ordinal);
}
