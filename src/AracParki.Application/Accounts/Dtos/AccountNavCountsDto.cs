namespace AracParki.Application.Accounts.Dtos;

public sealed class AccountNavCountsDto
{
    public int Listings { get; init; }
    public int Favorites { get; init; }
    public int SavedSearches { get; init; }
    public int UnreadNotifications { get; init; }
}
