using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Queries;
using AracParki.Application.Listings.Services;
using AracParki.Domain.Listings;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace AracParki.Web.Pages.Api;

[EnableRateLimiting("featured-listings")]
public sealed class FeaturedGridModel(ListingService listingService) : PageModel
{
    private const int Take = 15;

    public IReadOnlyList<ListingCardDto> Items { get; private set; } = [];

    public async Task OnGetAsync(string? tip, CancellationToken cancellationToken)
    {
        var intent = string.IsNullOrWhiteSpace(tip) ? ListingIntent.All : tip.Trim();
        if (!ListingIntent.Known.Contains(intent))
        {
            intent = ListingIntent.All;
        }

        var filter = new ListingSearchQuery { Intent = intent };
        Items = await listingService.GetFeaturedAsync(filter, Take, cancellationToken);

        Response.Headers.CacheControl = "private, max-age=30";
        Response.Headers["X-Featured-Count"] = Items.Count.ToString();
        Response.Headers["X-Featured-See-All"] = ListingRoutes.ListUrl(filter);
    }
}
