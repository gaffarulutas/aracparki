using System.Security.Claims;
using AracParki.Application.Authorization;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using AracParki.Domain.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Admin.Sikayetler;

[Authorize(Policy = AuthPolicies.ListingModerate)]
public sealed class DetayModel(
    ListingReportService reports,
    ListingModerationService moderation) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    public ListingReportDetailDto? Report { get; private set; }
    public string? FormError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        Report = await reports.GetByIdAsync(Id, cancellationToken);
        if (Report is null)
        {
            return NotFound();
        }

        ViewData["PageKey"] = "account";
        ViewData["Title"] = $"Şikayet · {Report.AdNo} | Araç Parkı";
        ViewData["Robots"] = "noindex, nofollow";
        return Page();
    }

    public async Task<IActionResult> OnPostReviewingAsync(string? adminNotes, CancellationToken cancellationToken)
        => await RunActionAsync(
            (adminId) => reports.MarkReviewingAsync(Id, adminId, adminNotes, cancellationToken),
            "Şikayet incelemeye alındı.",
            ListingReportStatus.Reviewing,
            cancellationToken);

    public async Task<IActionResult> OnPostActionedAsync(string? adminNotes, CancellationToken cancellationToken)
        => await RunActionAsync(
            (adminId) => reports.MarkActionedAsync(Id, adminId, adminNotes, cancellationToken),
            "Şikayette işlem yapıldı. Kullanıcıya bildirim gönderildi.",
            ListingReportStatus.Actioned,
            cancellationToken);

    public async Task<IActionResult> OnPostDismissedAsync(string? adminNotes, CancellationToken cancellationToken)
        => await RunActionAsync(
            (adminId) => reports.MarkDismissedAsync(Id, adminId, adminNotes, cancellationToken),
            "Şikayette işlem yapılmadı. Kullanıcıya bildirim gönderildi.",
            ListingReportStatus.Dismissed,
            cancellationToken);

    public async Task<IActionResult> OnPostArchiveListingAsync(string? adminNotes, CancellationToken cancellationToken)
    {
        if (!TryGetAdminId(out var adminId))
        {
            return Challenge();
        }

        Report = await reports.GetByIdAsync(Id, cancellationToken);
        if (Report is null)
        {
            return NotFound();
        }

        try
        {
            // Sonuçlandır + bildir önce; arşiv ikinci adım (kısmi başarısızlıkta şikayet açık kalmasın).
            await reports.MarkActionedAsync(Id, adminId, adminNotes, cancellationToken);
            await moderation.ArchiveAsync(Report.AdNo, adminId, cancellationToken);
            TempData["AuthNotice"] = $"{Report.AdNo} yayından kaldırıldı ve şikayet sonuçlandı.";
            return RedirectToPage("/Admin/Sikayetler/Index", new { durum = ListingReportStatus.Actioned });
        }
        catch (ArgumentException ex)
        {
            FormError = ex.Message;
            await OnGetAsync(cancellationToken);
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            FormError = ex.Message;
            await OnGetAsync(cancellationToken);
            return Page();
        }
        catch (Exception)
        {
            FormError = "İşlem başarısız. Lütfen tekrar deneyin.";
            await OnGetAsync(cancellationToken);
            return Page();
        }
    }

    private async Task<IActionResult> RunActionAsync(
        Func<long, Task> action,
        string notice,
        string redirectStatus,
        CancellationToken cancellationToken)
    {
        if (!TryGetAdminId(out var adminId))
        {
            return Challenge();
        }

        try
        {
            await action(adminId);
            TempData["AuthNotice"] = notice;
            return RedirectToPage("/Admin/Sikayetler/Index", new { durum = redirectStatus });
        }
        catch (ArgumentException ex)
        {
            FormError = ex.Message;
            await OnGetAsync(cancellationToken);
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            FormError = ex.Message;
            await OnGetAsync(cancellationToken);
            return Page();
        }
        catch (Exception)
        {
            FormError = "İşlem başarısız. Lütfen tekrar deneyin.";
            await OnGetAsync(cancellationToken);
            return Page();
        }
    }

    private bool TryGetAdminId(out long adminId)
    {
        adminId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out adminId) && adminId > 0;
    }
}
