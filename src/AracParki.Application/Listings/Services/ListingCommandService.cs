using AracParki.Application.Listings.Commands;
using AracParki.Domain.Listings;
using FluentValidation;

namespace AracParki.Application.Listings.Services;

public sealed class ListingCommandService(
    IListingStore store,
    IValidator<CreatePublishedListingCommand> validator)
{
    public async Task<string> CreatePublishedAsync(
        CreatePublishedListingCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        return await store.CreatePublishedAsync(Normalize(command), cancellationToken);
    }

    private static CreatePublishedListingCommand Normalize(CreatePublishedListingCommand command)
    {
        var intents = command.Intents
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var images = command.ImageUrls
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(ListingImageUrl.MaxCount)
            .ToArray();

        var attachmentIds = command.AttachmentIds
            .Where(id => id > 0)
            .Distinct()
            .Take(20)
            .ToArray();

        var hasSale = intents.Contains(ListingIntent.Satilik, StringComparer.Ordinal);
        var hasRent = intents.Contains(ListingIntent.Kiralik, StringComparer.Ordinal);

        var priceUnit = string.IsNullOrWhiteSpace(command.PriceUnit) ? null : command.PriceUnit.Trim();
        if (!hasRent)
        {
            priceUnit = null;
        }

        decimal price = command.Price;
        decimal? rentPrice = command.RentPrice is > 0 ? command.RentPrice : null;
        if (hasSale && hasRent)
        {
            rentPrice ??= command.RentPrice;
        }
        else if (hasRent && !hasSale)
        {
            // Tek tip kiralık: price alanı kira bedeli; rent_price boş kalır.
            rentPrice = null;
        }
        else
        {
            rentPrice = null;
        }

        return new CreatePublishedListingCommand
        {
            AccountId = command.AccountId,
            SellerDisplayName = command.SellerDisplayName.Trim(),
            Phone = command.Phone.Trim(),
            SellerType = command.SellerType.Trim(),
            CategoryId = command.CategoryId,
            BrandId = command.BrandId,
            ModelId = command.ModelId is > 0 ? command.ModelId : null,
            ModelName = command.ModelName.Trim(),
            SerialNo = string.IsNullOrWhiteSpace(command.SerialNo) ? null : command.SerialNo.Trim(),
            CityId = command.CityId,
            DistrictId = command.DistrictId,
            NeighborhoodId = command.NeighborhoodId is > 0 ? command.NeighborhoodId : null,
            PrimaryIntent = command.PrimaryIntent.Trim(),
            Intents = intents,
            Condition = command.Condition.Trim(),
            ModelYear = command.ModelYear,
            Hours = command.Hours is >= 0 ? command.Hours : null,
            Tons = command.Tons,
            CapacityKg = command.CapacityKg is > 0 ? command.CapacityKg : null,
            Horsepower = command.Horsepower is >= 0 ? command.Horsepower : null,
            Price = price,
            RentPrice = rentPrice,
            PriceUnit = priceUnit,
            IncludesOperator = command.IncludesOperator && hasRent,
            Title = command.Title.Trim(),
            Description = command.Description.Trim(),
            SpecsJson = string.IsNullOrWhiteSpace(command.SpecsJson) ? "{}" : command.SpecsJson.Trim(),
            ImageUrls = images,
            AttachmentIds = attachmentIds
        };
    }
}
