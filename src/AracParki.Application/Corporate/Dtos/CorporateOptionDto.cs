namespace AracParki.Application.Corporate.Dtos;

/// <summary>İlan sihirbazındaki satıcı seçimi için hafif kurumsal hesap özeti.</summary>
public sealed class CorporateOptionDto
{
    public long Id { get; init; }
    public required string DisplayName { get; init; }
    public required string TradeName { get; init; }
    public required string CompanyType { get; init; }
    public required string Phone { get; init; }
}
