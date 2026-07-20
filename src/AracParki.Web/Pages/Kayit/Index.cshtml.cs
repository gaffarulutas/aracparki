using AracParki.Application.Accounts.Services;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace AracParki.Web.Pages.Kayit;

public sealed class IndexModel(AccountService accounts) : PageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string? FormError { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/");
        }

        SetMeta();
        return Page();
    }

    [EnableRateLimiting("auth-sensitive")]
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        SetMeta();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (ok, error, emailSent) = await accounts.RegisterAsync(
            Input.Email,
            Input.Password,
            Input.FirstName,
            Input.LastName,
            cancellationToken);

        if (!ok)
        {
            FormError = error ?? "Kayıt başarısız.";
            return Page();
        }

        // Soft gate: sign in immediately; sticky banner nudges email verification.
        var (loginOk, _, account) = await accounts.LoginAsync(Input.Email, Input.Password, cancellationToken);
        if (loginOk && account is not null)
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                AuthCookie.CreatePrincipal(account),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                });
        }

        TempData["AuthNotice"] = emailSent
            ? "Hesabın hazır. Onay mailini gönderdik — üstteki bardan tekrar gönderebilirsin."
            : "Hesabın hazır. Onay maili şu an gönderilemedi; üstteki bardan tekrar dene.";

        return Redirect("/");
    }

    private void SetMeta()
    {
        ViewData["PageKey"] = "auth";
        ViewData["Title"] = "Hesap Oluştur | Araç Parkı";
        ViewData["Description"] = "Araç Parkı’na üye ol — satılık ve kiralık iş makinesi ilanı ver.";
        ViewData["Robots"] = "noindex, nofollow";
    }
}
