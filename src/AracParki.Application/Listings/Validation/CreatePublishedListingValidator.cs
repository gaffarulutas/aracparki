using System.Text.Json;
using AracParki.Application.Listings.Commands;
using AracParki.Domain.Listings;
using FluentValidation;

namespace AracParki.Application.Listings.Validation;

public sealed class CreatePublishedListingValidator : AbstractValidator<CreatePublishedListingCommand>
{
    public CreatePublishedListingValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.SellerDisplayName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Phone).NotEmpty().MinimumLength(10).MaximumLength(20);

        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.BrandId).GreaterThan(0);
        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(8000);

        RuleFor(x => x.CityId).GreaterThan(0);
        RuleFor(x => x.DistrictId).GreaterThan(0);

        RuleFor(x => x.PrimaryIntent)
            .Must(i => i is ListingIntent.Satilik or ListingIntent.Kiralik);

        RuleFor(x => x.Intents)
            .NotEmpty()
            .Must(intents => intents.All(i => i is ListingIntent.Satilik or ListingIntent.Kiralik))
            .Must((cmd, intents) => intents.Contains(cmd.PrimaryIntent));

        RuleFor(x => x.Condition).Must(EquipmentCondition.Known.Contains);

        RuleFor(x => x.ModelYear).InclusiveBetween(1950, 2100);
        RuleFor(x => x.Hours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Tons).GreaterThan(0);
        RuleFor(x => x.Horsepower).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CapacityKg).GreaterThan(0).When(x => x.CapacityKg.HasValue);

        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.PriceUnit)
            .Must(u => u is null || PriceUnit.Known.Contains(u));
        RuleFor(x => x.PriceUnit)
            .NotEmpty()
            .Must(PriceUnit.Known.Contains!)
            .When(x => x.Intents.Contains(ListingIntent.Kiralik))
            .WithMessage("Kiralık ilanlarda fiyat birimi zorunlu.");

        RuleFor(x => x.SpecsJson)
            .Must(BeJsonObject)
            .WithMessage("Specs must be a JSON object.");

        RuleFor(x => x.ImageUrls)
            .NotEmpty()
            .Must(urls => urls.Count is >= 1 and <= 8)
            .WithMessage("1–8 görsel URL gerekli.");

        RuleForEach(x => x.ImageUrls)
            .Must(BeHttpUrl)
            .WithMessage("Geçerli bir http(s) görsel URL gir.");
    }

    private static bool BeJsonObject(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool BeHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
