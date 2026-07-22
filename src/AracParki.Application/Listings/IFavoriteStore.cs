using AracParki.Application.Listings.Dtos;

namespace AracParki.Application.Listings;

public interface IFavoriteStore
{
    Task<bool> IsFavoriteAsync(long accountId, long listingId, CancellationToken cancellationToken);

    Task<bool> ToggleAsync(long accountId, long listingId, CancellationToken cancellationToken);

    Task RemoveAsync(long accountId, long listingId, CancellationToken cancellationToken);

    Task<int> CountPublishedAsync(long accountId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ListingCardDto>> ListPublishedAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken);
}
