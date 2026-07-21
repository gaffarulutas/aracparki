namespace AracParki.Application.Corporate;

/// <summary>Kurumsal hesap formundan gelen, normalize edilmiş profil verisi.</summary>
public sealed record CorporateProfileData(
    string CompanyType,
    string TradeName,
    string DisplayName,
    string TaxOffice,
    string TaxNumber,
    string? MersisNo,
    string? TradeRegistryNo,
    string? KepAddress,
    string AuthorizedName,
    string Phone,
    string Email,
    string? Website,
    int CityId,
    int DistrictId,
    string AddressLine);
