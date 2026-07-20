using System.Security.Claims;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Ilanlarim;

[Authorize]
public sealed class IndexModel(ListingService listings) : PageModel
{
    public IReadOnlyList<ListingCardDto> Items { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(raw, out var accountId))
        {
            return Challenge();
        }

        Items = await listings.GetByAccountIdAsync(accountId, 50, cancellationToken);

        ViewData["PageKey"] = "account";
        ViewData["Title"] = "İlanlarım | Araç Parkı";
        ViewData["Description"] = "Yayınladığın iş makinesi ilanları";
        ViewData["Robots"] = "noindex, nofollow";

        return Page();
    }
}
