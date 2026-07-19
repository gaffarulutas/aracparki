using AracParki.Application.Listings.Queries;
using AracParki.Application.Listings.Validation;
using FluentValidation.TestHelper;

namespace AracParki.UnitTests;

public sealed class SpecFilterBuilderTests
{
    [Fact]
    public void Build_splits_bool_enum_and_number()
    {
        var (equality, mins) = SpecFilterBuilder.Build(new Dictionary<string, string>
        {
            ["breaker_circuit"] = "true",
            ["fuel"] = "diesel",
            ["lift_height_m"] = "3.5"
        });

        Assert.Contains("breaker_circuit", equality, StringComparison.Ordinal);
        Assert.Contains("fuel", equality, StringComparison.Ordinal);
        Assert.Contains("diesel", equality, StringComparison.Ordinal);
        Assert.Contains("lift_height_m", mins, StringComparison.Ordinal);
        Assert.Contains("3.5", mins, StringComparison.Ordinal);
    }

    [Fact]
    public void Invalid_price_unit_fails_validation()
    {
        var validator = new ListingSearchQueryValidator();
        var result = validator.TestValidate(new ListingSearchQuery { PriceUnit = "year" });
        result.ShouldHaveValidationErrorFor(x => x.PriceUnit);
    }
}
