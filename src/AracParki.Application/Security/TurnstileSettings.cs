namespace AracParki.Application.Security;

public sealed class TurnstileSettings
{
    public const string SectionName = "CloudflareTurnstile";

    /// <summary>Public sitekey embedded in the widget.</summary>
    public string SiteKey { get; set; } = string.Empty;

    /// <summary>Server-side secret for challenges.cloudflare.com/turnstile/v0/siteverify.</summary>
    public string Secret { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SiteKey) && !string.IsNullOrWhiteSpace(Secret);
}
