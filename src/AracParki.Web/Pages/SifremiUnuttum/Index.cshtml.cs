using AracParki.Application.Accounts.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AracParki.Web.Pages.SifremiUnuttum;

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
    public ForgotInput Input { get; set; } = new();

    public bool Submitted { get; private set; }
    public string? DevResetUrl { get; private set; }

    public IActionResult OnGet()
    {
        ViewData["PageKey"] = "auth";
        ViewData["Title"] = "Şifremi Unuttum | Araç Parkı";
        ViewData["Description"] = "Araç Parkı hesap şifreni sıfırla.";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ViewData["PageKey"] = "auth";
        ViewData["Title"] = "Şifremi Unuttum | Araç Parkı";

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = await _accounts.RequestPasswordResetAsync(Input.Email, cancellationToken);
        Submitted = true;

        // No SMTP yet — in Development expose one-time link for QA.
        if (_env.IsDevelopment() && !string.IsNullOrEmpty(token))
        {
            DevResetUrl = $"/sifre-sifirla?token={Uri.EscapeDataString(token)}";
        }

        return Page();
    }

    public sealed class ForgotInput
    {
        [Required(ErrorMessage = "E-posta gerekli.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;
    }
}
