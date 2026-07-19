namespace AracParki.Domain.Listings;

public static class PriceUnit
{
    public const string Hour = "hour";
    public const string Day = "day";
    public const string Week = "week";
    public const string Month = "month";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        Hour,
        Day,
        Week,
        Month
    };

    public static string Label(string unit) => unit switch
    {
        Hour => "Saatlik",
        Day => "Günlük",
        Week => "Haftalık",
        Month => "Aylık",
        _ => unit
    };
}
