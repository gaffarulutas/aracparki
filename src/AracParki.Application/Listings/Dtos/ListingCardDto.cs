namespace AracParki.Application.Listings.Dtos;

public sealed class ListingCardDto
{
    public long Id { get; init; }
    public required string AdNo { get; init; }
    public required string Title { get; init; }
    public required string Category { get; init; }
    public required string Brand { get; init; }
    public required string ModelName { get; init; }
    public required string PrimaryIntent { get; init; }
    public required string Condition { get; init; }
    public int ModelYear { get; init; }
    public int? Hours { get; init; }
    public decimal Tons { get; init; }
    public int? CapacityKg { get; init; }
    public int? Horsepower { get; init; }
    public required string City { get; init; }
    public required string District { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = Domain.Listings.Currency.Try;
    public string? PriceUnit { get; init; }
    public required string CoverImageUrl { get; init; }
    public required string SellerType { get; init; }
    public bool IsVerified { get; init; }
    public DateTimeOffset ListedAt { get; init; }
    public string Status { get; init; } = Domain.Listings.ListingStatus.Published;
    public string? RejectionReason { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}
