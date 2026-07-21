using System.Security.Claims;
using AracParki.Application.Corporate.Dtos;
using AracParki.Application.Corporate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.KurumsalHesap;

public sealed class IndexModel(CorporateAccountService corporate) : AccountPageModel
{
    public IReadOnlyList<CorporateAccountDto> Accounts { get; private set; } = [];
    public string? Notice { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(raw, out var accountId))
        {
            return Challenge();
        }

        Accounts = await corporate.ListMineAsync(accountId, cancellationToken);
        Notice = TempData["CorporateNotice"] as string;
        SetAccountMeta("Kurumsal Hesap", "Kurumsal hesaplarını yönet");
        return Page();
    }
}
