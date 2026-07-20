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
        var primary = command.PrimaryIntent.Trim();
        var intents = new[] { primary };

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

        var isRent = primary == ListingIntent.Kiralik;
        var priceUnit = isRent && !string.IsNullOrWhiteSpace(command.PriceUnit)
            ? command.PriceUnit.Trim()
            : null;

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
            PrimaryIntent = primary,
            Intents = intents,
            Condition = command.Condition.Trim(),
            ModelYear = command.ModelYear,
            Hours = command.Hours is >= 0 ? command.Hours : null,
            Tons = command.Tons,
            CapacityKg = command.CapacityKg is > 0 ? command.CapacityKg : null,
            Horsepower = command.Horsepower is >= 0 ? command.Horsepower : null,
            Price = command.Price,
            RentPrice = null,
            PriceUnit = priceUnit,
            IncludesOperator = command.IncludesOperator && isRent,
            Title = command.Title.Trim(),
            Description = command.Description.Trim(),
            SpecsJson = string.IsNullOrWhiteSpace(command.SpecsJson) ? "{}" : command.SpecsJson.Trim(),
            ImageUrls = images,
            AttachmentIds = attachmentIds
        };
    }
}
