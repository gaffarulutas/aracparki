namespace AracParki.Domain.Listings;

public static class Currency
{
    public const string Try = "TRY";
    public const string Usd = "USD";
    public const string Eur = "EUR";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        Try,
        Usd,
        Eur
    };

    public static string Normalize(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            return Try;
        }

        var code = currency.Trim().ToUpperInvariant();
        return Known.Contains(code) ? code : Try;
    }

    public static string Label(string? currency) => Normalize(currency) switch
    {
        Usd => "USD",
        Eur => "EUR",
        _ => "TL"
    };

    public static string Symbol(string? currency) => Normalize(currency) switch
    {
        Usd => "$",
        Eur => "€",
        _ => "₺"
    };
}
