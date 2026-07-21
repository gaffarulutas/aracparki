using AracParki.Application.Authorization;
using AracParki.Application.Corporate.Dtos;
using AracParki.Application.Corporate.Services;
using AracParki.Domain.Corporate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Admin.Kurumsal;

[Authorize(Policy = AuthPolicies.ListingModerate)]
public sealed class IndexModel(CorporateAccountService corporate) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "durum")]
    public string Status { get; set; } = CorporateStatus.Pending;

    public IReadOnlyList<CorporateAccountDto> Items { get; private set; } = [];
    public CorporateModerationCountsDto Counts { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Status = string.IsNullOrWhiteSpace(Status) ? CorporateStatus.Pending : Status.Trim();
        Counts = await corporate.GetModerationCountsAsync(cancellationToken);
        Items = await corporate.ListForModerationAsync(Status, 50, cancellationToken);
        ViewData["PageKey"] = "account";
        ViewData["Title"] = "Kurumsal hesap moderasyonu | Araç Parkı";
        ViewData["Robots"] = "noindex, nofollow";
        return Page();
    }
}
