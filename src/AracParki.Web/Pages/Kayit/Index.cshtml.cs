using AracParki.Application.Accounts.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Kayit;

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
    public RegisterInput Input { get; set; } = new();

    public string? FormError { get; private set; }
    public bool Submitted { get; private set; }
    public string? DevVerifyUrl { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/");
        }

        SetMeta();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        SetMeta();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (ok, error, _, verifyToken) = await _accounts.RegisterAsync(
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

        Submitted = true;
        if (_env.IsDevelopment() && !string.IsNullOrEmpty(verifyToken))
        {
            DevVerifyUrl = $"/eposta-dogrula?token={Uri.EscapeDataString(verifyToken)}";
        }

        return Page();
    }

    private void SetMeta()
    {
        ViewData["PageKey"] = "auth";
        ViewData["Title"] = "Hesap Oluştur | Araç Parkı";
        ViewData["Description"] = "Araç Parkı’na üye ol — satılık ve kiralık iş makinesi ilanı ver.";
    }
}
