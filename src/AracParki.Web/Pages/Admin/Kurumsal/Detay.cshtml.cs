using System.Security.Claims;
using AracParki.Application.Authorization;
using AracParki.Application.Corporate.Dtos;
using AracParki.Application.Corporate.Services;
using AracParki.Domain.Corporate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Admin.Kurumsal;

[Authorize(Policy = AuthPolicies.ListingModerate)]
public sealed class DetayModel(CorporateAccountService corporate) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    public CorporateAccountDto? Account { get; private set; }
    public IReadOnlyList<CorporateDocumentDto> Documents { get; private set; } = [];
    public string? FormError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        Account = await corporate.GetForModerationAsync(Id, cancellationToken);
        if (Account is null)
        {
            return NotFound();
        }

        Documents = await corporate.ListDocumentsAsync(Id, cancellationToken);
        ViewData["PageKey"] = "account";
        ViewData["Title"] = $"Kurumsal · {Account.DisplayName} | Araç Parkı";
        ViewData["Robots"] = "noindex, nofollow";
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAdminId(out var adminId))
        {
            return Challenge();
        }

        var (ok, error) = await corporate.ApproveAsync(Id, adminId, cancellationToken);
        if (ok)
        {
            TempData["AuthNotice"] = "Kurumsal hesap onaylandı.";
            return RedirectToPage("/Admin/Kurumsal/Index", new { durum = CorporateStatus.Pending });
        }

        FormError = error;
        await OnGetAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostRejectAsync(string? reason, CancellationToken cancellationToken)
    {
        if (!TryGetAdminId(out var adminId))
        {
            return Challenge();
        }

        var (ok, error) = await corporate.RejectAsync(Id, adminId, reason, cancellationToken);
        if (ok)
        {
            TempData["AuthNotice"] = "Kurumsal hesap reddedildi.";
            return RedirectToPage("/Admin/Kurumsal/Index", new { durum = CorporateStatus.Pending });
        }

        FormError = error;
        await OnGetAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnGetDocumentAsync(long documentId, CancellationToken cancellationToken)
    {
        if (!TryGetAdminId(out var adminId))
        {
            return Challenge();
        }

        var opened = await corporate.OpenDocumentAsync(documentId, adminId, requesterIsAdmin: true, cancellationToken);
        if (opened is null)
        {
            return NotFound();
        }

        var (content, document) = opened.Value;
        return File(content, document.ContentType, document.FileName);
    }

    private bool TryGetAdminId(out long adminId)
    {
        adminId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out adminId) && adminId > 0;
    }
}
