using AracParki.Application.Listings;
using AracParki.Application.Listings.Commands;
using AracParki.Application.Listings.Validation;
using AracParki.Domain.Listings;
using FluentValidation.TestHelper;

namespace AracParki.UnitTests;

public sealed class CreatePublishedListingValidatorTests
{
    private readonly CreatePublishedListingValidator _validator = new();

    private static CreatePublishedListingCommand ValidCommand() => new()
    {
        AccountId = 1,
        SellerDisplayName = "Test User",
        Phone = "905551112233",
        SellerType = SellerType.Owner,
        CategoryId = 1,
        BrandId = 1,
        ModelName = "320D",
        CityId = 34,
        DistrictId = 1,
        PrimaryIntent = ListingIntent.Satilik,
        Intents = [ListingIntent.Satilik],
        Condition = EquipmentCondition.Used,
        ModelYear = 2018,
        Hours = 4500,
        Tons = 22.5m,
        Horsepower = 150,
        Price = 1_250_000m,
        Title = "CAT 320D",
        Description = "Bakımlı makine.",
        SpecsJson = "{}",
        ImageUrls = ["/uploads/listings/1/abc.jpg"]
    };

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Rejects_dual_intent()
    {
        var cmd = ValidCommand();
        cmd = new CreatePublishedListingCommand
        {
            AccountId = cmd.AccountId,
            SellerDisplayName = cmd.SellerDisplayName,
            Phone = cmd.Phone,
            SellerType = cmd.SellerType,
            CategoryId = cmd.CategoryId,
            BrandId = cmd.BrandId,
            ModelName = cmd.ModelName,
            CityId = cmd.CityId,
            DistrictId = cmd.DistrictId,
            PrimaryIntent = ListingIntent.Satilik,
            Intents = [ListingIntent.Satilik, ListingIntent.Kiralik],
            Condition = cmd.Condition,
            ModelYear = cmd.ModelYear,
            Hours = cmd.Hours,
            Tons = cmd.Tons,
            Horsepower = cmd.Horsepower,
            Price = cmd.Price,
            PriceUnit = PriceUnit.Day,
            Title = cmd.Title,
            Description = cmd.Description,
            SpecsJson = cmd.SpecsJson,
            ImageUrls = cmd.ImageUrls
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Intents);
    }

    [Fact]
    public void Rent_requires_price_unit()
    {
        var cmd = ValidCommand();
        cmd = new CreatePublishedListingCommand
        {
            AccountId = cmd.AccountId,
            SellerDisplayName = cmd.SellerDisplayName,
            Phone = cmd.Phone,
            SellerType = cmd.SellerType,
            CategoryId = cmd.CategoryId,
            BrandId = cmd.BrandId,
            ModelName = cmd.ModelName,
            CityId = cmd.CityId,
            DistrictId = cmd.DistrictId,
            PrimaryIntent = ListingIntent.Kiralik,
            Intents = [ListingIntent.Kiralik],
            Condition = cmd.Condition,
            ModelYear = cmd.ModelYear,
            Hours = cmd.Hours,
            Tons = cmd.Tons,
            Horsepower = cmd.Horsepower,
            Price = cmd.Price,
            PriceUnit = null,
            Title = cmd.Title,
            Description = cmd.Description,
            SpecsJson = cmd.SpecsJson,
            ImageUrls = cmd.ImageUrls
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PriceUnit);
    }

    [Fact]
    public void Rejects_http_image_url()
    {
        var cmd = ValidCommand();
        cmd = new CreatePublishedListingCommand
        {
            AccountId = cmd.AccountId,
            SellerDisplayName = cmd.SellerDisplayName,
            Phone = cmd.Phone,
            SellerType = cmd.SellerType,
            CategoryId = cmd.CategoryId,
            BrandId = cmd.BrandId,
            ModelName = cmd.ModelName,
            CityId = cmd.CityId,
            DistrictId = cmd.DistrictId,
            PrimaryIntent = cmd.PrimaryIntent,
            Intents = cmd.Intents,
            Condition = cmd.Condition,
            ModelYear = cmd.ModelYear,
            Hours = cmd.Hours,
            Tons = cmd.Tons,
            Horsepower = cmd.Horsepower,
            Price = cmd.Price,
            Title = cmd.Title,
            Description = cmd.Description,
            SpecsJson = cmd.SpecsJson,
            ImageUrls = ["http://evil.example/a.jpg"]
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor("ImageUrls[0]");
    }

    [Fact]
    public void Rejects_localhost_https_image()
    {
        Assert.False(ListingImageUrl.IsAllowed("https://127.0.0.1/x.jpg"));
        Assert.False(ListingImageUrl.IsAllowed("https://localhost/x.jpg"));
        Assert.True(ListingImageUrl.IsAllowed("/uploads/listings/1/a.jpg"));
        Assert.True(ListingImageUrl.IsAllowed("https://cdn.example.com/a.jpg"));
    }

    [Fact]
    public void Hours_null_allowed()
    {
        var cmd = ValidCommand();
        cmd = new CreatePublishedListingCommand
        {
            AccountId = cmd.AccountId,
            SellerDisplayName = cmd.SellerDisplayName,
            Phone = cmd.Phone,
            SellerType = cmd.SellerType,
            CategoryId = cmd.CategoryId,
            BrandId = cmd.BrandId,
            ModelName = cmd.ModelName,
            CityId = cmd.CityId,
            DistrictId = cmd.DistrictId,
            PrimaryIntent = cmd.PrimaryIntent,
            Intents = cmd.Intents,
            Condition = cmd.Condition,
            ModelYear = cmd.ModelYear,
            Hours = null,
            Tons = cmd.Tons,
            Horsepower = null,
            Price = cmd.Price,
            Title = cmd.Title,
            Description = cmd.Description,
            SpecsJson = cmd.SpecsJson,
            ImageUrls = cmd.ImageUrls
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Hours);
        result.ShouldNotHaveValidationErrorFor(x => x.Horsepower);
    }
}
