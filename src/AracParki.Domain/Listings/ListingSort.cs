namespace AracParki.Domain.Listings;

public static class ListingSort
{
    public const string Newest = "yeni";
    public const string PriceAsc = "fiyat-artan";
    public const string PriceDesc = "fiyat-azalan";
    public const string HoursAsc = "saat";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        Newest,
        PriceAsc,
        PriceDesc,
        HoursAsc
    };
}
