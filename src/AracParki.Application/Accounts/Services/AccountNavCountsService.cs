using AracParki.Application.Accounts.Dtos;
using AracParki.Application.Listings;
using AracParki.Application.Notifications;

namespace AracParki.Application.Accounts.Services;

public sealed class AccountNavCountsService(
    IListingQuery listings,
    IFavoriteStore favorites,
    ISavedSearchStore savedSearches,
    INotificationService notifications)
{
    private long _cachedAccountId;
    private AccountNavCountsDto? _cached;

    public async Task<AccountNavCountsDto> GetAsync(long accountId, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            return new AccountNavCountsDto();
        }

        if (_cached is not null && _cachedAccountId == accountId)
        {
            return _cached;
        }

        var listingsTask = listings.CountByAccountIdAsync(accountId, cancellationToken);
        var favoritesTask = favorites.CountPublishedAsync(accountId, cancellationToken);
        var savedTask = savedSearches.CountByAccountAsync(accountId, cancellationToken);
        var unreadTask = notifications.CountUnreadAsync(accountId, cancellationToken);
        await Task.WhenAll(listingsTask, favoritesTask, savedTask, unreadTask);

        _cachedAccountId = accountId;
        _cached = new AccountNavCountsDto
        {
            Listings = await listingsTask,
            Favorites = await favoritesTask,
            SavedSearches = await savedTask,
            UnreadNotifications = await unreadTask
        };
        return _cached;
    }
}
