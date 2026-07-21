namespace AracParki.Application.Listings.Dtos;

public sealed class ListingEditDto
{
    public long Id { get; init; }
    public required string AdNo { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public int CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CapacityMetric { get; init; }
    public int GroupId { get; init; }
    public string? GroupName { get; init; }
    public int BrandId { get; init; }
    public string? BrandName { get; init; }
    public int? ModelId { get; init; }
    public required string ModelName { get; init; }
    public string? SerialNo { get; init; }
    public required string Condition { get; init; }
    public int ModelYear { get; init; }
    public int? Hours { get; init; }
    public decimal Tons { get; init; }
    public int? CapacityKg { get; init; }
    public int? Horsepower { get; init; }
    public required string PrimaryIntent { get; init; }
    public decimal Price { get; init; }
    public required string Currency { get; init; }
    public string? PriceUnit { get; init; }
    public bool IncludesOperator { get; init; }
    public required string SellerType { get; init; }
    public int CityId { get; init; }
    public string? CityName { get; init; }
    public int DistrictId { get; init; }
    public string? DistrictName { get; init; }
    public int? NeighborhoodId { get; init; }
    public string? NeighborhoodName { get; init; }
    public required string SpecsJson { get; init; }
    public required string Status { get; init; }
    public string? RejectionReason { get; init; }
    public IReadOnlyList<string> ImageUrls { get; init; } = [];
    public IReadOnlyList<int> AttachmentIds { get; init; } = [];
}
