using AracParki.Application.Abstractions;
using AracParki.Application.Accounts.Services;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace AracParki.Web.Pages.Giris;

public sealed class IndexModel(AccountService accounts, ITurnstileVerifier turnstile) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }
    public string? FormError { get; private set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(SafeReturn(returnUrl));
        }

        ReturnUrl = returnUrl;
        SetMeta();
        return Page();
    }

    [EnableRateLimiting("auth-sensitive")]
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ReturnUrl = returnUrl;
        SetMeta();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var turnstileToken = Request.Form["cf-turnstile-response"].ToString();
        if (!await turnstile.VerifyAsync(turnstileToken, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken))
        {
            FormError = "Bot doğrulaması başarısız. Lütfen tekrar dene.";
            return Page();
        }

        var (ok, error, account) = await accounts.LoginAsync(Input.Email, Input.Password, cancellationToken);
        if (!ok || account is null)
        {
            FormError = error ?? "Giriş başarısız.";
            return Page();
        }

        var props = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthCookie.CreatePrincipal(account),
            props);

        return LocalRedirect(SafeReturn(returnUrl));
    }

    private void SetMeta()
    {
        ViewData["PageKey"] = "auth";
        ViewData["Title"] = "Giriş Yap | Araç Parkı";
        ViewData["Description"] = "Araç Parkı hesabına giriş yap — ilanlarını yönet, teklifleri takip et.";
        ViewData["Robots"] = "noindex, nofollow";
    }

    private string SafeReturn(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : "/";
}
