namespace AracParki.Application.Listings;

public interface IListingImageStorage
{
    Task<ListingImageSaveResult> SaveAsync(
        long accountId,
        Stream content,
        string contentType,
        string? originalFilename,
        CancellationToken cancellationToken);

    /// <summary>Hard-delete the stored master object (R2 or local disk).</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
