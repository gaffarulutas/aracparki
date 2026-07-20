namespace AracParki.Application.Listings;

public static class ListingImageUrl
{
    public const string UploadPrefix = "/uploads/listings/";
    public const int MaxCount = 8;
    public const long MaxUploadBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    public static bool IsAllowedContentType(string? contentType)
        => !string.IsNullOrWhiteSpace(contentType) && AllowedContentTypes.Contains(contentType);

    public static bool IsAllowed(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.Trim();
        if (trimmed.StartsWith(UploadPrefix, StringComparison.OrdinalIgnoreCase)
            && !trimmed.Contains("..", StringComparison.Ordinal)
            && trimmed.Length < 500)
        {
            return true;
        }

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
               && uri.Scheme == Uri.UriSchemeHttps
               && uri.Host.Length > 0
               && !IsBlockedHost(uri.Host);
    }

    public static bool IsBlockedHost(string host)
    {
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
            || host is "127.0.0.1" or "::1" or "0.0.0.0")
        {
            return true;
        }

        if (System.Net.IPAddress.TryParse(host, out var ip))
        {
            return System.Net.IPAddress.IsLoopback(ip)
                   || ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                      && IsPrivateIpv4(ip);
        }

        return false;
    }

    private static bool IsPrivateIpv4(System.Net.IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        return bytes[0] == 10
               || (bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
               || (bytes[0] == 192 && bytes[1] == 168)
               || (bytes[0] == 169 && bytes[1] == 254);
    }

    public static string ExtensionForContentType(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        _ => ".jpg"
    };
}
