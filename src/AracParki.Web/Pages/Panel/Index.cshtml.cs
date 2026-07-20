using System.Security.Claims;
using AracParki.Application.Listings.Services;
using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.Panel;

public sealed class IndexModel(ListingService listings) : AccountPageModel
{
    public string DisplayName { get; private set; } = string.Empty;

    public string Greeting { get; private set; } = "Hesabına hızlı bakış";

    public int ListingCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(raw, out var accountId))
        {
            return Challenge();
        }

        DisplayName = User.Identity?.Name ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(DisplayName))
        {
            Greeting = "Merhaba " + DisplayName + " - hesabına hızlı bakış";
        }

        var items = await listings.GetByAccountIdAsync(accountId, 50, cancellationToken);
        ListingCount = items.Count;

        SetAccountMeta("Panel", "Hesap paneli");
        return Page();
    }
}