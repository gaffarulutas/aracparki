using AracParki.Application.Authorization;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Admin;

[Authorize(Policy = AuthPolicies.ListingModerate)]
public sealed class IndexModel(ListingModerationService moderation) : PageModel
{
    public ModerationCountsDto Counts { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Counts = await moderation.GetCountsAsync(cancellationToken);
        ViewData["PageKey"] = "account";
        ViewData["Title"] = "Admin | Araç Parkı";
        ViewData["Robots"] = "noindex, nofollow";
        return Page();
    }
}
