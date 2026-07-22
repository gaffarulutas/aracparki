using System.Text.Json;
using AracParki.Application.Listings.Dtos;

namespace AracParki.Application.Listings.Services;

public sealed class SavedSearchService(ISavedSearchStore store)
{
    public const int NameMaxLength = 160;
    public const int UrlMaxLength = 2000;

    public Task<IReadOnlyList<SavedSearchDto>> ListAsync(long accountId, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            return Task.FromResult<IReadOnlyList<SavedSearchDto>>([]);
        }

        return store.ListByAccountAsync(accountId, cancellationToken);
    }

    public async Task<SavedSearchDto> SaveAsync(
        long accountId,
        string? label,
        string? url,
        CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(accountId));
        }

        var normalizedUrl = NormalizeUrl(url);
        var name = string.IsNullOrWhiteSpace(label)
            ? "Kayıtlı arama"
            : label.Trim();
        if (name.Length > NameMaxLength)
        {
            name = name[..NameMaxLength];
        }

        var queryJson = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["url"] = normalizedUrl,
            ["label"] = name
        });

        var id = await store.UpsertAsync(accountId, name, normalizedUrl, queryJson, cancellationToken);
        return new SavedSearchDto
        {
            Id = id,
            Name = name,
            Url = normalizedUrl,
            QueryJson = queryJson,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public Task<bool> DeleteAsync(long accountId, long id, CancellationToken cancellationToken)
    {
        if (accountId <= 0 || id <= 0)
        {
            return Task.FromResult(false);
        }

        return store.DeleteAsync(accountId, id, cancellationToken);
    }

    public Task<bool> DeleteByUrlAsync(long accountId, string? url, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            return Task.FromResult(false);
        }

        var normalizedUrl = NormalizeUrl(url);
        return store.DeleteByUrlAsync(accountId, normalizedUrl, cancellationToken);
    }

    public async Task<bool> ExistsByUrlAsync(long accountId, string? url, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            return false;
        }

        var normalizedUrl = NormalizeUrl(url);
        var found = await store.FindByUrlAsync(accountId, normalizedUrl, cancellationToken);
        return found is not null;
    }

    private static string NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Arama adresi zorunlu.", nameof(url));
        }

        var trimmed = url.Trim();
        if (trimmed.Length > UrlMaxLength)
        {
            throw new ArgumentException($"Arama adresi en fazla {UrlMaxLength} karakter olabilir.", nameof(url));
        }

        if (!trimmed.StartsWith('/'))
        {
            throw new ArgumentException("Arama adresi geçersiz.", nameof(url));
        }

        if (trimmed.StartsWith("//", StringComparison.Ordinal)
            || trimmed.Contains("://", StringComparison.Ordinal)
            || trimmed.Contains('\\'))
        {
            throw new ArgumentException("Arama adresi geçersiz.", nameof(url));
        }

        return trimmed;
    }
}
