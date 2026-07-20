using AracParki.Application.Catalog.Services;
using AracParki.Application.Listings;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using AracParki.Domain.Listings;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Ilan;

public sealed class IndexModel(ListingService listingService, CatalogService catalog, SiteUrls siteUrls) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string AdNo { get; set; } = string.Empty;

    public ListingDetailDto? Listing { get; private set; }
    public IReadOnlyList<SpecDisplayRow> SpecRows { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(AdNo))
        {
            return NotFound();
        }

        Listing = await listingService.GetByAdNoAsync(AdNo, cancellationToken);
        if (Listing is null)
        {
            return NotFound();
        }

        if (Listing.CategoryId > 0)
        {
            var attrs = await catalog.GetCategoryAttributesAsync(Listing.CategoryId, cancellationToken);
            SpecRows = SpecsJsonBuilder.ToDisplayRows(Listing.SpecsJson, attrs);
        }

        ViewData["PageKey"] = "detail";
        ViewData["Title"] = $"{Listing.Title} | Araç Parkı";
        ViewData["OgTitle"] = $"{Listing.Title} | Araç Parkı";
        ViewData["OgType"] = "product";
        ViewData["Description"] =
            $"{Listing.Title} — {Listing.ModelYear}{(Listing.Hours is null ? "" : $", {Listing.Hours} saat")}, {Listing.City}. İş makinesi ilanı.";
        ViewData["SearchQuery"] = string.Empty;
        ViewData["CanonicalIncludeQuery"] = false;

        var cover = !string.IsNullOrWhiteSpace(Listing.CoverImageUrl)
            ? Listing.CoverImageUrl
            : Listing.ImageUrls.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(cover))
        {
            ViewData["OgImage"] = cover;
            ViewData["OgImageAlt"] = Listing.Title;
            ViewData["TwitterCard"] = "summary_large_image";
        }

        ViewData["JsonLd"] = BuildProductJsonLd(Listing);

        return Page();
    }

    private string BuildProductJsonLd(ListingDetailDto listing)
    {
        var images = listing.ImageUrls.Count > 0
            ? listing.ImageUrls.Select(siteUrls.Absolute).ToList()
            : string.IsNullOrWhiteSpace(listing.CoverImageUrl)
                ? new List<string>()
                : [siteUrls.Absolute(listing.CoverImageUrl)];

        var product = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Product",
            ["name"] = listing.Title,
            ["description"] = listing.Description,
            ["sku"] = listing.AdNo,
            ["brand"] = new Dictionary<string, object?>
            {
                ["@type"] = "Brand",
                ["name"] = listing.Brand
            },
            ["category"] = listing.Category,
            ["image"] = images,
            ["offers"] = new Dictionary<string, object?>
            {
                ["@type"] = "Offer",
                ["url"] = siteUrls.Absolute($"/ilan/{listing.AdNo}"),
                ["priceCurrency"] = "TRY",
                ["price"] = listing.Price.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                ["availability"] = "https://schema.org/InStock",
                ["itemCondition"] = listing.Condition.Contains("sıfır", StringComparison.OrdinalIgnoreCase)
                    || listing.Condition.Contains("sifir", StringComparison.OrdinalIgnoreCase)
                    ? "https://schema.org/NewCondition"
                    : "https://schema.org/UsedCondition"
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(product);
    }

    public string BackToListUrl()
    {
        if (Listing is null)
        {
            return ListingRoutes.List;
        }

        var intent = Listing.PrimaryIntent switch
        {
            ListingIntent.Kiralik => ListingIntent.Kiralik,
            _ => ListingIntent.Satilik
        };

        return ListingRoutes.ListUrl(new()
        {
            Intent = intent,
            Category = Listing.Category
        });
    }
}
