using AracParki.Application.Abstractions;
using AracParki.Application.Listings;
using AracParki.Application.Listings.Dtos;
using AracParki.Domain.Listings;
using Dapper;

namespace AracParki.Infrastructure.Listings;

public sealed class FavoriteRepository(IDbConnectionFactory connectionFactory) : IFavoriteStore
{
    public async Task<bool> IsFavoriteAsync(long accountId, long listingId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT EXISTS(
                    SELECT 1
                    FROM listing_favorites
                    WHERE account_id = @AccountId
                      AND listing_id = @ListingId
                )
                """,
                new { AccountId = accountId, ListingId = listingId },
                cancellationToken: cancellationToken));
    }

    public async Task<bool> ToggleAsync(long accountId, long listingId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var removed = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE FROM listing_favorites
                WHERE account_id = @AccountId
                  AND listing_id = @ListingId
                """,
                new { AccountId = accountId, ListingId = listingId },
                cancellationToken: cancellationToken));

        if (removed > 0)
        {
            return false;
        }

        var inserted = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO listing_favorites (account_id, listing_id)
                SELECT @AccountId, l.id
                FROM listings l
                WHERE l.id = @ListingId
                  AND l.status = @Published
                  AND (l.expires_at IS NULL OR l.expires_at > NOW())
                ON CONFLICT (account_id, listing_id) DO NOTHING
                """,
                new
                {
                    AccountId = accountId,
                    ListingId = listingId,
                    Published = ListingStatus.Published
                },
                cancellationToken: cancellationToken));

        if (inserted > 0)
        {
            return true;
        }

        // Concurrent add: favorite already exists.
        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT EXISTS(
                    SELECT 1
                    FROM listing_favorites
                    WHERE account_id = @AccountId
                      AND listing_id = @ListingId
                )
                """,
                new { AccountId = accountId, ListingId = listingId },
                cancellationToken: cancellationToken));

        if (exists)
        {
            return true;
        }

        throw new InvalidOperationException("Favoriye eklenecek yayındaki ilan bulunamadı.");
    }

    public async Task RemoveAsync(long accountId, long listingId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE FROM listing_favorites
                WHERE account_id = @AccountId
                  AND listing_id = @ListingId
                """,
                new { AccountId = accountId, ListingId = listingId },
                cancellationToken: cancellationToken));
    }

    public async Task<int> CountPublishedAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                SELECT COUNT(*)::int
                FROM listing_favorites f
                JOIN listings l ON l.id = f.listing_id
                WHERE f.account_id = @AccountId
                  AND l.status = @Published
                  AND (l.expires_at IS NULL OR l.expires_at > NOW())
                """,
                new
                {
                    AccountId = accountId,
                    Published = ListingStatus.Published
                },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<ListingCardDto>> ListPublishedAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<ListingCardDto>(
            new CommandDefinition(
                """
                SELECT
                    l.id,
                    l.ad_no AS AdNo,
                    l.title,
                    c.name AS Category,
                    b.name AS Brand,
                    l.model_name AS ModelName,
                    l.primary_intent AS PrimaryIntent,
                    l.condition AS Condition,
                    l.model_year AS ModelYear,
                    l.hours,
                    l.tons,
                    l.capacity_kg AS CapacityKg,
                    l.horsepower,
                    city.name AS City,
                    d.name AS District,
                    l.price,
                    l.currency AS Currency,
                    l.price_unit AS PriceUnit,
                    l.cover_image_url AS CoverImageUrl,
                    s.seller_type AS SellerType,
                    CASE WHEN ca.id IS NOT NULL AND ca.status = 'approved' THEN TRUE ELSE s.is_verified END AS IsVerified,
                    l.listed_at AS ListedAt,
                    l.status AS Status,
                    l.rejection_reason AS RejectionReason,
                    l.expires_at AS ExpiresAt
                FROM listing_favorites f
                JOIN listings l ON l.id = f.listing_id
                JOIN categories c ON c.id = l.category_id
                JOIN brands b ON b.id = l.brand_id
                JOIN cities city ON city.id = l.city_id
                JOIN districts d ON d.id = l.district_id
                JOIN sellers s ON s.id = l.seller_id
                LEFT JOIN corporate_accounts ca ON ca.id = l.corporate_account_id
                WHERE f.account_id = @AccountId
                  AND l.status = @Published
                  AND (l.expires_at IS NULL OR l.expires_at > NOW())
                ORDER BY f.created_at DESC, l.id DESC
                LIMIT @Take
                """,
                new
                {
                    AccountId = accountId,
                    Published = ListingStatus.Published,
                    Take = take
                },
                cancellationToken: cancellationToken));

        return items.AsList();
    }
}
