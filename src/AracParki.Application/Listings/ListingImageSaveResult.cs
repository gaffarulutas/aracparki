namespace AracParki.Application.Listings;

/// <summary>Result of persisting a listing image to object storage (never local disk when Cloudflare is on).</summary>
public sealed record ListingImageSaveResult(
    string DeliveryUrl,
    string ImageId,
    string StorageKey,
    int Version,
    int Width,
    int Height,
    long ByteSize,
    string MimeType,
    string ChecksumSha256,
    string? OriginalFilename = null);
