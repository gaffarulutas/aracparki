using System.Security.Claims;
using AracParki.Application.Accounts.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Giris;

public sealed class IndexModel : PageModel
{
    private readonly AccountService _accounts;
    private readonly IHostEnvironment _env;

    public IndexModel(AccountService accounts, IHostEnvironment env)
    {
        _accounts = accounts;
        _env = env;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }
    public string? FormError { get; private set; }
    public bool NeedsEmailConfirm { get; private set; }
    public string? DevVerifyUrl { get; private set; }

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

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ReturnUrl = returnUrl;
        SetMeta();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (ok, error, account) = await _accounts.LoginAsync(Input.Email, Input.Password, cancellationToken);
        if (error == "email_unconfirmed" && account is not null)
        {
            NeedsEmailConfirm = true;
            FormError = "E-posta adresini henüz onaylamadın. Gelen kutundaki bağlantıyı kullan veya yeniden gönder.";
            var token = await _accounts.RequestEmailVerificationAsync(account.Email, cancellationToken);
            if (_env.IsDevelopment() && !string.IsNullOrEmpty(token))
            {
                DevVerifyUrl = $"/eposta-dogrula?token={Uri.EscapeDataString(token)}";
            }

            return Page();
        }

        if (!ok || account is null)
        {
            FormError = error ?? "Giriş başarısız.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Name, account.DisplayName)
        };

        if (!string.IsNullOrWhiteSpace(account.Phone))
        {
            claims.Add(new Claim("phone", account.Phone));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var props = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            props);

        return LocalRedirect(SafeReturn(returnUrl));
    }

    private void SetMeta()
    {
        ViewData["PageKey"] = "auth";
        ViewData["Title"] = "Giriş Yap | Araç Parkı";
        ViewData["Description"] = "Araç Parkı hesabına giriş yap — ilanlarını yönet, teklifleri takip et.";
    }

    private string SafeReturn(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : "/";
}
