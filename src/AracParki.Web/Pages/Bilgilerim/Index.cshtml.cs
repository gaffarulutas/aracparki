using System.Security.Claims;
using AracParki.Application.Accounts;
using AracParki.Application.Accounts.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Bilgilerim;

[Authorize]
public sealed class IndexModel(IAccountStore accounts) : PageModel
{
    public AccountDto? Account { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(raw, out var accountId))
        {
            return Challenge();
        }

        Account = await accounts.FindByIdAsync(accountId, cancellationToken);
        if (Account is null)
        {
            return Challenge();
        }

        ViewData["PageKey"] = "account";
        ViewData["Title"] = "Bilgilerim | Araç Parkı";
        ViewData["Description"] = "Ad, e-posta, telefon ve doğrulama durumu";
        ViewData["Robots"] = "noindex, nofollow";

        return Page();
    }
}
