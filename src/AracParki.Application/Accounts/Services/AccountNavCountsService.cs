using AracParki.Application.Accounts.Dtos;
using AracParki.Application.Listings;

namespace AracParki.Application.Accounts.Services;

public sealed class AccountNavCountsService(
    IListingQuery listings,
    IFavoriteStore favorites,
    ISavedSearchStore savedSearches)
{
    public async Task<AccountNavCountsDto> GetAsync(long accountId, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            return new AccountNavCountsDto();
        }

        var listingsTask = listings.CountByAccountIdAsync(accountId, cancellationToken);
        var favoritesTask = favorites.CountPublishedAsync(accountId, cancellationToken);
        var savedTask = savedSearches.CountByAccountAsync(accountId, cancellationToken);
        await Task.WhenAll(listingsTask, favoritesTask, savedTask);

        return new AccountNavCountsDto
        {
            Listings = await listingsTask,
            Favorites = await favoritesTask,
            SavedSearches = await savedTask
        };
    }
}
