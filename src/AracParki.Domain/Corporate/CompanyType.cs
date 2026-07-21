namespace AracParki.Domain.Corporate;

public static class CompanyType
{
    public const string Sahis = "sahis";
    public const string Limited = "limited";
    public const string Anonim = "anonim";
    public const string Diger = "diger";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        Sahis,
        Limited,
        Anonim,
        Diger
    };

    /// <summary>Sermaye şirketleri — MERSİS/ticaret sicil ve ek evrak zorunluluğu bunlara uygulanır.</summary>
    public static bool IsCapitalCompany(string companyType) =>
        companyType is Limited or Anonim;

    public static string Label(string companyType) => companyType switch
    {
        Sahis => "Şahıs şirketi",
        Limited => "Limited şirket",
        Anonim => "Anonim şirket",
        Diger => "Kooperatif / Diğer",
        _ => companyType
    };
}
