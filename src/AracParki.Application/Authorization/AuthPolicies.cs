namespace AracParki.Application.Authorization;

public static class AuthPolicies
{
    public const string ListingPublish = "Listing.Publish";
    public const string ListingModerate = "Listing.Moderate";
}

public static class AuthRoles
{
    public const string Seller = "Seller";
    public const string Moderator = "Moderator";
    /// <summary>Cookie ClaimTypes.Role value for staff admins (maps from accounts.role = admin).</summary>
    public const string Admin = "Admin";
}
