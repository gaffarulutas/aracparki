using AracParki.Application.Listings.Dtos;

namespace AracParki.Application.Listings;

public interface IListingReportStore
{
    Task<long> CreateAsync(
        long listingId,
        string adNo,
        long reporterAccountId,
        string reasonCode,
        string? message,
        CancellationToken cancellationToken);

    Task<bool> HasActiveReportAsync(
        long reporterAccountId,
        long listingId,
        CancellationToken cancellationToken);

    Task<ListingReportDetailDto?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ListingReportListItemDto>> ListAsync(
        string status,
        int take,
        CancellationToken cancellationToken);

    Task<ListingReportCountsDto> GetCountsAsync(CancellationToken cancellationToken);

    Task<bool> UpdateStatusAsync(
        long id,
        string status,
        long adminAccountId,
        string? adminNotes,
        CancellationToken cancellationToken);
}
