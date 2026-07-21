using System.Security.Claims;
using AracParki.Application.Catalog.Dtos;
using AracParki.Application.Catalog.Services;
using AracParki.Application.Corporate;
using AracParki.Application.Corporate.Dtos;
using AracParki.Application.Corporate.Services;
using AracParki.Domain.Corporate;
using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.KurumsalHesap;

public sealed class DuzenleModel(
    CorporateAccountService corporate,
    CatalogService catalog) : AccountPageModel
{
    [BindProperty(SupportsGet = true)]
    public long? Id { get; set; }

    public CorporateAccountDto? Account { get; private set; }
    public IReadOnlyList<CorporateDocumentDto> Documents { get; private set; } = [];
    public IReadOnlyList<CityOptionDto> Cities { get; private set; } = [];
    public IReadOnlyList<DistrictOptionDto> Districts { get; private set; } = [];
    public string? FormError { get; private set; }
    public string? Notice { get; private set; }

    public bool IsEditable => Account is null
        || Account.Status is CorporateStatus.Draft or CorporateStatus.Rejected;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        if (!await LoadAsync(accountId, cancellationToken))
        {
            return NotFound();
        }

        Notice = TempData["CorporateNotice"] as string;
        SetAccountMeta(
            Account is null ? "Yeni kurumsal hesap" : Account.DisplayName,
            "Kurumsal hesap bilgileri ve evraklar");
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(
        string? companyType,
        string? tradeName,
        string? displayName,
        string? taxOffice,
        string? taxNumber,
        string? mersisNo,
        string? tradeRegistryNo,
        string? kepAddress,
        string? authorizedName,
        string? phone,
        string? email,
        string? website,
        int cityId,
        int districtId,
        string? addressLine,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        var input = new CorporateProfileData(
            companyType ?? "",
            tradeName ?? "",
            displayName ?? "",
            taxOffice ?? "",
            taxNumber ?? "",
            mersisNo,
            tradeRegistryNo,
            kepAddress,
            authorizedName ?? "",
            phone ?? "",
            email ?? "",
            website,
            cityId,
            districtId,
            addressLine ?? "");

        var (ok, error, id) = await corporate.SaveAsync(accountId, Id, input, cancellationToken);
        if (!ok)
        {
            FormError = error;
            await LoadAsync(accountId, cancellationToken);
            SetAccountMeta("Kurumsal hesap", "Kurumsal hesap bilgileri ve evraklar");
            return Page();
        }

        TempData["CorporateNotice"] = "Firma bilgileri kaydedildi. Şimdi evraklarını yükleyebilirsin.";
        return RedirectToPage("/KurumsalHesap/Duzenle", new { id });
    }

    public async Task<IActionResult> OnPostUploadDocAsync(
        string? docType,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        if (Id is null or 0)
        {
            return RedirectToPage("/KurumsalHesap/Index");
        }

        if (file is null || file.Length == 0)
        {
            FormError = "Yüklenecek dosyayı seç.";
        }
        else
        {
            await using var stream = file.OpenReadStream();
            var (ok, error) = await corporate.UploadDocumentAsync(
                Id.Value,
                accountId,
                docType ?? "",
                stream,
                file.ContentType ?? "",
                file.FileName,
                file.Length,
                cancellationToken);

            if (ok)
            {
                TempData["CorporateNotice"] = $"{CorporateDocumentType.Label(docType ?? "")} yüklendi.";
                return RedirectToPage("/KurumsalHesap/Duzenle", new { id = Id });
            }

            FormError = error;
        }

        if (!await LoadAsync(accountId, cancellationToken))
        {
            return NotFound();
        }

        SetAccountMeta("Kurumsal hesap", "Kurumsal hesap bilgileri ve evraklar");
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteDocAsync(long documentId, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        if (Id is null or 0)
        {
            return RedirectToPage("/KurumsalHesap/Index");
        }

        var (ok, error) = await corporate.DeleteDocumentAsync(documentId, Id.Value, accountId, cancellationToken);
        if (ok)
        {
            TempData["CorporateNotice"] = "Evrak silindi.";
            return RedirectToPage("/KurumsalHesap/Duzenle", new { id = Id });
        }

        FormError = error;
        if (!await LoadAsync(accountId, cancellationToken))
        {
            return NotFound();
        }

        SetAccountMeta("Kurumsal hesap", "Kurumsal hesap bilgileri ve evraklar");
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        if (Id is null or 0)
        {
            return RedirectToPage("/KurumsalHesap/Index");
        }

        var (ok, error) = await corporate.SubmitAsync(Id.Value, accountId, cancellationToken);
        if (ok)
        {
            TempData["CorporateNotice"] = "Başvurun onaya gönderildi. Sonuç e-posta ile ve buradan takip edilebilir.";
            return RedirectToPage("/KurumsalHesap/Index");
        }

        FormError = error;
        if (!await LoadAsync(accountId, cancellationToken))
        {
            return NotFound();
        }

        SetAccountMeta("Kurumsal hesap", "Kurumsal hesap bilgileri ve evraklar");
        return Page();
    }

    /// <summary>Evrak indirme — yalnızca sahibi (admin kendi sayfasından indirir).</summary>
    public async Task<IActionResult> OnGetDocumentAsync(long documentId, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        var opened = await corporate.OpenDocumentAsync(documentId, accountId, requesterIsAdmin: false, cancellationToken);
        if (opened is null)
        {
            return NotFound();
        }

        var (content, document) = opened.Value;
        return File(content, document.ContentType, document.FileName);
    }

    private async Task<bool> LoadAsync(long accountId, CancellationToken cancellationToken)
    {
        Cities = await catalog.GetAllCitiesAsync(cancellationToken);

        if (Id is > 0)
        {
            Account = await corporate.GetOwnedAsync(Id.Value, accountId, cancellationToken);
            if (Account is null)
            {
                return false;
            }

            Documents = await corporate.ListDocumentsAsync(Account.Id, cancellationToken);
            Districts = await catalog.GetDistrictsByCityAsync(Account.CityId, cancellationToken);
        }

        return true;
    }

    private bool TryGetAccountId(out long accountId)
    {
        accountId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out accountId) && accountId > 0;
    }
}
