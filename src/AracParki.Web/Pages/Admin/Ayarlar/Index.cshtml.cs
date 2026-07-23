using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AracParki.Application.Authorization;
using AracParki.Application.Common;
using AracParki.Application.Site.Dtos;
using AracParki.Application.Site.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Admin.Ayarlar;

[Authorize(Policy = AuthPolicies.ListingModerate)]
public sealed class IndexModel(SiteSettingsService siteSettings) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? FormError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var s = await siteSettings.GetAsync(cancellationToken);
        Input = InputModel.From(s);
        SetMeta();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        SetMeta();
        NormalizeOptionalFields();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ValidateBusinessRules() is { } error)
        {
            FormError = error;
            return Page();
        }

        if (!TryGetAdminId(out var adminId))
        {
            return Challenge();
        }

        await siteSettings.UpdateAsync(Input.ToDto(), adminId, cancellationToken);
        TempData["AuthNotice"] = "Site ayarları kaydedildi.";
        return RedirectToPage();
    }

    private void NormalizeOptionalFields()
    {
        // Empty strings from the form fail [EmailAddress] / look “set”; treat as null.
        Input.AdsEmail = NullIfWhiteSpace(Input.AdsEmail);
        Input.SupportPhone = NullIfWhiteSpace(Input.SupportPhone);
        Input.WhatsAppPhone = NullIfWhiteSpace(Input.WhatsAppPhone);
        Input.WorkingHours = NullIfWhiteSpace(Input.WorkingHours);
        Input.ResponseNote = NullIfWhiteSpace(Input.ResponseNote);
        Input.LegalCompanyName = NullIfWhiteSpace(Input.LegalCompanyName);
        Input.AddressLine = NullIfWhiteSpace(Input.AddressLine);
        Input.City = NullIfWhiteSpace(Input.City);
        Input.PostalCode = NullIfWhiteSpace(Input.PostalCode);
        Input.FooterTagline = NullIfWhiteSpace(Input.FooterTagline);
        Input.InstagramUrl = NullIfWhiteSpace(Input.InstagramUrl);
        Input.FacebookUrl = NullIfWhiteSpace(Input.FacebookUrl);
        Input.TwitterUrl = NullIfWhiteSpace(Input.TwitterUrl);
        Input.YoutubeUrl = NullIfWhiteSpace(Input.YoutubeUrl);
        Input.LinkedInUrl = NullIfWhiteSpace(Input.LinkedInUrl);
        Input.TikTokUrl = NullIfWhiteSpace(Input.TikTokUrl);

        if (Input.AdsEmail is null)
        {
            ModelState.Remove($"{nameof(Input)}.{nameof(Input.AdsEmail)}");
        }
    }

    private string? ValidateBusinessRules()
    {
        if (string.IsNullOrWhiteSpace(Input.SupportEmail)
            || !new EmailAddressAttribute().IsValid(Input.SupportEmail.Trim()))
        {
            return "Destek e-postası geçerli olmalı.";
        }

        if (Input.AdsEmail is not null && !new EmailAddressAttribute().IsValid(Input.AdsEmail))
        {
            return "Reklam e-postası geçerli olmalı.";
        }

        if (Input.SupportPhone is not null && Formatters.PhoneDigits(Input.SupportPhone) is null)
        {
            return "Destek telefonu geçersiz. Örn: 0532 123 45 67";
        }

        if (Input.WhatsAppPhone is not null && Formatters.PhoneDigits(Input.WhatsAppPhone) is null)
        {
            return "WhatsApp numarası geçersiz. Örn: 0532 123 45 67";
        }

        foreach (var (label, url) in new (string, string?)[]
                 {
                     ("Instagram", Input.InstagramUrl),
                     ("Facebook", Input.FacebookUrl),
                     ("X / Twitter", Input.TwitterUrl),
                     ("YouTube", Input.YoutubeUrl),
                     ("LinkedIn", Input.LinkedInUrl),
                     ("TikTok", Input.TikTokUrl)
                 })
        {
            if (url is not null && !SiteSettingsDto.TryNormalizeHttpUrl(url, out _))
            {
                return $"{label} adresi https:// ile başlayan geçerli bir URL olmalı.";
            }
        }

        return null;
    }

    private void SetMeta()
    {
        ViewData["PageKey"] = "account";
        ViewData["Title"] = "Site ayarları | Admin";
        ViewData["Robots"] = "noindex, nofollow";
    }

    private bool TryGetAdminId(out long adminId)
    {
        adminId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out adminId) && adminId > 0;
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed class InputModel
    {
        [Display(Name = "Destek e-postası")]
        [Required(ErrorMessage = "Destek e-postası gerekli.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [StringLength(200)]
        public string SupportEmail { get; set; } = string.Empty;

        [Display(Name = "Destek telefonu")]
        [StringLength(32)]
        public string? SupportPhone { get; set; }

        [Display(Name = "WhatsApp destek")]
        [StringLength(32)]
        public string? WhatsAppPhone { get; set; }

        [Display(Name = "Reklam / iş birliği e-postası")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [StringLength(200)]
        public string? AdsEmail { get; set; }

        [Display(Name = "Çalışma saatleri")]
        [StringLength(200)]
        public string? WorkingHours { get; set; }

        [Display(Name = "Yanıt süresi notu")]
        [StringLength(300)]
        public string? ResponseNote { get; set; }

        [Display(Name = "Görünen marka adı")]
        [Required(ErrorMessage = "Marka adı gerekli.")]
        [StringLength(120)]
        public string CompanyDisplayName { get; set; } = "Araç Parkı";

        [Display(Name = "Resmi ünvan")]
        [StringLength(200)]
        public string? LegalCompanyName { get; set; }

        [Display(Name = "Adres")]
        [StringLength(300)]
        public string? AddressLine { get; set; }

        [Display(Name = "Şehir")]
        [StringLength(80)]
        public string? City { get; set; }

        [Display(Name = "Posta kodu")]
        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Display(Name = "Footer açıklaması")]
        [StringLength(400)]
        public string? FooterTagline { get; set; }

        [Display(Name = "Instagram")]
        [StringLength(300)]
        public string? InstagramUrl { get; set; }

        [Display(Name = "Facebook")]
        [StringLength(300)]
        public string? FacebookUrl { get; set; }

        [Display(Name = "X (Twitter)")]
        [StringLength(300)]
        public string? TwitterUrl { get; set; }

        [Display(Name = "YouTube")]
        [StringLength(300)]
        public string? YoutubeUrl { get; set; }

        [Display(Name = "LinkedIn")]
        [StringLength(300)]
        public string? LinkedInUrl { get; set; }

        [Display(Name = "TikTok")]
        [StringLength(300)]
        public string? TikTokUrl { get; set; }

        public static InputModel From(SiteSettingsDto s) => new()
        {
            SupportEmail = s.SupportEmail,
            SupportPhone = s.SupportPhone,
            WhatsAppPhone = s.WhatsAppPhone,
            AdsEmail = s.AdsEmail,
            WorkingHours = s.WorkingHours,
            ResponseNote = s.ResponseNote,
            CompanyDisplayName = s.CompanyDisplayName,
            LegalCompanyName = s.LegalCompanyName,
            AddressLine = s.AddressLine,
            City = s.City,
            PostalCode = s.PostalCode,
            FooterTagline = s.FooterTagline,
            InstagramUrl = s.InstagramUrl,
            FacebookUrl = s.FacebookUrl,
            TwitterUrl = s.TwitterUrl,
            YoutubeUrl = s.YoutubeUrl,
            LinkedInUrl = s.LinkedInUrl,
            TikTokUrl = s.TikTokUrl
        };

        public SiteSettingsDto ToDto() => new()
        {
            SupportEmail = SupportEmail.Trim(),
            SupportPhone = NullIfWhiteSpace(SupportPhone),
            WhatsAppPhone = NullIfWhiteSpace(WhatsAppPhone),
            AdsEmail = NullIfWhiteSpace(AdsEmail),
            WorkingHours = NullIfWhiteSpace(WorkingHours),
            ResponseNote = NullIfWhiteSpace(ResponseNote),
            CompanyDisplayName = CompanyDisplayName.Trim(),
            LegalCompanyName = NullIfWhiteSpace(LegalCompanyName),
            AddressLine = NullIfWhiteSpace(AddressLine),
            City = NullIfWhiteSpace(City),
            PostalCode = NullIfWhiteSpace(PostalCode),
            FooterTagline = NullIfWhiteSpace(FooterTagline),
            InstagramUrl = NormalizeUrl(InstagramUrl),
            FacebookUrl = NormalizeUrl(FacebookUrl),
            TwitterUrl = NormalizeUrl(TwitterUrl),
            YoutubeUrl = NormalizeUrl(YoutubeUrl),
            LinkedInUrl = NormalizeUrl(LinkedInUrl),
            TikTokUrl = NormalizeUrl(TikTokUrl)
        };

        private static string? NormalizeUrl(string? value)
            => SiteSettingsDto.TryNormalizeHttpUrl(value, out var normalized) ? normalized : NullIfWhiteSpace(value);

        private static string? NullIfWhiteSpace(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
