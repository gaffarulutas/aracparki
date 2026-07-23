using AracParki.Application.Site.Dtos;

namespace AracParki.Application.Site;

public interface ISiteSettingsStore
{
    Task<SiteSettingsDto> GetAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(SiteSettingsDto settings, long? updatedByAccountId, CancellationToken cancellationToken = default);
}
