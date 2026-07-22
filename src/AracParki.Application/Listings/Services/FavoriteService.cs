using AracParki.Application.Listings.Dtos;

namespace AracParki.Application.Listings.Services;

public sealed class FavoriteService(IFavoriteStore store)
{
    public Task<bool> IsFavoriteAsync(long accountId, long listingId, CancellationToken cancellationToken)
    {
        if (accountId <= 0 || listingId <= 0)
        {
            return Task.FromResult(false);
        }

        return store.IsFavoriteAsync(accountId, listingId, cancellationToken);
    }

    public Task<bool> ToggleAsync(long accountId, long listingId, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(accountId));
        }

        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        return store.ToggleAsync(accountId, listingId, cancellationToken);
    }

    public Task RemoveAsync(long accountId, long listingId, CancellationToken cancellationToken)
    {
        if (accountId <= 0 || listingId <= 0)
        {
            return Task.CompletedTask;
        }

        return store.RemoveAsync(accountId, listingId, cancellationToken);
    }

    public Task<IReadOnlyList<ListingCardDto>> ListAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            return Task.FromResult<IReadOnlyList<ListingCardDto>>([]);
        }

        return store.ListPublishedAsync(accountId, Math.Clamp(take, 1, 100), cancellationToken);
    }
}
