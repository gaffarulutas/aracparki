using AracParki.Application.Listings.Dtos;

namespace AracParki.Application.Listings;

public interface ISavedSearchStore
{
    Task<IReadOnlyList<SavedSearchDto>> ListByAccountAsync(long accountId, CancellationToken cancellationToken);

    Task<int> CountByAccountAsync(long accountId, CancellationToken cancellationToken);

    Task<SavedSearchDto?> FindByUrlAsync(long accountId, string url, CancellationToken cancellationToken);

    Task<long> UpsertAsync(long accountId, string name, string url, string queryJson, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(long accountId, long id, CancellationToken cancellationToken);

    Task<bool> DeleteByUrlAsync(long accountId, string url, CancellationToken cancellationToken);
}
