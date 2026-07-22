namespace AracParki.Application.Listings.Dtos;

public sealed class SavedSearchDto
{
    public long Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string QueryJson { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
