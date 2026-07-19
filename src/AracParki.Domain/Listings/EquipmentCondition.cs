namespace AracParki.Domain.Listings;

public static class EquipmentCondition
{
    public const string New = "new";
    public const string Used = "used";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        New,
        Used
    };

    public static string Label(string condition) => condition switch
    {
        New => "Sıfır",
        Used => "İkinci el",
        _ => condition
    };

    public static string BadgeClass(string condition) => condition switch
    {
        New => "badge badge-new",
        _ => "badge badge-used"
    };
}
