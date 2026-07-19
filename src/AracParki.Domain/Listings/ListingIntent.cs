namespace AracParki.Domain.Listings;

public static class ListingIntent
{
    public const string All = "all";
    public const string Satilik = "satilik";
    public const string Kiralik = "kiralik";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        All,
        Satilik,
        Kiralik
    };

    public static string Label(string intent) => intent switch
    {
        Satilik => "Satılık",
        Kiralik => "Kiralık",
        _ => "Tümü"
    };

    public static string BadgeClass(string intent) => intent switch
    {
        Kiralik => "badge badge-rent",
        _ => "badge badge-sale"
    };
}
