using System.Security.Claims;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Ilanlarim;

[Authorize]
public sealed class IndexModel(ListingService listings, ListingCommandService commands) : PageModel
{
    public IReadOnlyList<ListingCardDto> Items { get; private set; } = [];
    public string? FormError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
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

    public async Task<IActionResult> OnPostArchiveAsync(string adNo, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        try
        {
            await commands.ArchiveAsync(adNo, accountId, cancellationToken);
            TempData["AuthNotice"] = $"{adNo} yayından kaldırıldı.";
            return RedirectToPage();
        }
        catch (InvalidOperationException)
        {
            FormError = "Yayından kaldırılacak ilan bulunamadı.";
            await OnGetAsync(cancellationToken);
            return Page();
        }
        catch (Exception)
        {
            FormError = "İşlem başarısız. Lütfen tekrar dene.";
            await OnGetAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRepublishAsync(string adNo, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        try
        {
            await commands.RepublishAsync(adNo, accountId, cancellationToken);
            TempData["AuthNotice"] = $"{adNo} incelemeye gönderildi.";
            return RedirectToPage();
        }
        catch (InvalidOperationException)
        {
            FormError = "Yeniden yayınlanacak ilan bulunamadı.";
            await OnGetAsync(cancellationToken);
            return Page();
        }
        catch (Exception)
        {
            FormError = "İşlem başarısız. Lütfen tekrar dene.";
            await OnGetAsync(cancellationToken);
            return Page();
        }
    }

    private bool TryGetAccountId(out long accountId)
    {
        accountId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out accountId) && accountId > 0;
    }
}
