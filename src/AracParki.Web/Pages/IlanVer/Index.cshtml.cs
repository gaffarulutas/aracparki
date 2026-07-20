using System.Security.Claims;
using System.Text.Json;
using AracParki.Application.Accounts;
using AracParki.Application.Accounts.Services;
using AracParki.Application.Catalog.Dtos;
using AracParki.Application.Catalog.Services;
using AracParki.Application.Listings;
using AracParki.Application.Listings.Commands;
using AracParki.Application.Listings.Services;
using AracParki.Domain.Listings;
using AracParki.Web.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace AracParki.Web.Pages.IlanVer;

[Authorize]
public sealed class IndexModel(
    CatalogService catalog,
    AccountService accounts,
    ListingCommandService listingCommands,
    IListingImageStorage imageStorage,
    IWizardDraftStore wizardDrafts,
    IPhoneOtpService phoneOtp,
    ILogger<IndexModel> logger) : PageModel
{
    private static readonly string[] StepTitles =
    [
        "Kategori seçimi",
        "Araç bilgileri",
        "Fiyat ve konum",
        "Fotoğraflar",
        "Yayına hazır"
    ];

    private static readonly JsonSerializerOptions AttrJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public WizardDraft Draft { get; private set; } = new();
    public int Step { get; private set; } = 1;
    public string StepTitle => StepTitles[Math.Clamp(Step, 1, 5) - 1];
    public string? FormError { get; private set; }
    public IReadOnlyList<string> FormErrors { get; private set; } = [];
    /// <summary>Field key → message for inline validation (WCAG 3.3.1).</summary>
    public IReadOnlyDictionary<string, string> FieldErrors { get; private set; }
        = new Dictionary<string, string>(StringComparer.Ordinal);

    public bool HasFieldError(string key) => FieldErrors.ContainsKey(key);

    public string? FieldError(string key)
        => FieldErrors.TryGetValue(key, out var msg) ? msg : null;

    public string FieldInvalidClass(string key)
        => HasFieldError(key) ? "is-invalid" : "";

    public string? AriaInvalid(string key)
        => HasFieldError(key) ? "true" : null;
    public bool RequirePhoneVerification { get; private set; }
    public bool ShowPhoneGate => RequirePhoneVerification;
    public string? AccountPhone { get; private set; }
    public bool AccountPhoneConfirmed { get; private set; }
    public string? PublishToken { get; private set; }

    public IReadOnlyList<CategoryGroupDto> CategoryGroups { get; private set; } = [];
    public IReadOnlyList<BrandOptionDto> Brands { get; private set; } = [];
    public IReadOnlyList<EquipmentModelOptionDto> Models { get; private set; } = [];
    public IReadOnlyList<CategoryAttributeDto> Attributes { get; private set; } = [];
    public IReadOnlyList<AttachmentOptionDto> Attachments { get; private set; } = [];
    public IReadOnlyList<CityOptionDto> Cities { get; private set; } = [];
    public IReadOnlyList<DistrictOptionDto> Districts { get; private set; } = [];
    public IReadOnlyList<NeighborhoodOptionDto> Neighborhoods { get; private set; } = [];

    public string AttrJson(object value) =>
        JsonSerializer.Serialize(value, AttrJsonOptions);

    private async Task<WizardDraft> LoadDraftAsync(CancellationToken cancellationToken)
    {
        var draft = await WizardDraftStore.LoadAsync(
            HttpContext.Session, wizardDrafts, GetAccountId(), cancellationToken);
        Draft = draft;
        SyncPhoneFromAccount();
        return draft;
    }

    public async Task<IActionResult> OnGetAsync(int? adim, CancellationToken cancellationToken)
    {
        await LoadAccountPhoneAsync(cancellationToken);
        await LoadDraftAsync(cancellationToken);
        Step = ResolveStep(adim, Draft, RequirePhoneVerification);
        await LoadLookupsAsync(cancellationToken);
        PublishToken = EnsurePublishToken();
        SetMeta();
        return Page();
    }

    [EnableRateLimiting("phone-otp")]
    public async Task<IActionResult> OnPostSendPhoneOtpAsync(
        string? phone,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId is null)
        {
            return Challenge();
        }

        await LoadAccountPhoneAsync(cancellationToken);
        if (!RequirePhoneVerification)
        {
            return new JsonResult(new { ok = true, alreadyVerified = true });
        }

        var normalized = AccountService.NormalizePhone(phone)
                         ?? AccountService.NormalizePhone(AccountPhone);
        if (normalized is null)
        {
            return new JsonResult(new { ok = false, error = "Geçerli bir telefon numarası gir (10–15 rakam)." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var (ok, error, devCode) = await phoneOtp.SendAsync(accountId.Value, normalized, cancellationToken);
        if (!ok)
        {
            return new JsonResult(new { ok = false, error = error ?? "Doğrulama kodu gönderilemedi." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        await LoadDraftAsync(cancellationToken);
        Draft.Phone = normalized;
        await PersistDraftAsync(cancellationToken);

        return new JsonResult(new
        {
            ok = true,
            message = "Kod WhatsApp’a gönderildi.",
            maskedPhone = AccountService.MaskPhone(normalized),
            devCode
        });
    }

    [EnableRateLimiting("phone-otp")]
    public async Task<IActionResult> OnPostVerifyPhoneOtpAsync(
        string? phone,
        string? otpCode,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId is null)
        {
            return Challenge();
        }

        await LoadAccountPhoneAsync(cancellationToken);
        if (!RequirePhoneVerification)
        {
            return new JsonResult(new { ok = true, alreadyVerified = true, reload = true });
        }

        var normalized = AccountService.NormalizePhone(phone)
                         ?? AccountService.NormalizePhone(AccountPhone);
        if (normalized is null)
        {
            await LoadDraftAsync(cancellationToken);
            normalized = AccountService.NormalizePhone(Draft.Phone);
        }

        if (normalized is null)
        {
            return new JsonResult(new { ok = false, error = "Geçerli bir telefon numarası gir." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        if (string.IsNullOrWhiteSpace(otpCode))
        {
            return new JsonResult(new { ok = false, error = "Doğrulama kodunu gir." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var (ok, error) = await phoneOtp.VerifyAsync(accountId.Value, normalized, otpCode, cancellationToken);
        if (!ok)
        {
            return new JsonResult(new { ok = false, error = error ?? "Doğrulama başarısız." })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        await LoadDraftAsync(cancellationToken);
        Draft.Phone = normalized;
        Draft.PhoneVerified = true;
        await PersistDraftAsync(cancellationToken);
        await RefreshAuthCookieAsync(accountId.Value, cancellationToken);

        return new JsonResult(new { ok = true, reload = true });
    }

    public async Task<IActionResult> OnPostCascadeAsync(
        string? intent,
        int categoryId,
        int brandId,
        int? modelId,
        string? modelName,
        int modelYear,
        int? groupId,
        string? groupName,
        CancellationToken cancellationToken)
    {
        if (await RejectIfPhoneGateAsync(cancellationToken) is { } gated)
        {
            return gated;
        }

        await LoadDraftAsync(cancellationToken);

        if (intent is not (ListingIntent.Satilik or ListingIntent.Kiralik))
        {
            return await FailAsync(1, "İlan tipi seç (Satılık veya Kiralık).", cancellationToken);
        }

        var categories = await catalog.GetAllCategoriesAsync(cancellationToken);
        var category = categories.FirstOrDefault(c => c.Id == categoryId);
        if (category is null)
        {
            return await FailAsync(1, "Kategori seçimi geçersiz.", cancellationToken);
        }

        var brands = await catalog.GetBrandsByCategoryAsync(categoryId, cancellationToken);
        var brand = brands.FirstOrDefault(b => b.Id == brandId);
        if (brand is null)
        {
            return await FailAsync(1, "Marka seçimi geçersiz — cascader’da markayı seç.", cancellationToken);
        }

        string resolvedModelName = modelName?.Trim() ?? "";
        int? resolvedModelId = modelId is > 0 ? modelId : null;
        EquipmentModelOptionDto? catalogModel = null;
        if (resolvedModelId is not null)
        {
            var models = await catalog.GetModelsByBrandCategoryAsync(brandId, categoryId, cancellationToken);
            catalogModel = models.FirstOrDefault(m => m.Id == resolvedModelId);
            if (catalogModel is null)
            {
                return await FailAsync(1, "Model seçimi geçersiz.", cancellationToken);
            }

            resolvedModelName = catalogModel.Name;
        }

        if (string.IsNullOrWhiteSpace(resolvedModelName))
        {
            return await FailAsync(1, "Model seçimi zorunlu.", cancellationToken);
        }

        if (modelYear is < 1950 or > 2100)
        {
            return await FailAsync(1, "Model yılı geçersiz.", cancellationToken);
        }

        var categoryChanged = Draft.CategoryId != categoryId;
        if (categoryChanged)
        {
            Draft.Specs = new Dictionary<string, string>(StringComparer.Ordinal);
            Draft.SpecsJson = "{}";
            Draft.AttachmentIds = [];
            Draft.CapacityKg = null;
            Draft.Title = "";
            Draft.Description = "";
        }

        var intentChanged = Draft.PrimaryIntent != intent;
        Draft.PrimaryIntent = intent;
        Draft.Intents = [intent];
        if (intentChanged)
        {
            Draft.RentPrice = null;
            if (intent == ListingIntent.Satilik)
            {
                Draft.PriceUnit = null;
                Draft.IncludesOperator = false;
            }
        }

        Draft.GroupId = groupId is > 0 ? groupId.Value : category.GroupId ?? 0;
        Draft.GroupName = string.IsNullOrWhiteSpace(groupName) ? null : groupName.Trim();
        Draft.CategoryId = category.Id;
        Draft.CategoryName = category.Name;
        Draft.CapacityMetric = category.CapacityMetric;
        Draft.BrandId = brand.Id;
        Draft.BrandName = brand.Name;
        Draft.ModelId = resolvedModelId;
        Draft.ModelName = resolvedModelName;
        Draft.ModelYear = modelYear;

        if (catalogModel is not null)
        {
            var attrs = await catalog.GetCategoryAttributesAsync(category.Id, cancellationToken);
            CatalogModelDefaults.Apply(Draft, catalogModel, category.CapacityMetric, attrs);
        }
        else
        {
            CatalogModelDefaults.ClearCatalogLocks(Draft);
        }

        Draft.Step = 2;
        await PersistDraftAsync(cancellationToken);
        return RedirectToPage(new { adim = 2 });
    }

    public async Task<IActionResult> OnPostMachineAsync(
        string? condition,
        int? hours,
        bool hoursUnknown,
        decimal tons,
        int? capacityKg,
        int? horsepower,
        bool horsepowerUnknown,
        string? serialNo,
        string? title,
        string? description,
        [FromForm] Dictionary<string, string>? specs,
        int[]? attachmentIds,
        CancellationToken cancellationToken)
    {
        if (await RejectIfPhoneGateAsync(cancellationToken) is { } gated)
        {
            return gated;
        }

        await LoadDraftAsync(cancellationToken);
        if (!Draft.HasCategory || !Draft.HasIntent)
        {
            return RedirectToPage(new { adim = 1 });
        }

        if (string.IsNullOrWhiteSpace(condition) || !EquipmentCondition.Known.Contains(condition))
        {
            return await FailAsync(2, "Durum seçimi geçersiz.", cancellationToken);
        }

        var categoryAttrs = await catalog.GetCategoryAttributesAsync(Draft.CategoryId, cancellationToken);
        // Catalog-sourced keys stay authoritative; form may only fill gaps / non-catalog attrs.
        var catalogLockedKeys = CatalogModelDefaults.LockedSpecKeys(Draft);
        var formSpecs = specs is null
            ? null
            : specs
                .Where(kv => !catalogLockedKeys.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        var mergedForBuild = new Dictionary<string, string>(Draft.Specs, StringComparer.Ordinal);
        if (formSpecs is not null)
        {
            foreach (var (k, v) in formSpecs)
            {
                if (!string.IsNullOrWhiteSpace(v))
                {
                    mergedForBuild[k] = v;
                }
            }
        }

        var (specsOk, specsError, specsJson, storedRaw) = SpecsJsonBuilder.TryBuild(mergedForBuild, categoryAttrs);
        if (!specsOk)
        {
            var specGaps = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var attr in categoryAttrs.Where(a => a.IsRequired))
            {
                if (!mergedForBuild.TryGetValue(attr.Key, out var raw) || string.IsNullOrWhiteSpace(raw))
                {
                    specGaps["spec:" + attr.Key] = $"{attr.Label} zorunlu.";
                }
            }

            if (specGaps.Count == 0 && !string.IsNullOrWhiteSpace(specsError))
            {
                specGaps["specs"] = specsError!;
            }

            return await FailAsync(
                2,
                specsError ?? "Özellikler geçersiz.",
                cancellationToken,
                specGaps);
        }

        var allowedAttachments = await catalog.GetAttachmentsByCategoryAsync(Draft.CategoryId, cancellationToken);
        var allowedIds = allowedAttachments.Select(a => a.Id).ToHashSet();
        var selectedAttachments = (attachmentIds ?? [])
            .Where(id => allowedIds.Contains(id))
            .Distinct()
            .Take(20)
            .ToList();

        Draft.Condition = condition;
        Draft.HoursUnknown = hoursUnknown;
        Draft.Hours = hoursUnknown ? null : hours;

        if (!Draft.TonsFromCatalog)
        {
            Draft.Tons = tons;
        }

        if (Draft.CapacityMetric == "capacity_kg")
        {
            if (!Draft.CapacityKgFromCatalog)
            {
                Draft.CapacityKg = capacityKg;
            }
        }
        else
        {
            Draft.CapacityKg = Draft.CapacityKgFromCatalog ? Draft.CapacityKg : null;
        }

        if (!Draft.HorsepowerFromCatalog)
        {
            Draft.HorsepowerUnknown = horsepowerUnknown;
            Draft.Horsepower = horsepowerUnknown ? null : horsepower;
        }
        else
        {
            Draft.HorsepowerUnknown = false;
        }

        Draft.SerialNo = string.IsNullOrWhiteSpace(serialNo) ? null : serialNo.Trim();
        Draft.Title = string.IsNullOrWhiteSpace(title) ? Draft.SuggestedTitle() : title.Trim();
        Draft.Description = description?.Trim() ?? "";
        Draft.Specs = storedRaw;
        Draft.SpecsJson = specsJson;
        Draft.AttachmentIds = selectedAttachments;

        if (!Draft.HasMachine)
        {
            var gaps = CollectMachineFieldErrors(Draft);
            return await FailAsync(
                2,
                gaps.Count > 0
                    ? "Zorunlu alanları tamamla — eksikler aşağıda işaretlendi."
                    : "Makine bilgilerini kontrol et — zorunlu alanlar eksik.",
                cancellationToken,
                gaps);
        }

        if (Draft.CapacityMetric == "capacity_kg" && Draft.CapacityKg is not > 0)
        {
            return await FailAsync(
                2,
                "Kapasite (kg) zorunlu.",
                cancellationToken,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["capacityKg"] = "Kapasite (kg) gir."
                });
        }

        Draft.Step = 3;
        await PersistDraftAsync(cancellationToken);
        return RedirectToPage(new { adim = 3 });
    }

    public async Task<IActionResult> OnPostSaleAsync(
        decimal price,
        string? priceUnit,
        bool includesOperator,
        string? sellerType,
        int cityId,
        int districtId,
        int? neighborhoodId,
        CancellationToken cancellationToken)
    {
        if (await RejectIfPhoneGateAsync(cancellationToken) is { } gated)
        {
            return gated;
        }

        await LoadDraftAsync(cancellationToken);
        if (!Draft.HasMachine || !Draft.HasIntent)
        {
            return RedirectToPage(new { adim = Draft.HasCategory && Draft.HasIntent ? 2 : 1 });
        }

        if (string.IsNullOrWhiteSpace(sellerType) || !SellerType.Known.Contains(sellerType))
        {
            return await FailAsync(3, "Satıcı tipi seç (Sahibi / Bayi).", cancellationToken);
        }

        var cities = await catalog.GetAllCitiesAsync(cancellationToken);
        var city = cities.FirstOrDefault(c => c.Id == cityId);
        DistrictOptionDto? district = null;
        NeighborhoodOptionDto? neighborhood = null;
        if (city is not null)
        {
            var districts = await catalog.GetDistrictsByCityAsync(cityId, cancellationToken);
            district = districts.FirstOrDefault(d => d.Id == districtId);
            if (district is not null && neighborhoodId is > 0)
            {
                var neighborhoods = await catalog.GetNeighborhoodsByDistrictAsync(districtId, cancellationToken);
                neighborhood = neighborhoods.FirstOrDefault(n => n.Id == neighborhoodId);
            }
        }

        var isRent = Draft.PrimaryIntent == ListingIntent.Kiralik;
        Draft.Intents = [Draft.PrimaryIntent];
        Draft.Price = price;
        Draft.RentPrice = null;
        Draft.PriceUnit = isRent && !string.IsNullOrWhiteSpace(priceUnit) ? priceUnit.Trim() : null;
        Draft.IncludesOperator = includesOperator && isRent;
        Draft.SellerType = sellerType;
        Draft.CityId = city?.Id ?? 0;
        Draft.CityName = city?.Name;
        Draft.DistrictId = district?.Id ?? 0;
        Draft.DistrictName = district?.Name;
        Draft.NeighborhoodId = neighborhood?.Id;
        Draft.NeighborhoodName = neighborhood?.DisplayName ?? neighborhood?.Name;
        Draft.Phone = AccountService.NormalizePhone(AccountPhone) ?? Draft.Phone;
        Draft.PhoneVerified = true;

        if (city is null || district is null)
        {
            return await FailAsync(3, "İl ve ilçe seçimi zorunlu.", cancellationToken);
        }

        if (!Draft.HasSaleInfo(RequirePhoneVerification))
        {
            var gaps = CollectSaleFieldErrors(Draft, RequirePhoneVerification);
            return await FailAsync(
                3,
                gaps.Count > 0
                    ? "Zorunlu alanları tamamla — eksikler aşağıda işaretlendi."
                    : "Satış bilgilerini kontrol et.",
                cancellationToken,
                gaps);
        }

        Draft.Step = 4;
        await PersistDraftAsync(cancellationToken);
        return RedirectToPage(new { adim = 4 });
    }

    public async Task<IActionResult> OnPostImagesAsync(string[]? imageUrls, CancellationToken cancellationToken)
    {
        if (await RejectIfPhoneGateAsync(cancellationToken) is { } gated)
        {
            return gated;
        }

        await LoadDraftAsync(cancellationToken);
        if (!Draft.HasSaleInfo(RequirePhoneVerification))
        {
            return RedirectToPage(new { adim = 3 });
        }

        var submitted = (imageUrls ?? [])
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .ToList();

        if (submitted.Count > ListingImageUrl.MaxCount)
        {
            return await FailAsync(4, $"En fazla {ListingImageUrl.MaxCount} görsel ekleyebilirsin.", cancellationToken);
        }

        Draft.ImageUrls = submitted
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(ListingImageUrl.MaxCount)
            .ToList();

        if (Draft.ImageUrls.Count == 0)
        {
            return await FailAsync(4, "En az 1 görsel yükle veya URL gir.", cancellationToken);
        }

        if (Draft.ImageUrls.Any(u => !ListingImageUrl.IsAllowed(u)))
        {
            return await FailAsync(4, "Görseller yüklenen dosya veya https:// URL olmalı.", cancellationToken);
        }

        Draft.Step = 5;
        await PersistDraftAsync(cancellationToken);
        return RedirectToPage(new { adim = 5 });
    }

    public async Task<IActionResult> OnPostUploadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (await RejectIfPhoneGateAsync(cancellationToken) is { } gated)
        {
            return gated;
        }

        await LoadDraftAsync(cancellationToken);
        var accountId = GetAccountId();
        if (accountId is null)
        {
            return Challenge();
        }

        if (!Draft.HasSaleInfo(RequirePhoneVerification))
        {
            return RedirectToPage(new { adim = 3 });
        }

        if (file is null || file.Length == 0)
        {
            return await FailAsync(4, "Dosya seçilmedi.", cancellationToken);
        }

        if (file.Length > ListingImageUrl.MaxUploadBytes)
        {
            return await FailAsync(4, "Görsel en fazla 5 MB olabilir.", cancellationToken);
        }

        if (!ListingImageUrl.IsAllowedContentType(file.ContentType))
        {
            return await FailAsync(4, "Yalnızca JPEG, PNG, WebP veya GIF yükleyebilirsin.", cancellationToken);
        }

        if (Draft.ImageUrls.Count >= ListingImageUrl.MaxCount)
        {
            return await FailAsync(4, $"En fazla {ListingImageUrl.MaxCount} görsel ekleyebilirsin.", cancellationToken);
        }

        await using var stream = file.OpenReadStream();
        var url = await imageStorage.SaveAsync(accountId.Value, stream, file.ContentType, cancellationToken);
        Draft.ImageUrls.Add(url);
        Draft.Step = 4;
        await PersistDraftAsync(cancellationToken);
        return RedirectToPage(new { adim = 4 });
    }

    [EnableRateLimiting("listing-publish")]
    public async Task<IActionResult> OnPostPublishAsync(string? publishToken, CancellationToken cancellationToken)
    {
        if (await RejectIfPhoneGateAsync(cancellationToken) is { } gated)
        {
            return gated;
        }

        await LoadDraftAsync(cancellationToken);
        Step = 5;

        var expected = HttpContext.Session.GetString("ilan-ver-publish-token");
        if (string.IsNullOrWhiteSpace(publishToken)
            || string.IsNullOrWhiteSpace(expected)
            || !CryptographicOperationsEquals(expected, publishToken))
        {
            PublishToken = EnsurePublishToken();
            return await FailAsync(5, "Yayın isteği geçersiz veya tekrarlandı. Tekrar dene.", cancellationToken);
        }

        HttpContext.Session.Remove("ilan-ver-publish-token");

        if (!Draft.HasCategory || !Draft.HasMachine || !Draft.HasSaleInfo(RequirePhoneVerification) || !Draft.HasImages)
        {
            return await FailAsync(5, "İlan henüz yayınlanmaya hazır değil. Adımları tamamla.", cancellationToken);
        }

        var accountId = GetAccountId();
        if (accountId is null)
        {
            return Challenge();
        }

        var account = await accounts.GetByIdAsync(accountId.Value, cancellationToken);
        if (account is null)
        {
            return Challenge();
        }

        var phone = AccountService.NormalizePhone(account.Phone ?? Draft.Phone);
        if (phone is null || (!account.PhoneConfirmed && !Draft.PhoneVerified))
        {
            return await FailAsync(5, "Telefon doğrulanmadan yayınlanamaz. Satış adımına dön.", cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(account.Phone) || !account.PhoneConfirmed)
        {
            var (ok, error, updated) = await accounts.UpdatePhoneAsync(accountId.Value, phone, cancellationToken);
            if (!ok || updated is null)
            {
                return await FailAsync(5, error ?? "Telefon güncellenemedi.", cancellationToken);
            }

            await RefreshAuthCookieAsync(accountId.Value, cancellationToken);
            account = await accounts.GetByIdAsync(accountId.Value, cancellationToken) ?? updated;
        }

        var command = new CreatePublishedListingCommand
        {
            AccountId = account.Id,
            SellerDisplayName = account.DisplayName,
            Phone = phone,
            SellerType = Draft.SellerType,
            CategoryId = Draft.CategoryId,
            BrandId = Draft.BrandId,
            ModelId = Draft.ModelId,
            ModelName = Draft.ModelName,
            SerialNo = Draft.SerialNo,
            CityId = Draft.CityId,
            DistrictId = Draft.DistrictId,
            NeighborhoodId = Draft.NeighborhoodId,
            PrimaryIntent = Draft.PrimaryIntent,
            Intents = [Draft.PrimaryIntent],
            Condition = Draft.Condition,
            ModelYear = Draft.ModelYear,
            Hours = Draft.HoursUnknown ? null : Draft.Hours,
            Tons = Draft.Tons,
            CapacityKg = Draft.CapacityKg,
            Horsepower = Draft.HorsepowerUnknown ? null : Draft.Horsepower,
            Price = Draft.Price,
            RentPrice = null,
            PriceUnit = Draft.PriceUnit,
            IncludesOperator = Draft.IncludesOperator,
            Title = Draft.Title,
            Description = Draft.Description,
            SpecsJson = string.IsNullOrWhiteSpace(Draft.SpecsJson) ? "{}" : Draft.SpecsJson,
            ImageUrls = Draft.ImageUrls,
            AttachmentIds = Draft.AttachmentIds
        };

        try
        {
            var adNo = await listingCommands.CreatePublishedAsync(command, cancellationToken);
            await WizardDraftStore.ClearAllAsync(HttpContext.Session, wizardDrafts, accountId, cancellationToken);
            return Redirect(ListingRoutes.Detail(adNo));
        }
        catch (ValidationException ex)
        {
            FormErrors = ex.Errors.Select(e => e.ErrorMessage).Distinct().ToArray();
            FormError = FormErrors.FirstOrDefault() ?? "İlan doğrulanamadı.";
            logger.LogWarning(ex, "Listing publish validation failed");
            await LoadLookupsAsync(cancellationToken);
            PublishToken = EnsurePublishToken();
            SetMeta();
            return Page();
        }
        catch (Exception ex)
        {
            FormError = "İlan yayınlanırken bir hata oluştu. Lütfen tekrar dene.";
            logger.LogError(ex, "Listing publish failed for account {AccountId}", account.Id);
            await LoadLookupsAsync(cancellationToken);
            PublishToken = EnsurePublishToken();
            SetMeta();
            return Page();
        }
    }

    public IReadOnlyList<string> EnumOptions(CategoryAttributeDto attr)
    {
        if (string.IsNullOrWhiteSpace(attr.EnumOptionsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(attr.EnumOptionsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public bool HasAttachment(int id) => Draft.AttachmentIds.Contains(id);

    private async Task<IActionResult?> RejectIfPhoneGateAsync(CancellationToken cancellationToken)
    {
        await LoadAccountPhoneAsync(cancellationToken);
        if (!RequirePhoneVerification)
        {
            return null;
        }

        await LoadDraftAsync(cancellationToken);
        FormError = "İlan vermek için önce telefonunu doğrula.";
        FormErrors = [FormError];
        Step = Math.Clamp(Draft.Step is >= 1 and <= 5 ? Draft.Step : 1, 1, 5);
        await LoadLookupsAsync(cancellationToken);
        PublishToken = EnsurePublishToken();
        SetMeta();
        return Page();
    }

    private async Task<IActionResult> FailAsync(
        int step,
        string message,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string>? fieldErrors = null)
    {
        FormError = message;
        FieldErrors = fieldErrors is { Count: > 0 }
            ? new Dictionary<string, string>(fieldErrors, StringComparer.Ordinal)
            : new Dictionary<string, string>(StringComparer.Ordinal);
        FormErrors = FieldErrors.Count > 0
            ? FieldErrors.Values.Distinct().ToArray()
            : [message];
        Step = step;
        await LoadLookupsAsync(cancellationToken);
        PublishToken = EnsurePublishToken();
        SetMeta();
        return Page();
    }

    /// <summary>
    /// Maps HasMachine gaps to field keys for inline errors (Deque / WCAG 3.3.1).
    /// </summary>
    private static Dictionary<string, string> CollectMachineFieldErrors(WizardDraft draft)
    {
        var gaps = new Dictionary<string, string>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(draft.Condition) || !EquipmentCondition.Known.Contains(draft.Condition))
        {
            gaps["condition"] = "Durum seç (İkinci el / Sıfır).";
        }

        if (!draft.HoursUnknown && draft.Hours is null)
        {
            gaps["hours"] = "Çalışma saati gir veya “Bilmiyorum” seç.";
        }

        if (!draft.HorsepowerFromCatalog && !draft.HorsepowerUnknown && draft.Horsepower is null)
        {
            gaps["horsepower"] = "Beygir (HP) gir veya “Bilmiyorum” seç.";
        }

        if (!draft.TonsFromCatalog && draft.Tons <= 0)
        {
            gaps["tons"] = draft.CapacityMetric == "capacity_t"
                ? "Kapasite (ton) gir."
                : "Tonaj gir.";
        }

        if (draft.CapacityMetric == "capacity_kg" && draft.CapacityKg is not > 0)
        {
            gaps["capacityKg"] = "Kapasite (kg) gir.";
        }

        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            gaps["title"] = "İlan başlığı zorunlu.";
        }

        if (string.IsNullOrWhiteSpace(draft.Description))
        {
            gaps["description"] = "Açıklama zorunlu.";
        }

        return gaps;
    }

    private static Dictionary<string, string> CollectSaleFieldErrors(WizardDraft draft, bool requirePhone)
    {
        var gaps = new Dictionary<string, string>(StringComparer.Ordinal);

        if (draft.Price <= 0)
        {
            gaps["price"] = draft.PrimaryIntent == ListingIntent.Kiralik
                ? "Kira bedeli gir."
                : "Satış fiyatı gir.";
        }

        if (draft.PrimaryIntent == ListingIntent.Kiralik
            && (string.IsNullOrWhiteSpace(draft.PriceUnit) || !PriceUnit.Known.Contains(draft.PriceUnit)))
        {
            gaps["priceUnit"] = "Kira birimi seç.";
        }

        if (string.IsNullOrWhiteSpace(draft.SellerType) || !SellerType.Known.Contains(draft.SellerType))
        {
            gaps["sellerType"] = "Satıcı tipi seç.";
        }

        if (draft.CityId <= 0)
        {
            gaps["cityId"] = "İl seç.";
        }

        if (draft.DistrictId <= 0)
        {
            gaps["districtId"] = "İlçe seç.";
        }

        if (requirePhone
            && (AccountService.NormalizePhone(draft.Phone) is null || !draft.PhoneVerified))
        {
            gaps["phone"] = "Telefon doğrulaması gerekli.";
        }

        return gaps;
    }

    private async Task PersistDraftAsync(CancellationToken cancellationToken)
        => await WizardDraftStore.PersistAsync(
            HttpContext.Session,
            wizardDrafts,
            GetAccountId(),
            Draft,
            cancellationToken);

    private async Task LoadAccountPhoneAsync(CancellationToken cancellationToken)
    {
        var id = GetAccountId();
        if (id is null)
        {
            RequirePhoneVerification = true;
            AccountPhoneConfirmed = false;
            return;
        }

        var account = await accounts.GetByIdAsync(id.Value, cancellationToken);
        AccountPhone = account?.Phone;
        AccountPhoneConfirmed = account?.PhoneConfirmed == true;
        RequirePhoneVerification = !AccountPhoneConfirmed;
    }

    private void SyncPhoneFromAccount()
    {
        if (!AccountPhoneConfirmed)
        {
            return;
        }

        Draft.PhoneVerified = true;
        var normalized = AccountService.NormalizePhone(AccountPhone);
        if (normalized is not null)
        {
            Draft.Phone = normalized;
        }
    }

    private async Task RefreshAuthCookieAsync(long accountId, CancellationToken cancellationToken)
    {
        var updated = await accounts.GetByIdAsync(accountId, cancellationToken);
        if (updated is null)
        {
            return;
        }

        var existing = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var props = existing.Properties ?? new AuthenticationProperties();
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthCookie.CreatePrincipal(updated),
            props);
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        if (Step == 1)
        {
            CategoryGroups = await catalog.GetCategoryGroupsAsync(cancellationToken);
            return;
        }

        if (Step == 2 && Draft.CategoryId > 0)
        {
            Attributes = await catalog.GetCategoryAttributesAsync(Draft.CategoryId, cancellationToken);
            Attachments = await catalog.GetAttachmentsByCategoryAsync(Draft.CategoryId, cancellationToken);
            return;
        }

        if (Step == 3)
        {
            Cities = await catalog.GetAllCitiesAsync(cancellationToken);
            if (Draft.CityId > 0)
            {
                Districts = await catalog.GetDistrictsByCityAsync(Draft.CityId, cancellationToken);
            }

            if (Draft.DistrictId > 0)
            {
                Neighborhoods = await catalog.GetNeighborhoodsByDistrictAsync(Draft.DistrictId, cancellationToken);
            }
        }
    }

    private static int ResolveStep(int? adim, WizardDraft draft, bool requirePhoneVerification)
    {
        var requested = adim is >= 1 and <= 5 ? adim.Value : Math.Clamp(draft.Step, 1, 5);

        if (requested >= 2 && (!draft.HasCategory || !draft.HasIntent))
        {
            return 1;
        }

        if (requested >= 3 && !draft.HasMachine)
        {
            return 2;
        }

        if (requested >= 4 && !draft.HasSaleInfo(requirePhoneVerification))
        {
            return 3;
        }

        if (requested >= 5 && !draft.HasImages)
        {
            return 4;
        }

        return requested;
    }

    private string EnsurePublishToken()
    {
        var existing = HttpContext.Session.GetString("ilan-ver-publish-token");
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var token = WebEncoders.Base64UrlEncode(Guid.NewGuid().ToByteArray());
        HttpContext.Session.SetString("ilan-ver-publish-token", token);
        return token;
    }

    private static bool CryptographicOperationsEquals(string a, string b)
    {
        var ba = System.Text.Encoding.UTF8.GetBytes(a);
        var bb = System.Text.Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length
               && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(ba, bb);
    }

    private long? GetAccountId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out var id) ? id : null;
    }

    private void SetMeta()
    {
        ViewData["PageKey"] = "wizard";
        ViewData["Title"] = $"Ücretsiz İlan Ver · {StepTitle} | Araç Parkı";
        ViewData["Description"] = "Makineni Araç Parkı’nda ücretsiz yayınla — satılık veya kiralık.";
        ViewData["Robots"] = "noindex, nofollow";
    }
}
