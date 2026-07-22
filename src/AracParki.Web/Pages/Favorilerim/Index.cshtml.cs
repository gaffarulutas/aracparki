using System.Security.Claims;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.Favorilerim;

public sealed class IndexModel(FavoriteService favorites) : AccountPageModel
{
    public IReadOnlyList<ListingCardDto> Items { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        Items = await favorites.ListAsync(accountId, 50, cancellationToken);
        SetAccountMeta("Favorilerim", "Beğendiğin iş makinesi ilanları");
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAsync(long listingId, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        await favorites.RemoveAsync(accountId, listingId, cancellationToken);
        TempData["AuthNotice"] = "İlan favorilerden çıkarıldı.";
        return RedirectToPage();
    }

    private bool TryGetAccountId(out long accountId)
    {
        accountId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out accountId) && accountId > 0;
    }
}
