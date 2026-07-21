namespace AracParki.Application.Listings;

/// <summary>Image asset attached to a publish command (CDN delivery URL + storage metadata).</summary>
public sealed class ListingImageAsset
{
    public required string DeliveryUrl { get; init; }
    public string? ImageId { get; init; }
    public string? StorageKey { get; init; }
    public int Version { get; init; } = 1;
    public int? Width { get; init; }
    public int? Height { get; init; }
    public long? ByteSize { get; init; }
    public string? MimeType { get; init; }
    public string? ChecksumSha256 { get; init; }
    public string? OriginalFilename { get; init; }

    public static ListingImageAsset FromUrl(string url) => new() { DeliveryUrl = url.Trim() };

    public static ListingImageAsset FromSaveResult(ListingImageSaveResult result) => new()
    {
        DeliveryUrl = result.DeliveryUrl,
        ImageId = result.ImageId,
        StorageKey = result.StorageKey,
        Version = result.Version,
        Width = result.Width,
        Height = result.Height,
        ByteSize = result.ByteSize,
        MimeType = result.MimeType,
        ChecksumSha256 = result.ChecksumSha256,
        OriginalFilename = result.OriginalFilename
    };
}
