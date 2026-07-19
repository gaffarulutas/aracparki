using AracParki.Application.Abstractions;
using AracParki.Application.Catalog;
using AracParki.Application.Catalog.Dtos;
using Dapper;

namespace AracParki.Infrastructure.Catalog;

public sealed class CatalogRepository : ICatalogQuery
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ISqlQueryLoader _sql;

    public CatalogRepository(IDbConnectionFactory connectionFactory, ISqlQueryLoader sql)
    {
        _connectionFactory = connectionFactory;
        _sql = sql;
    }

    public async Task<IReadOnlyList<CategorySummaryDto>> GetCategoriesWithCountsAsync(CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<CategorySummaryDto>(
            new CommandDefinition(_sql.Get("Catalog/GetCategoriesWithCounts.sql"), cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<CitySummaryDto>> GetPopularCitiesAsync(CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<CitySummaryDto>(
            new CommandDefinition(_sql.Get("Catalog/GetPopularCities.sql"), cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<CityOptionDto>> GetAllCitiesAsync(CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<CityOptionDto>(
            new CommandDefinition(_sql.Get("Catalog/GetAllCities.sql"), cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<CategoryOptionDto>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<CategoryOptionDto>(
            new CommandDefinition(_sql.Get("Catalog/GetAllCategories.sql"), cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<CategoryGroupDto>> GetCategoryGroupsAsync(CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var groups = (await connection.QueryAsync<(int Id, string Name, string Slug)>(
            new CommandDefinition(_sql.Get("Catalog/GetCategoryGroups.sql"), cancellationToken: cancellationToken))).AsList();
        var categories = (await connection.QueryAsync<CategoryOptionDto>(
            new CommandDefinition(_sql.Get("Catalog/GetAllCategories.sql"), cancellationToken: cancellationToken))).AsList();

        return groups.Select(g => new CategoryGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Slug = g.Slug,
            Categories = categories.Where(c => c.GroupId == g.Id).ToList()
        }).ToList();
    }

    public async Task<IReadOnlyList<BrandOptionDto>> GetAllBrandsAsync(CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<BrandOptionDto>(
            new CommandDefinition(_sql.Get("Catalog/GetAllBrands.sql"), cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<BrandOptionDto>> GetBrandsByCategoryAsync(int categoryId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<BrandOptionDto>(
            new CommandDefinition(
                _sql.Get("Catalog/GetBrandsByCategory.sql"),
                new { CategoryId = categoryId },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<EquipmentModelOptionDto>> GetModelsByBrandCategoryAsync(
        int brandId,
        int categoryId,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<EquipmentModelOptionDto>(
            new CommandDefinition(
                _sql.Get("Catalog/GetModelsByBrandCategory.sql"),
                new { BrandId = brandId, CategoryId = categoryId },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<CategoryAttributeDto>> GetCategoryAttributesAsync(int categoryId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<CategoryAttributeDto>(
            new CommandDefinition(
                _sql.Get("Catalog/GetCategoryAttributes.sql"),
                new { CategoryId = categoryId },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<AttachmentOptionDto>> GetAttachmentsAsync(CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<AttachmentOptionDto>(
            new CommandDefinition(_sql.Get("Catalog/GetAttachments.sql"), cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<FacetCountDto>> GetBrandFacetsAsync(int? categoryId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<FacetCountDto>(
            new CommandDefinition(
                _sql.Get("Catalog/GetBrandFacets.sql"),
                new { CategoryId = categoryId },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<DistrictOptionDto>> GetDistrictsByCityAsync(int cityId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<DistrictOptionDto>(
            new CommandDefinition(
                _sql.Get("Catalog/GetDistrictsByCity.sql"),
                new { CityId = cityId },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<NeighborhoodOptionDto>> GetNeighborhoodsByDistrictAsync(
        int districtId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<NeighborhoodOptionDto>(
            new CommandDefinition(
                _sql.Get("Catalog/GetNeighborhoodsByDistrict.sql"),
                new { DistrictId = districtId, Take = take },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<StreetOptionDto>> GetStreetsByNeighborhoodAsync(
        int neighborhoodId,
        string? query,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<StreetOptionDto>(
            new CommandDefinition(
                _sql.Get("Catalog/GetStreetsByNeighborhood.sql"),
                new
                {
                    NeighborhoodId = neighborhoodId,
                    Query = string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2 ? null : query.Trim(),
                    Take = take
                },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }
}
