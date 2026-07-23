using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AracParki.Application.Accounts;
using AracParki.Application.Accounts.Dtos;
using AracParki.Application.Accounts.Services;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AracParki.Web.Pages.Bilgilerim;

[Authorize]
public sealed class IndexModel(
    AccountService accounts,
    IPhoneOtpService phoneOtp,
    ILogger<IndexModel> logger) : AccountPageModel
{
    public AccountDto? Account { get; private set; }

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    [TempData]
    public string? Notice { get; set; }

    /// <summary>Same-request validation / save errors (not TempData — avoids sticky flash on refresh).</summary>
    public string? FormError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (await LoadAsync(cancellationToken) is { } fail)
        {
            return fail;
        }

        SetMeta();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        if (await LoadAsync(cancellationToken, bindInput: false) is { } fail)
        {
            return fail;
        }

        if (!ModelState.IsValid)
        {
            FormError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                ?? "Formu kontrol et.";
            SetMeta();
            return Page();
        }

        var (ok, error, updated) = await accounts.UpdateProfileAsync(
            Account!.Id,
            Input.FirstName,
            Input.LastName,
            cancellationToken);

        if (!ok || updated is null)
        {
            FormError = error ?? "Bilgiler kaydedilemedi.";
            SetMeta();
            return Page();
        }

        await RefreshAuthCookieAsync(updated);
        Notice = "Bilgilerin güncellendi.";
        return RedirectToPage();
    }

    [EnableRateLimiting("auth-sensitive")]
    public async Task<IActionResult> OnPostResendEmailAsync(CancellationToken cancellationToken)
    {
        if (await LoadAsync(cancellationToken) is { } fail)
        {
            return fail;
        }

        if (Account!.EmailConfirmed)
        {
            Notice = "E-posta zaten doğrulanmış.";
            return RedirectToPage();
        }

        try
        {
            await accounts.ResendEmailVerificationAsync(Account.Email, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resend verification failed for account {AccountId}", Account.Id);
            Notice = "Doğrulama e-postası şu an gönderilemedi. Biraz sonra tekrar dene.";
            return RedirectToPage();
        }

        Notice = "Doğrulama bağlantısı e-posta adresine gönderildi (gelmezse spam klasörünü kontrol et).";
        return RedirectToPage();
    }

    [EnableRateLimiting("phone-otp")]
    public async Task<IActionResult> OnPostSendPhoneOtpAsync(
        string? phone,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId is null)
        {
            return Challenge();
        }

        var account = await accounts.GetByIdAsync(accountId.Value, cancellationToken);
        if (account is null)
        {
            return Challenge();
        }

        var normalized = AccountService.NormalizePhone(phone);
        if (normalized is null)
        {
            return new JsonResult(new { ok = false, error = "Geçerli bir telefon numarası gir (10–15 rakam)." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var current = AccountService.NormalizePhone(account.Phone);
        if (account.PhoneConfirmed
            && string.Equals(current, normalized, StringComparison.Ordinal))
        {
            return new JsonResult(new { ok = true, alreadyVerified = true });
        }

        // Do not persist phone here — only send OTP. Account.phone updates on successful verify.
        var (ok, error, devCode) = await phoneOtp.SendAsync(accountId.Value, normalized, cancellationToken);
        if (!ok)
        {
            return new JsonResult(new { ok = false, error = error ?? "Doğrulama kodu gönderilemedi." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        return new JsonResult(new
        {
            ok = true,
            message = "Kod WhatsApp’a gönderildi. Doğrulama başarılı olunca numara kaydedilir.",
            maskedPhone = AccountService.MaskPhone(normalized),
            devCode
        });
    }

    [EnableRateLimiting("phone-otp-verify")]
    public async Task<IActionResult> OnPostVerifyPhoneOtpAsync(
        string? phone,
        string? otpCode,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId is null)
        {
            return Challenge();
        }

        var account = await accounts.GetByIdAsync(accountId.Value, cancellationToken);
        if (account is null)
        {
            return Challenge();
        }

        var normalized = AccountService.NormalizePhone(phone);
        if (normalized is null)
        {
            return new JsonResult(new { ok = false, error = "Geçerli bir telefon numarası gir." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var current = AccountService.NormalizePhone(account.Phone);
        if (account.PhoneConfirmed
            && string.Equals(current, normalized, StringComparison.Ordinal))
        {
            return new JsonResult(new { ok = true, alreadyVerified = true, reload = true });
        }

        if (string.IsNullOrWhiteSpace(otpCode))
        {
            return new JsonResult(new { ok = false, error = "Doğrulama kodunu gir." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        // VerifyAsync persists phone + confirmation only after OTP succeeds.
        var (ok, error) = await phoneOtp.VerifyAsync(accountId.Value, normalized, otpCode, cancellationToken);
        if (!ok)
        {
            return new JsonResult(new { ok = false, error = error ?? "Doğrulama başarısız." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var refreshed = await accounts.GetByIdAsync(accountId.Value, cancellationToken);
        if (refreshed is not null)
        {
            await RefreshAuthCookieAsync(refreshed);
        }

        return new JsonResult(new { ok = true, reload = true });
    }

    private async Task RefreshAuthCookieAsync(AccountDto account)
    {
        var existing = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var props = existing.Properties ?? new AuthenticationProperties();
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthCookie.CreatePrincipal(account),
            props);
    }

    private async Task<IActionResult?> LoadAsync(CancellationToken cancellationToken, bool bindInput = true)
    {
        var accountId = GetAccountId();
        if (accountId is null)
        {
            return Challenge();
        }

        Account = await accounts.GetByIdAsync(accountId.Value, cancellationToken);
        if (Account is null)
        {
            return Challenge();
        }

        if (bindInput)
        {
            BindInputFromAccount(Account);
        }

        return null;
    }

    private void BindInputFromAccount(AccountDto account)
    {
        Input = new ProfileInput
        {
            FirstName = account.FirstName,
            LastName = account.LastName
        };
    }

    private void SetMeta()
        => SetAccountMeta(
            "Bilgilerim",
            "Kimlik, e-posta, telefon ve doğrulama bilgilerini yönet");

    private long? GetAccountId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out var id) ? id : null;
    }

    public sealed class ProfileInput
    {
        [Required(ErrorMessage = "Ad gerekli.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Ad 2–40 karakter olmalı.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gerekli.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Soyad 2–40 karakter olmalı.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;
    }
}
