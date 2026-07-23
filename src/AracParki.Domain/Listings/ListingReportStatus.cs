namespace AracParki.Domain.Listings;

public static class ListingReportStatus
{
    public const string Open = "open";
    public const string Reviewing = "reviewing";
    public const string Actioned = "actioned";
    public const string Dismissed = "dismissed";

    public static readonly HashSet<string> Known =
    [
        Open,
        Reviewing,
        Actioned,
        Dismissed
    ];

    public static readonly HashSet<string> Active =
    [
        Open,
        Reviewing
    ];

    public static bool IsKnown(string? status)
        => !string.IsNullOrWhiteSpace(status) && Known.Contains(status);

    public static bool IsActive(string? status)
        => !string.IsNullOrWhiteSpace(status) && Active.Contains(status);

    public static string Label(string status) => status switch
    {
        Open => "Yeni",
        Reviewing => "İncelemede",
        Actioned => "İşlem yapıldı",
        Dismissed => "İşlem yapılmadı",
        _ => status
    };

    public static string BadgeClass(string status) => status switch
    {
        Open => "badge badge-info",
        Reviewing => "badge badge-warn",
        Actioned => "badge badge-ok",
        Dismissed => "badge badge-muted",
        _ => "badge badge-muted"
    };
}
