using AracParki.Application.Listings.Queries;
using AracParki.Application.Listings.Validation;
using AracParki.Domain.Listings;
using FluentValidation.TestHelper;

namespace AracParki.UnitTests;

public sealed class ListingSearchQueryValidatorTests
{
    private readonly ListingSearchQueryValidator _validator = new();

    [Fact]
    public void Valid_default_query_passes()
    {
        var result = _validator.TestValidate(new ListingSearchQuery());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_intent_fails()
    {
        var result = _validator.TestValidate(new ListingSearchQuery { Intent = "hack" });
        result.ShouldHaveValidationErrorFor(x => x.Intent);
    }

    [Fact]
    public void Short_query_fails()
    {
        var result = _validator.TestValidate(new ListingSearchQuery { Query = "a" });
        result.ShouldHaveValidationErrorFor(x => x.Query);
    }

    [Fact]
    public void Known_sort_values_pass()
    {
        foreach (var sort in ListingSort.Known)
        {
            var result = _validator.TestValidate(new ListingSearchQuery { Sort = sort });
            result.ShouldNotHaveValidationErrorFor(x => x.Sort);
        }
    }
}
