namespace AracParki.Domain.Catalog;

public sealed class City
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public bool IsPopular { get; init; }
    public int SortOrder { get; init; }
}
