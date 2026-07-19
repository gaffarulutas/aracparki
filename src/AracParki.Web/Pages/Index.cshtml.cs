using AracParki.Application.Catalog.Dtos;
using AracParki.Application.Catalog.Services;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Queries;
using AracParki.Application.Listings.Services;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages;

public sealed class IndexModel : PageModel
{
    private readonly ListingService _listingService;
    private readonly CatalogService _catalogService;

    public IndexModel(ListingService listingService, CatalogService catalogService)
    {
        _listingService = listingService;
        _catalogService = catalogService;
    }

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

        Categories = await _catalogService.GetCategoriesWithCountsAsync(cancellationToken);
        PopularCities = await _catalogService.GetPopularCitiesAsync(cancellationToken);
        CategoryOptions = await _catalogService.GetAllCategoriesAsync(cancellationToken);
        CityOptions = await _catalogService.GetAllCitiesAsync(cancellationToken);
        Featured = await _listingService.GetFeaturedAsync(Filter, 12, cancellationToken);

        ViewData["PageKey"] = "home";
        ViewData["Title"] = "Araç Parkı | Türkiye İş Makinesi Satılık · Kiralık · İkinci El";
        ViewData["Description"] =
            "Satılık, kiralık ve ikinci el iş makineleri — tonaj, güç ve çalışma saatiyle karşılaştır.";
        ViewData["SearchQuery"] = Filter.Query;
    }
}
