using System.Globalization;
using AracParki.Domain.Listings;

namespace AracParki.Application.Common;

public static class Formatters
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public static string Price(decimal price, string? priceUnit, string? currency = null)
    {
        var formatted = price.ToString("N0", Tr) + " " + Currency.Label(currency);
        if (string.IsNullOrWhiteSpace(priceUnit))
        {
            return formatted;
        }

        var unitLabel = PriceUnit.Known.Contains(priceUnit)
            ? PriceUnit.Label(priceUnit)
            : priceUnit;
        return $"{formatted} / {unitLabel}";
    }

    public static string Hours(int hours) => $"{hours.ToString("N0", Tr)} saat";

    public static string Tons(decimal tons) => $"{tons.ToString("0.##", Tr)} ton";

    public static string Horsepower(int hp) => $"{hp} HP";

    public static string ListedAt(DateTimeOffset listedAt) =>
        listedAt.ToLocalTime().ToString("dd MMMM yyyy", Tr);

    public static string DateTime(DateTimeOffset value) =>
        value.ToLocalTime().ToString("d MMM yyyy · HH:mm", Tr);

    public static string Count(int count) => count.ToString("N0", Tr);

    /// <summary>
    /// Digits-only national TR number (no country code). Accepts 05…, 5…, 90…, +90….
    /// </summary>
    public static string? PhoneDigits(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length >= 12 && digits.StartsWith("90", StringComparison.Ordinal))
        {
            digits = digits[2..];
        }

        digits = digits.TrimStart('0');
        return digits.Length is >= 10 and <= 12 ? digits : null;
    }

    /// <summary>Display: +90 532 123 45 67</summary>
    public static string PhoneDisplay(string? phone)
    {
        var digits = PhoneDigits(phone);
        if (digits is null)
        {
            return string.IsNullOrWhiteSpace(phone) ? "" : phone.Trim();
        }

        if (digits.Length == 10)
        {
            return $"+90 {digits[..3]} {digits[3..6]} {digits[6..8]} {digits[8..10]}";
        }

        return "+90 " + digits;
    }

    /// <summary>E.164 href value without tel: prefix, e.g. +905321234567</summary>
    public static string? PhoneTel(string? phone)
    {
        var digits = PhoneDigits(phone);
        return digits is null ? null : "+90" + digits;
    }
}
