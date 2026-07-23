namespace AracParki.Application.Listings.Dtos;

public sealed class ListingReportCountsDto
{
    public int Open { get; init; }
    public int Reviewing { get; init; }
    public int Actioned { get; init; }
    public int Dismissed { get; init; }

    public int Active => Open + Reviewing;
}

public sealed class ListingReportListItemDto
{
    public long Id { get; init; }
    public long ListingId { get; init; }
    public required string AdNo { get; init; }
    public required string ListingTitle { get; init; }
    public required string ListingStatus { get; init; }
    public required string ReasonCode { get; init; }
    public string? Message { get; init; }
    public required string Status { get; init; }
    public long ReporterAccountId { get; init; }
    public required string ReporterName { get; init; }
    public required string ReporterEmail { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReviewedAt { get; init; }
}

public sealed class ListingReportDetailDto
{
    public long Id { get; init; }
    public long ListingId { get; init; }
    public required string AdNo { get; init; }
    public required string ListingTitle { get; init; }
    public required string ListingStatus { get; init; }
    public string? CoverImageUrl { get; init; }
    public required string ReasonCode { get; init; }
    public string? Message { get; init; }
    public required string Status { get; init; }
    public string? AdminNotes { get; init; }
    public long ReporterAccountId { get; init; }
    public required string ReporterName { get; init; }
    public required string ReporterEmail { get; init; }
    public long? ReviewedByAccountId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReviewedAt { get; init; }
}
