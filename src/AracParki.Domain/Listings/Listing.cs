namespace AracParki.Domain.Listings;

public sealed class Listing
{
    public long Id { get; init; }
    public required string AdNo { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public int CategoryId { get; init; }
    public int BrandId { get; init; }
    public int? ModelId { get; init; }
    public required string ModelName { get; init; }
    public string? SerialNo { get; init; }
    public int CityId { get; init; }
    public int DistrictId { get; init; }
    public long SellerId { get; init; }
    public required string PrimaryIntent { get; init; }
    public required string[] Intents { get; init; }
    public required string Condition { get; init; }
    public int ModelYear { get; init; }
    public int Hours { get; init; }
    public decimal Tons { get; init; }
    public int? CapacityKg { get; init; }
    public int Horsepower { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = Domain.Listings.Currency.Try;
    public string? PriceUnit { get; init; }
    public bool IncludesOperator { get; init; }
    public required string SpecsJson { get; init; }
    public required string CoverImageUrl { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset ListedAt { get; init; }
}
