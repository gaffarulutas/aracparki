using AracParki.Application.Catalog.Dtos;
using AracParki.Application.Catalog.Services;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Queries;
using AracParki.Application.Listings.Services;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages;

public sealed class IndexModel(ListingService listingService, CatalogService catalogService) : PageModel
{
    private const int FeaturedTake = 15;

    public IReadOnlyList<CategorySummaryDto> Categories { get; private set; } = [];
    public IReadOnlyList<CitySummaryDto> PopularCities { get; private set; } = [];
    public IReadOnlyList<CategoryOptionDto> CategoryOptions { get; private set; } = [];
    public IReadOnlyList<CityOptionDto> CityOptions { get; private set; } = [];
    public IReadOnlyList<ListingCardDto> Featured { get; private set; } = [];
    public ListingSearchQuery Filter { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Filter = ListingRoutes.FromRequest(Request.Query);
        if (!Request.Query.Any())
        {
            Filter = new ListingSearchQuery();
        }

        Categories = await catalogService.GetCategoriesWithCountsAsync(cancellationToken);
        PopularCities = await catalogService.GetPopularCitiesAsync(cancellationToken);
        CategoryOptions = await catalogService.GetAllCategoriesAsync(cancellationToken);
        CityOptions = await catalogService.GetAllCitiesAsync(cancellationToken);
        Featured = await listingService.GetFeaturedAsync(Filter, FeaturedTake, cancellationToken);

        ViewData["PageKey"] = "home";
        ViewData["Title"] = "Araç Parkı | Türkiye İş Makinesi Satılık · Kiralık · İkinci El";
        ViewData["Description"] =
            "Satılık, kiralık ve ikinci el iş makineleri — tonaj, güç ve çalışma saatiyle karşılaştır.";
        ViewData["SearchQuery"] = Filter.Query;
        ViewData["OgImageType"] = "image/jpeg";
        ViewData["OgImageWidth"] = "1200";
        ViewData["OgImageHeight"] = "630";
    }
}
