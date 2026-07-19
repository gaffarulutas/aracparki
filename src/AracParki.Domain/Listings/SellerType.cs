namespace AracParki.Domain.Listings;

public static class SellerType
{
    public const string Dealer = "dealer";
    public const string Owner = "owner";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        Dealer,
        Owner
    };

    public static string Label(string sellerType) => sellerType switch
    {
        Dealer => "Bayi",
        Owner => "Sahibi",
        _ => sellerType
    };
}
