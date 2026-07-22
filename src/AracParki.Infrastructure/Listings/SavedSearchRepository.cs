using AracParki.Application.Abstractions;
using AracParki.Application.Listings;
using AracParki.Application.Listings.Dtos;
using Dapper;
using Npgsql;

namespace AracParki.Infrastructure.Listings;

public sealed class SavedSearchRepository(IDbConnectionFactory connectionFactory) : ISavedSearchStore
{
    private const int MaxPerAccount = 50;

    private const string SelectSql = """
        SELECT
            id,
            name AS Name,
            query_json->>'url' AS Url,
            query_json::text AS QueryJson,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM saved_searches
        """;

    public async Task<IReadOnlyList<SavedSearchDto>> ListByAccountAsync(
        long accountId,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<SavedSearchDto>(
            new CommandDefinition(
                SelectSql + """
                 WHERE account_id = @AccountId
                ORDER BY created_at DESC, id DESC
                LIMIT @Take
                """,
                new { AccountId = accountId, Take = MaxPerAccount },
                cancellationToken: cancellationToken));

        return items.AsList();
    }

    public async Task<int> CountByAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                SELECT COUNT(*)::int
                FROM saved_searches
                WHERE account_id = @AccountId
                """,
                new { AccountId = accountId },
                cancellationToken: cancellationToken));
    }

    public async Task<SavedSearchDto?> FindByUrlAsync(
        long accountId,
        string url,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<SavedSearchDto>(
            new CommandDefinition(
                SelectSql + """
                 WHERE account_id = @AccountId
                   AND query_json->>'url' = @Url
                LIMIT 1
                """,
                new { AccountId = accountId, Url = url },
                cancellationToken: cancellationToken));
    }

    public async Task<long> UpsertAsync(
        long accountId,
        string name,
        string url,
        string queryJson,
        CancellationToken cancellationToken)
    {
        await using var connection = (NpgsqlConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingId = await connection.ExecuteScalarAsync<long?>(
                new CommandDefinition(
                    """
                    SELECT id
                    FROM saved_searches
                    WHERE account_id = @AccountId
                      AND query_json->>'url' = @Url
                    LIMIT 1
                    FOR UPDATE
                    """,
                    new { AccountId = accountId, Url = url },
                    transaction: tx,
                    cancellationToken: cancellationToken));

            if (existingId is > 0)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        UPDATE saved_searches
                        SET name = @Name,
                            query_json = CAST(@QueryJson AS jsonb)
                        WHERE id = @Id
                          AND account_id = @AccountId
                        """,
                        new
                        {
                            Id = existingId.Value,
                            AccountId = accountId,
                            Name = name,
                            QueryJson = queryJson
                        },
                        transaction: tx,
                        cancellationToken: cancellationToken));
                await tx.CommitAsync(cancellationToken);
                return existingId.Value;
            }

            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    """
                    SELECT COUNT(*)::int
                    FROM saved_searches
                    WHERE account_id = @AccountId
                    """,
                    new { AccountId = accountId },
                    transaction: tx,
                    cancellationToken: cancellationToken));

            if (count >= MaxPerAccount)
            {
                throw new InvalidOperationException($"En fazla {MaxPerAccount} kayıtlı arama tutabilirsin.");
            }

            try
            {
                var id = await connection.ExecuteScalarAsync<long>(
                    new CommandDefinition(
                        """
                        INSERT INTO saved_searches (account_id, name, query_json)
                        VALUES (@AccountId, @Name, CAST(@QueryJson AS jsonb))
                        RETURNING id
                        """,
                        new
                        {
                            AccountId = accountId,
                            Name = name,
                            QueryJson = queryJson
                        },
                        transaction: tx,
                        cancellationToken: cancellationToken));
                await tx.CommitAsync(cancellationToken);
                return id;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                // Concurrent insert won; update that row.
                var id = await connection.ExecuteScalarAsync<long>(
                    new CommandDefinition(
                        """
                        UPDATE saved_searches
                        SET name = @Name,
                            query_json = CAST(@QueryJson AS jsonb)
                        WHERE account_id = @AccountId
                          AND query_json->>'url' = @Url
                        RETURNING id
                        """,
                        new
                        {
                            AccountId = accountId,
                            Url = url,
                            Name = name,
                            QueryJson = queryJson
                        },
                        transaction: tx,
                        cancellationToken: cancellationToken));
                await tx.CommitAsync(cancellationToken);
                return id;
            }
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long accountId, long id, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE FROM saved_searches
                WHERE id = @Id
                  AND account_id = @AccountId
                """,
                new { Id = id, AccountId = accountId },
                cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> DeleteByUrlAsync(long accountId, string url, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE FROM saved_searches
                WHERE account_id = @AccountId
                  AND query_json->>'url' = @Url
                """,
                new { AccountId = accountId, Url = url },
                cancellationToken: cancellationToken));
        return rows > 0;
    }
}
