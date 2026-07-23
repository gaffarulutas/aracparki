using AracParki.Application.Abstractions;
using AracParki.Application.Notifications;
using AracParki.Application.Notifications.Dtos;
using Dapper;

namespace AracParki.Infrastructure.Notifications;

public sealed class NotificationRepository(IDbConnectionFactory connectionFactory) : INotificationStore
{
    public async Task<long> CreateAsync(
        long accountId,
        string type,
        string title,
        string body,
        string dataJson,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                """
                INSERT INTO notifications (account_id, type, title, body, data)
                VALUES (@AccountId, @Type, @Title, @Body, CAST(@DataJson AS jsonb))
                RETURNING id
                """,
                new
                {
                    AccountId = accountId,
                    Type = type,
                    Title = title,
                    Body = body,
                    DataJson = dataJson
                },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<NotificationDto>> ListByAccountAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<NotificationDto>(
            new CommandDefinition(
                """
                SELECT
                    id AS Id,
                    type AS Type,
                    title AS Title,
                    body AS Body,
                    data::text AS DataJson,
                    created_at AS CreatedAt,
                    read_at AS ReadAt
                FROM notifications
                WHERE account_id = @AccountId
                ORDER BY created_at DESC
                LIMIT @Take
                """,
                new { AccountId = accountId, Take = take },
                cancellationToken: cancellationToken));

        return rows.ToArray();
    }

    public async Task<int> CountUnreadAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                SELECT COUNT(*)::int
                FROM notifications
                WHERE account_id = @AccountId
                  AND read_at IS NULL
                """,
                new { AccountId = accountId },
                cancellationToken: cancellationToken));
    }

    public async Task<bool> MarkReadAsync(long accountId, long notificationId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE notifications
                SET read_at = NOW()
                WHERE id = @Id
                  AND account_id = @AccountId
                  AND read_at IS NULL
                """,
                new { Id = notificationId, AccountId = accountId },
                cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<int> MarkAllReadAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE notifications
                SET read_at = NOW()
                WHERE account_id = @AccountId
                  AND read_at IS NULL
                """,
                new { AccountId = accountId },
                cancellationToken: cancellationToken));
    }
}
