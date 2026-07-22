namespace AracParki.Application.Listings;

public sealed class ListingOptions
{
    public const string SectionName = "Listing";

    /// <summary>How long a listing stays published after admin approve.</summary>
    public int PublishedDurationDays { get; set; } = 30;

    /// <summary>How often the expiry background job polls.</summary>
    public int ExpiryPollMinutes { get; set; } = 5;
}
