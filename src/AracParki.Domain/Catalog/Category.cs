namespace AracParki.Domain.Catalog;

public sealed class Category
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required string IconKey { get; init; }
    public int SortOrder { get; init; }
}
