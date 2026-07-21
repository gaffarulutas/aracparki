using AracParki.Application.Abstractions;
using AracParki.Application.Listings;
using Dapper;

namespace AracParki.Infrastructure.Listings;

public sealed class WizardDraftRepository(IDbConnectionFactory connectionFactory) : IWizardDraftStore
{
    public async Task<string?> GetPayloadAsync(long accountId, CancellationToken cancellationToken)
    {
        var meta = await GetMetaAsync(accountId, cancellationToken);
        return meta?.PayloadJson;
    }

    public async Task<WizardDraftMeta?> GetMetaAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<WizardDraftMeta>(
            new CommandDefinition(
                """
                SELECT
                    step AS Step,
                    updated_at AS UpdatedAt,
                    payload::text AS PayloadJson
                FROM listing_wizard_drafts
                WHERE account_id = @AccountId
                """,
                new { AccountId = accountId },
                cancellationToken: cancellationToken));
    }

    public async Task SaveAsync(long accountId, int step, string payloadJson, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO listing_wizard_drafts (account_id, payload, step, updated_at)
                VALUES (@AccountId, CAST(@Payload AS jsonb), @Step, NOW())
                ON CONFLICT (account_id) DO UPDATE
                SET payload = EXCLUDED.payload,
                    step = EXCLUDED.step,
                    updated_at = NOW()
                """,
                new { AccountId = accountId, Payload = payloadJson, Step = step },
                cancellationToken: cancellationToken));
    }

    public async Task ClearAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE FROM listing_wizard_drafts
                WHERE account_id = @AccountId
                """,
                new { AccountId = accountId },
                cancellationToken: cancellationToken));
    }
}
