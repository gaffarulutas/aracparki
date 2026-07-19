using System.Text.Json;
using AracParki.Domain.Listings;
using FluentValidation;

namespace AracParki.Application.Listings.Validation;

public sealed class ListingSearchQueryValidator : AbstractValidator<Queries.ListingSearchQuery>
{
    public ListingSearchQueryValidator()
    {
        RuleFor(x => x.Intent)
            .Must(ListingIntent.Known.Contains)
            .WithMessage("Invalid listing intent.");

        RuleFor(x => x.Sort)
            .Must(ListingSort.Known.Contains)
            .WithMessage("Invalid sort.");

        RuleFor(x => x.Condition)
            .Must(c => c is null || EquipmentCondition.Known.Contains(c))
            .WithMessage("Invalid condition.");

        RuleFor(x => x.SellerType)
            .Must(s => s is null || SellerType.Known.Contains(s))
            .WithMessage("Invalid seller type.");

        RuleFor(x => x.PriceUnit)
            .Must(u => u is null || PriceUnit.Known.Contains(u))
            .WithMessage("Invalid price unit.");

        RuleFor(x => x.Query)
            .Must(q => q is null || q.Length >= 2)
            .WithMessage("Query must be at least 2 characters.");

        RuleFor(x => x.Page).InclusiveBetween(1, 500);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.YearMin).GreaterThanOrEqualTo(1970).When(x => x.YearMin.HasValue);
        RuleFor(x => x.YearMax).LessThanOrEqualTo(2100).When(x => x.YearMax.HasValue);
        RuleFor(x => x.HoursMin).GreaterThanOrEqualTo(0).When(x => x.HoursMin.HasValue);
        RuleFor(x => x.WeightMin).GreaterThanOrEqualTo(0).When(x => x.WeightMin.HasValue);
        RuleFor(x => x.PriceMin).GreaterThanOrEqualTo(0).When(x => x.PriceMin.HasValue);
        RuleFor(x => x.HorsepowerMin).GreaterThanOrEqualTo(0).When(x => x.HorsepowerMin.HasValue);
        RuleFor(x => x.CapacityKgMin).GreaterThanOrEqualTo(0).When(x => x.CapacityKgMin.HasValue);

        RuleFor(x => x.SpecsFilterJson)
            .Must(BeValidJsonObjectOrNull)
            .WithMessage("Invalid specs filter JSON.");

        RuleFor(x => x.SpecMinJson)
            .Must(BeValidJsonObjectOrNull)
            .WithMessage("Invalid specs min JSON.");

        RuleForEach(x => x.AttachmentIds).GreaterThan(0);
    }

    private static bool BeValidJsonObjectOrNull(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
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
}
