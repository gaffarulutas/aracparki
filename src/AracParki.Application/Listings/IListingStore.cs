using AracParki.Application.Listings.Commands;
using AracParki.Application.Listings.Dtos;

namespace AracParki.Application.Listings;

public interface IListingStore
{
    /// <summary>
    /// Ensures a seller for the account, inserts listing in pending_review + images, returns ad_no.
    /// </summary>
    Task<string> CreatePublishedAsync(CreatePublishedListingCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Owner resubmit: update listing fields, reset moderation, set pending_review.
    /// </summary>
    Task UpdateForReviewAsync(
        string adNo,
        long accountId,
        CreatePublishedListingCommand command,
        CancellationToken cancellationToken);

    Task ApproveAsync(
        string adNo,
        long adminAccountId,
        int publishedDurationDays,
        CancellationToken cancellationToken);

    Task RejectAsync(string adNo, long adminAccountId, string reason, CancellationToken cancellationToken);

    /// <summary>Owner unpublish: published → archived.</summary>
    Task ArchiveByOwnerAsync(string adNo, long accountId, CancellationToken cancellationToken);

    /// <summary>Admin take-down: published → archived.</summary>
    Task ArchiveByAdminAsync(string adNo, long adminAccountId, CancellationToken cancellationToken);

    /// <summary>Owner republish: archived → pending_review.</summary>
    Task RepublishByOwnerAsync(string adNo, long accountId, CancellationToken cancellationToken);

    /// <summary>Batch-expire published listings past expires_at. Returns affected row count.</summary>
    Task<int> ExpirePublishedAsync(CancellationToken cancellationToken);

    Task<ModerationCountsDto> GetModerationCountsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ModerationListItemDto>> ListForModerationAsync(
        string status,
        int take,
        CancellationToken cancellationToken);
}
