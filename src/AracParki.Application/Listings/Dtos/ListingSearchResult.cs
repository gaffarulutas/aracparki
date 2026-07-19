namespace AracParki.Application.Listings.Dtos;

public sealed class ListingSearchResult
{
    public required IReadOnlyList<ListingCardDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasMore { get; init; }
    public DateTimeOffset? NextCursorListedAt { get; init; }
    public long? NextCursorId { get; init; }
}
