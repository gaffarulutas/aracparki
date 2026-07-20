using AracParki.Application.Listings.Commands;
using AracParki.Domain.Listings;
using FluentValidation;

namespace AracParki.Application.Listings.Validation;

public sealed class CreatePublishedListingValidator : AbstractValidator<CreatePublishedListingCommand>
{
    public CreatePublishedListingValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0).WithMessage("Hesap geçersiz.");
        RuleFor(x => x.SellerDisplayName).NotEmpty().MaximumLength(120)
            .WithMessage("Satıcı adı gerekli.");
        RuleFor(x => x.SellerType).Must(SellerType.Known.Contains)
            .WithMessage("Satıcı tipi geçersiz.");
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon gerekli.")
            .MinimumLength(10).WithMessage("Telefon en az 10 rakam olmalı.")
            .MaximumLength(15).WithMessage("Telefon en fazla 15 rakam olmalı.")
            .Matches(@"^\d+$").WithMessage("Telefon yalnızca rakam içermeli.");

        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Kategori seç.");
        RuleFor(x => x.BrandId).GreaterThan(0).WithMessage("Marka seç.");
        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(120).WithMessage("Model adı gerekli.");
        RuleFor(x => x.SerialNo).MaximumLength(80).When(x => x.SerialNo is not null)
            .WithMessage("Seri no en fazla 80 karakter.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).WithMessage("Başlık gerekli.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(8000).WithMessage("Açıklama gerekli.");

        RuleFor(x => x.CityId).GreaterThan(0).WithMessage("İl seç.");
        RuleFor(x => x.DistrictId).GreaterThan(0).WithMessage("İlçe seç.");
        RuleFor(x => x.NeighborhoodId).GreaterThan(0).When(x => x.NeighborhoodId.HasValue)
            .WithMessage("Mahalle geçersiz.");

        RuleFor(x => x.PrimaryIntent)
            .Must(i => i is ListingIntent.Satilik or ListingIntent.Kiralik)
            .WithMessage("Birincil ilan tipi geçersiz.");

        RuleFor(x => x.Intents)
            .NotEmpty().WithMessage("En az bir ilan tipi seç.")
            .Must(intents => intents.All(i => i is ListingIntent.Satilik or ListingIntent.Kiralik))
            .WithMessage("İlan tipi geçersiz.")
            .Must((cmd, intents) => intents.Contains(cmd.PrimaryIntent))
            .WithMessage("Birincil tip, seçilen tiplerden biri olmalı.");

        RuleFor(x => x.Condition).Must(EquipmentCondition.Known.Contains)
            .WithMessage("Durum geçersiz.");

        RuleFor(x => x.ModelYear).InclusiveBetween(1950, 2100).WithMessage("Model yılı geçersiz.");
        RuleFor(x => x.Hours).GreaterThanOrEqualTo(0).When(x => x.Hours.HasValue)
            .WithMessage("Çalışma saati geçersiz.");
        RuleFor(x => x.Tons).GreaterThan(0).WithMessage("Tonaj / kapasite 0'dan büyük olmalı.");
        RuleFor(x => x.Horsepower).GreaterThanOrEqualTo(0).When(x => x.Horsepower.HasValue)
            .WithMessage("Beygir gücü geçersiz.");
        RuleFor(x => x.CapacityKg).GreaterThan(0).When(x => x.CapacityKg.HasValue)
            .WithMessage("Kapasite (kg) geçersiz.");

        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalı.");
        RuleFor(x => x.RentPrice).GreaterThan(0).When(x => x.RentPrice.HasValue)
            .WithMessage("Kira bedeli geçersiz.");
        RuleFor(x => x.RentPrice)
            .NotNull()
            .When(x => x.Intents.Contains(ListingIntent.Satilik) && x.Intents.Contains(ListingIntent.Kiralik))
            .WithMessage("Satılık + kiralık ilanlarda kira bedeli zorunlu.");
        RuleFor(x => x.PriceUnit)
            .Must(u => u is null || PriceUnit.Known.Contains(u))
            .WithMessage("Fiyat birimi geçersiz.");
        RuleFor(x => x.PriceUnit)
            .NotEmpty()
            .Must(PriceUnit.Known.Contains!)
            .When(x => x.Intents.Contains(ListingIntent.Kiralik))
            .WithMessage("Kiralık ilanlarda fiyat birimi zorunlu.");

        RuleFor(x => x.SpecsJson)
            .Must(BeJsonObject)
            .WithMessage("Özellik verisi geçersiz.")
            .Must(json => System.Text.Encoding.UTF8.GetByteCount(json ?? "") <= SpecsJsonBuilder.MaxJsonBytes)
            .WithMessage("Özellik verisi çok büyük.");

        RuleFor(x => x.ImageUrls)
            .NotEmpty()
            .Must(urls => urls.Count is >= 1 and <= ListingImageUrl.MaxCount)
            .WithMessage($"1–{ListingImageUrl.MaxCount} görsel gerekli.");

        RuleForEach(x => x.ImageUrls)
            .Must(ListingImageUrl.IsAllowed)
            .WithMessage("Görseller https:// URL veya yüklenen dosya olmalı.");

        RuleForEach(x => x.AttachmentIds).GreaterThan(0).WithMessage("Ekipman seçimi geçersiz.");
        RuleFor(x => x.AttachmentIds).Must(ids => ids.Count <= 20)
            .WithMessage("En fazla 20 ekipman seçilebilir.");
    }

    private static bool BeJsonObject(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object;
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }
}
