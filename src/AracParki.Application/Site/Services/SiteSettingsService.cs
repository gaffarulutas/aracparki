using AracParki.Application.Site.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace AracParki.Application.Site.Services;

public sealed class SiteSettingsService(ISiteSettingsStore store, IMemoryCache cache)
{
    private const string CacheKey = "site-settings:v1";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<SiteSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await cache.GetOrCreateAsync(
            CacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await store.GetAsync(cancellationToken);
            });

        return settings ?? SiteSettingsDto.CreateDefaults();
    }

    public async Task UpdateAsync(SiteSettingsDto settings, long? updatedByAccountId, CancellationToken cancellationToken = default)
    {
        await store.UpdateAsync(settings, updatedByAccountId, cancellationToken);
        cache.Remove(CacheKey);
    }
}
