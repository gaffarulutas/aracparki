using AracParki.Application.Abstractions;
using AracParki.Application.Accounts;
using AracParki.Application.Accounts.Dtos;
using Dapper;

namespace AracParki.Infrastructure.Accounts;

public sealed class AccountRepository(IDbConnectionFactory connectionFactory) : IAccountStore
{
    private const string AccountSelect = """
        SELECT id,
               email,
               password_hash AS PasswordHash,
               first_name AS FirstName,
               last_name AS LastName,
               phone,
               email_confirmed_at AS EmailConfirmedAt
        FROM accounts
        """;

    public async Task<AccountDto?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AccountDto>(
            new CommandDefinition(
                AccountSelect + """
                 WHERE email = @Email
                LIMIT 1
                """,
                new { Email = email },
                cancellationToken: cancellationToken));
    }

    public async Task<AccountDto?> FindByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AccountDto>(
            new CommandDefinition(
                AccountSelect + """
                 WHERE id = @Id
                LIMIT 1
                """,
                new { Id = id },
                cancellationToken: cancellationToken));
    }

    public async Task<long> CreateAsync(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                """
                INSERT INTO accounts (email, password_hash, first_name, last_name, phone)
                VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone)
                RETURNING id
                """,
                new
                {
                    Email = email,
                    PasswordHash = passwordHash,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone
                },
                cancellationToken: cancellationToken));
    }

    public async Task UpdatePasswordHashAsync(long accountId, string passwordHash, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE accounts
                SET password_hash = @PasswordHash, updated_at = NOW()
                WHERE id = @Id
                """,
                new { Id = accountId, PasswordHash = passwordHash },
                cancellationToken: cancellationToken));
    }

    public async Task MarkEmailConfirmedAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE accounts
                SET email_confirmed_at = COALESCE(email_confirmed_at, NOW()), updated_at = NOW()
                WHERE id = @Id
                """,
                new { Id = accountId },
                cancellationToken: cancellationToken));
    }

    public async Task SaveResetTokenAsync(long accountId, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO password_reset_tokens (account_id, token_hash, expires_at)
                VALUES (@AccountId, @TokenHash, @ExpiresAt)
                """,
                new { AccountId = accountId, TokenHash = tokenHash, ExpiresAt = expiresAt },
                cancellationToken: cancellationToken));
    }

    public async Task<(long AccountId, string TokenHash)?> FindValidResetTokenAsync(string tokenHash, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<TokenRow>(
            new CommandDefinition(
                """
                SELECT account_id AS AccountId, token_hash AS TokenHash
                FROM password_reset_tokens
                WHERE token_hash = @TokenHash
                  AND used_at IS NULL
                  AND expires_at > NOW()
                LIMIT 1
                """,
                new { TokenHash = tokenHash },
                cancellationToken: cancellationToken));

        return row is null ? null : (row.AccountId, row.TokenHash);
    }

    public async Task MarkResetTokenUsedAsync(string tokenHash, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE password_reset_tokens
                SET used_at = NOW()
                WHERE token_hash = @TokenHash
                """,
                new { TokenHash = tokenHash },
                cancellationToken: cancellationToken));
    }

    public async Task SaveEmailVerificationTokenAsync(long accountId, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO email_verification_tokens (account_id, token_hash, expires_at)
                VALUES (@AccountId, @TokenHash, @ExpiresAt)
                """,
                new { AccountId = accountId, TokenHash = tokenHash, ExpiresAt = expiresAt },
                cancellationToken: cancellationToken));
    }

    public async Task<(long AccountId, string TokenHash)?> FindValidEmailVerificationTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<TokenRow>(
            new CommandDefinition(
                """
                SELECT account_id AS AccountId, token_hash AS TokenHash
                FROM email_verification_tokens
                WHERE token_hash = @TokenHash
                  AND used_at IS NULL
                  AND expires_at > NOW()
                LIMIT 1
                """,
                new { TokenHash = tokenHash },
                cancellationToken: cancellationToken));

        return row is null ? null : (row.AccountId, row.TokenHash);
    }

    public async Task MarkEmailVerificationTokenUsedAsync(string tokenHash, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE email_verification_tokens
                SET used_at = NOW()
                WHERE token_hash = @TokenHash
                """,
                new { TokenHash = tokenHash },
                cancellationToken: cancellationToken));
    }

    private sealed class TokenRow
    {
        public long AccountId { get; init; }
        public required string TokenHash { get; init; }
    }
}
