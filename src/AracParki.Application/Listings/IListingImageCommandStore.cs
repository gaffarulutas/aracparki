namespace AracParki.Application.Listings;

public interface IListingImageCommandStore
{
    Task<IReadOnlyList<ListingImageRecord>> ListByAdNoForAccountAsync(
        string adNo,
        long accountId,
        CancellationToken cancellationToken);

    Task<bool> SoftDeleteAsync(
        string adNo,
        long accountId,
        long imageId,
        TimeSpan gracePeriod,
        CancellationToken cancellationToken);

    Task<bool> SetCoverAsync(
        string adNo,
        long accountId,
        long imageId,
        CancellationToken cancellationToken);

    Task<bool> ReorderAsync(
        string adNo,
        long accountId,
        IReadOnlyList<long> imageIdsInOrder,
        CancellationToken cancellationToken);
}

public sealed class ListingImageRecord
{
    public long Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string? ImageId { get; init; }
    public string? StorageKey { get; init; }
    public int SortOrder { get; init; }
    public bool IsCover { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public string? MimeType { get; init; }
    public int Version { get; init; }
}
