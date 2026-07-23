using AracParki.Application.Authorization;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using AracParki.Domain.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Admin.Sikayetler;

[Authorize(Policy = AuthPolicies.ListingModerate)]
public sealed class IndexModel(ListingReportService reports) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "durum")]
    public string Status { get; set; } = ListingReportStatus.Open;

    public IReadOnlyList<ListingReportListItemDto> Items { get; private set; } = [];
    public ListingReportCountsDto Counts { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Status = string.IsNullOrWhiteSpace(Status) ? ListingReportStatus.Open : Status.Trim();
        if (!ListingReportStatus.IsKnown(Status))
        {
            Status = ListingReportStatus.Open;
        }

        Counts = await reports.GetCountsAsync(cancellationToken);
        Items = await reports.ListAsync(Status, 50, cancellationToken);
        ViewData["PageKey"] = "account";
        ViewData["Title"] = "Şikayetler | Araç Parkı";
        ViewData["Robots"] = "noindex, nofollow";
        return Page();
    }
}
