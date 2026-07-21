namespace AracParki.Application.Listings.Dtos;

public sealed class ModerationCountsDto
{
    public int PendingReview { get; init; }
    public int Published { get; init; }
    public int Rejected { get; init; }
}

public sealed class ModerationListItemDto
{
    public long Id { get; init; }
    public required string AdNo { get; init; }
    public required string Title { get; init; }
    public required string Status { get; init; }
    public required string CoverImageUrl { get; init; }
    public required string SellerName { get; init; }
    public required string City { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
    public DateTimeOffset ListedAt { get; init; }
    public string? RejectionReason { get; init; }
}
