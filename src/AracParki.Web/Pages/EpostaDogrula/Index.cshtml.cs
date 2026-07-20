using AracParki.Application.Accounts.Services;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AracParki.Web.Pages.EpostaDogrula;

[EnableRateLimiting("auth-sensitive")]
public sealed class IndexModel(AccountService accounts, ILogger<IndexModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Ok { get; set; }

    public bool Confirmed { get; private set; }
    public bool PendingConfirm { get; private set; }
    public bool ResendSubmitted { get; private set; }
    public string? FormError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        SetMeta();

        if (Ok)
        {
            Confirmed = true;
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Token))
        {
            // Soft gate: no anonymous email form — resend from sticky banner when signed in.
            if (User.Identity?.IsAuthenticated == true && !AuthCookie.IsEmailConfirmed(User))
            {
                return Page();
            }

            return RedirectToPage("/Giris/Index");
        }

        var (pending, alreadyConfirmed, account, error) =
            await accounts.PeekEmailVerificationAsync(Token, cancellationToken);

        if (alreadyConfirmed && account is not null)
        {
            await SignInAsync(account);
            return RedirectToPage(new { ok = true });
        }

        if (pending)
        {
            PendingConfirm = true;
            return Page();
        }

        FormError = error ?? "Geçersiz veya süresi dolmuş bağlantı.";
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(CancellationToken cancellationToken)
    {
        SetMeta();

        if (string.IsNullOrWhiteSpace(Token))
        {
            FormError = "Geçersiz veya süresi dolmuş bağlantı.";
            return Page();
        }

        var (ok, error, account, _) = await accounts.ConfirmEmailAsync(Token, cancellationToken);
        if (!ok || account is null)
        {
            FormError = error;
            return Page();
        }

        await SignInAsync(account);
        return RedirectToPage(new { ok = true });
    }

    /// <summary>One-click resend for the signed-in account (no email form).</summary>
    public async Task<IActionResult> OnPostResendAsync(string? returnUrl, CancellationToken cancellationToken)
    {
        SetMeta();

        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Giris/Index");
        }

        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectWithNotice(
                returnUrl,
                "Oturum e-postası bulunamadı. Tekrar giriş yap.",
                ok: false,
                fallbackToPage: true);
        }

        if (AuthCookie.IsEmailConfirmed(User))
        {
            Confirmed = true;
            return Page();
        }

        try
        {
            await accounts.ResendEmailVerificationAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resend verification failed");
            return RedirectWithNotice(
                returnUrl,
                "Doğrulama e-postası şu an gönderilemedi. Biraz sonra tekrar dene.",
                ok: false,
                fallbackToPage: true);
        }

        return RedirectWithNotice(
            returnUrl,
            "Doğrulama e-postasını gönderdik. Gelen kutunu ve spam klasörünü kontrol et.",
            ok: true,
            fallbackToPage: true);
    }

    private IActionResult RedirectWithNotice(string? returnUrl, string notice, bool ok, bool fallbackToPage)
    {
        TempData["AuthNotice"] = notice;
        TempData["VerifyEmailFeedback"] = ok ? "ok" : "error";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var uri)
            && string.Equals(uri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase)
            && Url.IsLocalUrl(uri.PathAndQuery))
        {
            return LocalRedirect(uri.PathAndQuery);
        }

        if (fallbackToPage)
        {
            if (ok)
            {
                ResendSubmitted = true;
            }
            else
            {
                FormError = notice;
            }

            return Page();
        }

        return Redirect("/");
    }

    private async Task SignInAsync(Application.Accounts.Dtos.AccountDto account)
    {
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthCookie.CreatePrincipal(account),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) });
    }

    private void SetMeta()
    {
        ViewData["PageKey"] = "auth";
        ViewData["Title"] = "E-posta Doğrulama | Araç Parkı";
        ViewData["Robots"] = "noindex, nofollow";
        ViewData["Description"] = "Araç Parkı hesap e-posta doğrulama.";
    }
}
