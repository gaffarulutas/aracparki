using AracParki.Application.Abstractions;
using AracParki.Application.Accounts;
using Dapper;

namespace AracParki.Infrastructure.Accounts;

public sealed class PhoneOtpRepository(IDbConnectionFactory connectionFactory) : IPhoneOtpStore
{
    private sealed class OtpRow
    {
        public required string Phone { get; init; }
        public required string CodeHash { get; init; }
    }

    public async Task SaveAsync(
        long accountId,
        string phone,
        string codeHash,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    UPDATE phone_otp_tokens
                    SET consumed_at = COALESCE(consumed_at, NOW())
                    WHERE account_id = @AccountId
                      AND consumed_at IS NULL
                    """,
                    new { AccountId = accountId },
                    transaction: tx,
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO phone_otp_tokens (account_id, phone, code_hash, expires_at)
                    VALUES (@AccountId, @Phone, @CodeHash, @ExpiresAt)
                    """,
                    new
                    {
                        AccountId = accountId,
                        Phone = phone,
                        CodeHash = codeHash,
                        ExpiresAt = expiresAt.UtcDateTime
                    },
                    transaction: tx,
                    cancellationToken: cancellationToken));

            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(string Phone, string CodeHash)?> GetLatestAsync(
        long accountId,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<OtpRow>(
            new CommandDefinition(
                """
                SELECT phone AS Phone, code_hash AS CodeHash
                FROM phone_otp_tokens
                WHERE account_id = @AccountId
                  AND consumed_at IS NULL
                  AND expires_at > NOW()
                ORDER BY created_at DESC
                LIMIT 1
                """,
                new { AccountId = accountId },
                cancellationToken: cancellationToken));

        return row is null ? null : (row.Phone, row.CodeHash);
    }

    public async Task ConsumeLatestAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE phone_otp_tokens
                SET consumed_at = NOW()
                WHERE id = (
                    SELECT id
                    FROM phone_otp_tokens
                    WHERE account_id = @AccountId
                      AND consumed_at IS NULL
                      AND expires_at > NOW()
                    ORDER BY created_at DESC
                    LIMIT 1
                )
                """,
                new { AccountId = accountId },
                cancellationToken: cancellationToken));
    }
}
