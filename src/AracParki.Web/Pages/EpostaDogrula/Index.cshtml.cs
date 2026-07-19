using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AracParki.Application.Accounts.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.EpostaDogrula;

public sealed class IndexModel : PageModel
{
    private readonly AccountService _accounts;
    private readonly IHostEnvironment _env;

    public IndexModel(AccountService accounts, IHostEnvironment env)
    {
        _accounts = accounts;
        _env = env;
    }

    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    [BindProperty]
    public ResendInput Input { get; set; } = new();

    public bool Confirmed { get; private set; }
    public bool ResendSubmitted { get; private set; }
    public string? FormError { get; private set; }
    public string? DevVerifyUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        SetMeta();

        if (string.IsNullOrWhiteSpace(Token))
        {
            return Page();
        }

        var (ok, error, account) = await _accounts.ConfirmEmailAsync(Token, cancellationToken);
        if (!ok || account is null)
        {
            FormError = error;
            return Page();
        }

        Confirmed = true;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Name, account.DisplayName)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) });

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        SetMeta();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = await _accounts.RequestEmailVerificationAsync(Input.Email, cancellationToken);
        ResendSubmitted = true;
        if (_env.IsDevelopment() && !string.IsNullOrEmpty(token))
        {
            DevVerifyUrl = $"/eposta-dogrula?token={Uri.EscapeDataString(token)}";
        }

        return Page();
    }

    private void SetMeta()
    {
        ViewData["PageKey"] = "auth";
        ViewData["Title"] = "E-posta Doğrulama | Araç Parkı";
        ViewData["Description"] = "Araç Parkı hesap e-posta doğrulama.";
    }

    public sealed class ResendInput
    {
        [Required(ErrorMessage = "E-posta gerekli.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;
    }
}
