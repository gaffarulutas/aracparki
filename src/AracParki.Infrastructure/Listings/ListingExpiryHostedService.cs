using AracParki.Application.Listings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AracParki.Infrastructure.Listings;

public sealed class ListingExpiryHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<ListingOptions> options,
    ILogger<ListingExpiryHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Stagger startup so migrate / warm-up finish first.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var store = scope.ServiceProvider.GetRequiredService<IListingStore>();
                var expired = await store.ExpirePublishedAsync(stoppingToken);
                if (expired > 0)
                {
                    logger.LogInformation("Expired {Count} published listing(s).", expired);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Listing expiry job failed.");
            }

            var minutes = Math.Clamp(options.Value.ExpiryPollMinutes, 1, 60);
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(minutes), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
