namespace AracParki.Domain.Listings;

public static class ListingReportReason
{
    public const string SoldOrRented = "sold_or_rented";
    public const string WrongCategory = "wrong_category";
    public const string WrongInfo = "wrong_info";
    public const string Duplicate = "duplicate";
    public const string Other = "other";

    public static readonly IReadOnlyList<(string Code, string Label)> All =
    [
        (SoldOrRented, "Makine satılmış veya kiralanmış"),
        (WrongCategory, "İlan kategorisi hatalı"),
        (WrongInfo, "İlan bilgileri hatalı veya yanıltıcı"),
        (Duplicate, "Aynı ilan birden fazla kez yayınlanmış"),
        (Other, "Diğer")
    ];

    public static readonly HashSet<string> Known =
    [
        SoldOrRented,
        WrongCategory,
        WrongInfo,
        Duplicate,
        Other
    ];

    public static bool IsKnown(string? code)
        => !string.IsNullOrWhiteSpace(code) && Known.Contains(code);

    public static string Label(string code) => code switch
    {
        SoldOrRented => "Makine satılmış veya kiralanmış",
        WrongCategory => "İlan kategorisi hatalı",
        WrongInfo => "İlan bilgileri hatalı veya yanıltıcı",
        Duplicate => "Aynı ilan birden fazla kez yayınlanmış",
        Other => "Diğer",
        _ => code
    };
}
