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
    private const string DecisionReviewing = "reviewing";
    private const string DecisionActioned = "actioned";
    private const string DecisionDismissed = "dismissed";
    private const string DecisionArchive = "archive";

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

    public async Task<IActionResult> OnPostResolveAsync(
        string? decision,
        string? adminNotes,
        CancellationToken cancellationToken)
    {
        var normalized = (decision ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            DecisionReviewing => await RunActionAsync(
                adminId => reports.MarkReviewingAsync(Id, adminId, adminNotes, cancellationToken),
                "Şikayet incelemeye alındı.",
                ListingReportStatus.Reviewing,
                cancellationToken),
            DecisionActioned => await RunActionAsync(
                adminId => reports.MarkActionedAsync(Id, adminId, adminNotes, cancellationToken),
                "Şikayette işlem yapıldı. Kullanıcıya bildirim gönderildi.",
                ListingReportStatus.Actioned,
                cancellationToken),
            DecisionDismissed => await RunActionAsync(
                adminId => reports.MarkDismissedAsync(Id, adminId, adminNotes, cancellationToken),
                "Şikayette işlem yapılmadı. Kullanıcıya bildirim gönderildi.",
                ListingReportStatus.Dismissed,
                cancellationToken),
            DecisionArchive => await ArchiveListingAsync(adminNotes, cancellationToken),
            _ => await InvalidDecisionAsync(cancellationToken)
        };
    }

    private async Task<IActionResult> ArchiveListingAsync(string? adminNotes, CancellationToken cancellationToken)
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

        if (Report.ListingStatus != ListingStatus.Published)
        {
            FormError = "Yalnızca yayındaki ilanlar arşivlenebilir.";
            await OnGetAsync(cancellationToken);
            return Page();
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

    private async Task<IActionResult> InvalidDecisionAsync(CancellationToken cancellationToken)
    {
        FormError = "Geçerli bir karar seçin.";
        await OnGetAsync(cancellationToken);
        return Page();
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
