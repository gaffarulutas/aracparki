using AracParki.Application.Listings.Commands;

namespace AracParki.Application.Listings;

public interface IListingStore
{
    /// <summary>
    /// Ensures a seller for the account, inserts published listing + images, returns ad_no.
    /// </summary>
    Task<string> CreatePublishedAsync(CreatePublishedListingCommand command, CancellationToken cancellationToken);
}
