using AracParki.Application.Listings.Dtos;
using AracParki.Domain.Listings;
using Microsoft.Extensions.Options;

namespace AracParki.Application.Listings.Services;

public sealed class ListingModerationService(
    IListingStore store,
    IOptions<ListingOptions> options)
{
    public const int RejectionReasonMaxLength = 1000;

    public Task<ModerationCountsDto> GetCountsAsync(CancellationToken cancellationToken)
        => store.GetModerationCountsAsync(cancellationToken);

    public Task<IReadOnlyList<ModerationListItemDto>> ListAsync(
        string? status,
        int take,
        CancellationToken cancellationToken)
    {
        var normalized = string.IsNullOrWhiteSpace(status)
            ? ListingStatus.PendingReview
            : status.Trim();
        if (!ListingStatus.Known.Contains(normalized)
            || normalized is ListingStatus.Draft)
        {
            normalized = ListingStatus.PendingReview;
        }

        return store.ListForModerationAsync(normalized, Math.Clamp(take, 1, 100), cancellationToken);
    }

    public Task ApproveAsync(string adNo, long adminAccountId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adNo);
        if (adminAccountId <= 0)
            throw new ArgumentOutOfRangeException(nameof(adminAccountId));

        var days = Math.Clamp(options.Value.PublishedDurationDays, 1, 365);
        return store.ApproveAsync(adNo.Trim(), adminAccountId, days, cancellationToken);
    }

    public Task RejectAsync(string adNo, long adminAccountId, string? reason, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adNo);
        if (adminAccountId <= 0)
            throw new ArgumentOutOfRangeException(nameof(adminAccountId));

        var trimmed = reason?.Trim() ?? "";
        if (trimmed.Length == 0)
            throw new ArgumentException("Red nedeni zorunlu.", nameof(reason));

        if (trimmed.Length > RejectionReasonMaxLength)
            throw new ArgumentException(
                $"Red nedeni en fazla {RejectionReasonMaxLength} karakter olabilir.",
                nameof(reason));

        return store.RejectAsync(adNo.Trim(), adminAccountId, trimmed, cancellationToken);
    }

    public Task ArchiveAsync(string adNo, long adminAccountId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adNo);
        if (adminAccountId <= 0)
            throw new ArgumentOutOfRangeException(nameof(adminAccountId));

        return store.ArchiveByAdminAsync(adNo.Trim(), adminAccountId, cancellationToken);
    }
}
