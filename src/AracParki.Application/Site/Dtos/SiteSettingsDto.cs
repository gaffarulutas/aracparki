using AracParki.Application.Common;

namespace AracParki.Application.Site.Dtos;

public sealed class SiteSettingsDto
{
    public string SupportEmail { get; set; } = "destek@aracparki.com";
    public string? SupportPhone { get; set; }
    public string? WhatsAppPhone { get; set; }
    public string? AdsEmail { get; set; }
    public string? WorkingHours { get; set; }
    public string? ResponseNote { get; set; }
    public string CompanyDisplayName { get; set; } = "Araç Parkı";
    public string? LegalCompanyName { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? FooterTagline { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TikTokUrl { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string EffectiveAdsEmail =>
        string.IsNullOrWhiteSpace(AdsEmail) ? SupportEmail : AdsEmail.Trim();

    public string SupportPhoneDisplay => Formatters.PhoneDisplay(SupportPhone);
    public string? SupportPhoneTel => Formatters.PhoneTel(SupportPhone);
    public string WhatsAppPhoneDisplay => Formatters.PhoneDisplay(WhatsAppPhone);

    public string? WhatsAppChatUrl
    {
        get
        {
            var digits = Formatters.PhoneDigits(WhatsAppPhone);
            return digits is null ? null : "https://wa.me/90" + digits;
        }
    }

    public string? FormattedAddress
    {
        get
        {
            var parts = new[] { AddressLine, PostalCode, City }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p!.Trim())
                .ToArray();
            return parts.Length == 0 ? null : string.Join(", ", parts);
        }
    }

    public bool HasAnySocial =>
        IsHttpUrl(InstagramUrl)
        || IsHttpUrl(FacebookUrl)
        || IsHttpUrl(TwitterUrl)
        || IsHttpUrl(YoutubeUrl)
        || IsHttpUrl(LinkedInUrl)
        || IsHttpUrl(TikTokUrl);

    public IReadOnlyList<(string Label, string Url, string Icon)> SocialLinks
    {
        get
        {
            var list = new List<(string, string, string)>(6);
            Add(list, "Instagram", InstagramUrl, "instagram");
            Add(list, "Facebook", FacebookUrl, "facebook");
            Add(list, "X", TwitterUrl, "twitter");
            Add(list, "YouTube", YoutubeUrl, "youtube");
            Add(list, "LinkedIn", LinkedInUrl, "linkedin");
            Add(list, "TikTok", TikTokUrl, "tiktok");
            return list;
        }
    }

    /// <summary>Fresh defaults — never reuse a shared mutable instance.</summary>
    public static SiteSettingsDto CreateDefaults() => new()
    {
        SupportEmail = "destek@aracparki.com",
        WorkingHours = "Hafta içi 09:00–18:00 (Türkiye saati)",
        ResponseNote = "İş günlerinde genellikle 1–2 gün içinde dönüş sağlarız.",
        CompanyDisplayName = "Araç Parkı",
        FooterTagline = "Türkiye genelinde satılık, kiralık ve ikinci el iş makineleri için uzman ilan platformu.",
        UpdatedAt = DateTimeOffset.UnixEpoch
    };

    private static void Add(List<(string, string, string)> list, string label, string? url, string icon)
    {
        if (!TryNormalizeHttpUrl(url, out var normalized))
        {
            return;
        }

        list.Add((label, normalized, icon));
    }

    private static bool IsHttpUrl(string? url) => TryNormalizeHttpUrl(url, out _);

    public static bool TryNormalizeHttpUrl(string? url, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(uri.Host))
        {
            return false;
        }

        normalized = uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
        return true;
    }
}
