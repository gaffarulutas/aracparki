using AracParki.Application.Authorization;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using AracParki.Domain.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Admin.Ilanlar;

[Authorize(Policy = AuthPolicies.ListingModerate)]
public sealed class IndexModel(ListingModerationService moderation) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "durum")]
    public string Status { get; set; } = ListingStatus.PendingReview;

    public IReadOnlyList<ModerationListItemDto> Items { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Items = await moderation.ListAsync(Status, 50, cancellationToken);
        Status = string.IsNullOrWhiteSpace(Status) ? ListingStatus.PendingReview : Status.Trim();
        ViewData["PageKey"] = "account";
        ViewData["Title"] = "İlan moderasyonu | Araç Parkı";
        ViewData["Robots"] = "noindex, nofollow";
        return Page();
    }
}
