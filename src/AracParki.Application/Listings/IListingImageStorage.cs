namespace AracParki.Application.Listings;

public interface IListingImageStorage
{
    Task<string> SaveAsync(
        long accountId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);
}
