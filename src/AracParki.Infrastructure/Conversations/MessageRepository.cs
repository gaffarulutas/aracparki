using AracParki.Application.Abstractions;
using AracParki.Application.Conversations;
using AracParki.Application.Conversations.Dtos;
using Dapper;

namespace AracParki.Infrastructure.Conversations;

public sealed class MessageRepository(IDbConnectionFactory connectionFactory) : IMessageStore
{
    public async Task<long> GetOrCreateThreadAsync(
        long listingId,
        long buyerAccountId,
        long sellerAccountId,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                """
                INSERT INTO message_threads (listing_id, buyer_account_id, seller_account_id)
                VALUES (@ListingId, @BuyerAccountId, @SellerAccountId)
                ON CONFLICT (listing_id, buyer_account_id) DO UPDATE
                    SET listing_id = EXCLUDED.listing_id
                RETURNING id
                """,
                new
                {
                    ListingId = listingId,
                    BuyerAccountId = buyerAccountId,
                    SellerAccountId = sellerAccountId
                },
                cancellationToken: cancellationToken));
    }

    public async Task<MessageThreadDetailDto?> GetThreadForAccountAsync(
        long threadId,
        long accountId,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<ThreadRow>(
            new CommandDefinition(
                """
                SELECT
                    t.id AS Id,
                    t.listing_id AS ListingId,
                    l.ad_no AS AdNo,
                    l.title AS ListingTitle,
                    l.cover_image_url AS CoverImageUrl,
                    l.price AS Price,
                    l.currency AS Currency,
                    l.price_unit AS PriceUnit,
                    l.primary_intent AS PrimaryIntent,
                    l.status AS ListingStatus,
                    t.buyer_account_id AS BuyerAccountId,
                    t.seller_account_id AS SellerAccountId,
                    CASE
                        WHEN t.buyer_account_id = @AccountId
                            THEN TRIM(COALESCE(sa.first_name, '') || ' ' || COALESCE(sa.last_name, ''))
                        ELSE TRIM(COALESCE(ba.first_name, '') || ' ' || COALESCE(ba.last_name, ''))
                    END AS CounterpartyName
                FROM message_threads t
                JOIN listings l ON l.id = t.listing_id
                JOIN accounts ba ON ba.id = t.buyer_account_id
                JOIN accounts sa ON sa.id = t.seller_account_id
                WHERE t.id = @ThreadId
                  AND (t.buyer_account_id = @AccountId OR t.seller_account_id = @AccountId)
                """,
                new { ThreadId = threadId, AccountId = accountId },
                cancellationToken: cancellationToken));

        if (row is null)
            return null;

        var messages = (await connection.QueryAsync<MessageRow>(
            new CommandDefinition(
                """
                SELECT
                    id AS Id,
                    thread_id AS ThreadId,
                    sender_account_id AS SenderAccountId,
                    body AS Body,
                    created_at AS CreatedAt
                FROM messages
                WHERE thread_id = @ThreadId
                ORDER BY created_at ASC, id ASC
                LIMIT 500
                """,
                new { ThreadId = threadId },
                cancellationToken: cancellationToken))).AsList();

        var amBuyer = row.BuyerAccountId == accountId;
        var counterparty = string.IsNullOrWhiteSpace(row.CounterpartyName) ? "Kullanıcı" : row.CounterpartyName.Trim();

        return new MessageThreadDetailDto
        {
            Id = row.Id,
            ListingId = row.ListingId,
            AdNo = row.AdNo,
            ListingTitle = row.ListingTitle,
            CoverImageUrl = row.CoverImageUrl ?? "",
            Price = row.Price,
            Currency = row.Currency,
            PriceUnit = row.PriceUnit,
            PrimaryIntent = row.PrimaryIntent,
            ListingStatus = row.ListingStatus,
            BuyerAccountId = row.BuyerAccountId,
            SellerAccountId = row.SellerAccountId,
            CounterpartyName = counterparty,
            AmBuyer = amBuyer,
            Messages = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ThreadId = m.ThreadId,
                SenderAccountId = m.SenderAccountId,
                Body = m.Body,
                CreatedAt = m.CreatedAt,
                IsMine = m.SenderAccountId == accountId
            }).ToArray()
        };
    }

    public async Task<MessageThreadMetaDto?> GetThreadMetaForAccountAsync(
        long threadId,
        long accountId,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<MessageThreadMetaDto>(
            new CommandDefinition(
                """
                SELECT
                    t.id AS Id,
                    t.buyer_account_id AS BuyerAccountId,
                    t.seller_account_id AS SellerAccountId,
                    l.ad_no AS AdNo
                FROM message_threads t
                JOIN listings l ON l.id = t.listing_id
                WHERE t.id = @ThreadId
                  AND (t.buyer_account_id = @AccountId OR t.seller_account_id = @AccountId)
                """,
                new { ThreadId = threadId, AccountId = accountId },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<MessageThreadListItemDto>> ListThreadsForAccountAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<ThreadListRow>(
            new CommandDefinition(
                """
                SELECT
                    t.id AS Id,
                    t.listing_id AS ListingId,
                    l.ad_no AS AdNo,
                    l.title AS ListingTitle,
                    l.cover_image_url AS CoverImageUrl,
                    l.price AS Price,
                    l.currency AS Currency,
                    l.price_unit AS PriceUnit,
                    l.primary_intent AS PrimaryIntent,
                    CASE
                        WHEN t.buyer_account_id = @AccountId
                            THEN TRIM(COALESCE(sa.first_name, '') || ' ' || COALESCE(sa.last_name, ''))
                        ELSE TRIM(COALESCE(ba.first_name, '') || ' ' || COALESCE(ba.last_name, ''))
                    END AS CounterpartyName,
                    (
                        SELECT LEFT(m.body, 160)
                        FROM messages m
                        WHERE m.thread_id = t.id
                        ORDER BY m.created_at DESC, m.id DESC
                        LIMIT 1
                    ) AS LastMessagePreview,
                    t.last_message_at AS LastMessageAt,
                    (t.buyer_account_id = @AccountId) AS AmBuyer,
                    EXISTS (
                        SELECT 1
                        FROM messages m
                        WHERE m.thread_id = t.id
                          AND m.sender_account_id <> @AccountId
                          AND m.created_at > COALESCE(
                              CASE WHEN t.buyer_account_id = @AccountId
                                   THEN t.buyer_last_read_at
                                   ELSE t.seller_last_read_at END,
                              TIMESTAMPTZ 'epoch')
                    ) AS IsUnread
                FROM message_threads t
                JOIN listings l ON l.id = t.listing_id
                JOIN accounts ba ON ba.id = t.buyer_account_id
                JOIN accounts sa ON sa.id = t.seller_account_id
                WHERE t.buyer_account_id = @AccountId
                   OR t.seller_account_id = @AccountId
                ORDER BY COALESCE(t.last_message_at, t.created_at) DESC
                LIMIT @Take
                """,
                new { AccountId = accountId, Take = take },
                cancellationToken: cancellationToken));

        return rows.Select(r => new MessageThreadListItemDto
        {
            Id = r.Id,
            ListingId = r.ListingId,
            AdNo = r.AdNo,
            ListingTitle = r.ListingTitle,
            CoverImageUrl = r.CoverImageUrl ?? "",
            Price = r.Price,
            Currency = r.Currency,
            PriceUnit = r.PriceUnit,
            PrimaryIntent = r.PrimaryIntent,
            CounterpartyName = string.IsNullOrWhiteSpace(r.CounterpartyName) ? "Kullanıcı" : r.CounterpartyName.Trim(),
            LastMessagePreview = r.LastMessagePreview,
            LastMessageAt = r.LastMessageAt,
            IsUnread = r.IsUnread,
            AmBuyer = r.AmBuyer
        }).ToArray();
    }

    public async Task<IReadOnlyList<MessageDto>?> ListMessagesAsync(
        long threadId,
        long accountId,
        long? afterId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var allowed = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT EXISTS (
                    SELECT 1 FROM message_threads
                    WHERE id = @ThreadId
                      AND (buyer_account_id = @AccountId OR seller_account_id = @AccountId)
                )
                """,
                new { ThreadId = threadId, AccountId = accountId },
                cancellationToken: cancellationToken));
        if (!allowed)
            return null;

        var rows = await connection.QueryAsync<MessageRow>(
            new CommandDefinition(
                """
                SELECT
                    id AS Id,
                    thread_id AS ThreadId,
                    sender_account_id AS SenderAccountId,
                    body AS Body,
                    created_at AS CreatedAt
                FROM messages
                WHERE thread_id = @ThreadId
                  AND (@AfterId IS NULL OR id > @AfterId)
                ORDER BY created_at ASC, id ASC
                LIMIT @Take
                """,
                new { ThreadId = threadId, AfterId = afterId, Take = take },
                cancellationToken: cancellationToken));

        return rows.Select(m => new MessageDto
        {
            Id = m.Id,
            ThreadId = m.ThreadId,
            SenderAccountId = m.SenderAccountId,
            Body = m.Body,
            CreatedAt = m.CreatedAt,
            IsMine = m.SenderAccountId == accountId
        }).ToArray();
    }

    public async Task<long> InsertMessageAsync(
        long threadId,
        long senderAccountId,
        string body,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var tx = await connection.BeginTransactionAsync(cancellationToken);

        var messageId = await connection.ExecuteScalarAsync<long?>(
            new CommandDefinition(
                """
                INSERT INTO messages (thread_id, sender_account_id, body)
                SELECT @ThreadId, @SenderAccountId, @Body
                FROM message_threads
                WHERE id = @ThreadId
                  AND (buyer_account_id = @SenderAccountId OR seller_account_id = @SenderAccountId)
                RETURNING id
                """,
                new { ThreadId = threadId, SenderAccountId = senderAccountId, Body = body },
                transaction: tx,
                cancellationToken: cancellationToken));

        if (messageId is null or <= 0)
            throw new InvalidOperationException("Konuşma bulunamadı.");

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE message_threads
                SET last_message_at = NOW()
                WHERE id = @ThreadId
                """,
                new { ThreadId = threadId },
                transaction: tx,
                cancellationToken: cancellationToken));

        await tx.CommitAsync(cancellationToken);
        return messageId.Value;
    }

    public async Task MarkReadAsync(long threadId, long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE message_threads
                SET buyer_last_read_at = CASE WHEN buyer_account_id = @AccountId THEN NOW() ELSE buyer_last_read_at END,
                    seller_last_read_at = CASE WHEN seller_account_id = @AccountId THEN NOW() ELSE seller_last_read_at END
                WHERE id = @ThreadId
                  AND (buyer_account_id = @AccountId OR seller_account_id = @AccountId)
                """,
                new { ThreadId = threadId, AccountId = accountId },
                cancellationToken: cancellationToken));
    }

    public async Task<int> CountUnreadThreadsAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                SELECT COUNT(*)::int
                FROM message_threads t
                WHERE (t.buyer_account_id = @AccountId OR t.seller_account_id = @AccountId)
                  AND EXISTS (
                      SELECT 1
                      FROM messages m
                      WHERE m.thread_id = t.id
                        AND m.sender_account_id <> @AccountId
                        AND m.created_at > COALESCE(
                            CASE WHEN t.buyer_account_id = @AccountId
                                 THEN t.buyer_last_read_at
                                 ELSE t.seller_last_read_at END,
                            TIMESTAMPTZ 'epoch')
                  )
                """,
                new { AccountId = accountId },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<UnreadMessageAlertDto>> ListUnreadIncomingAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<UnreadAlertRow>(
            new CommandDefinition(
                """
                SELECT
                    m.id AS MessageId,
                    m.thread_id AS ThreadId,
                    l.ad_no AS AdNo,
                    LEFT(m.body, 160) AS BodyPreview,
                    m.created_at AS CreatedAt
                FROM messages m
                JOIN message_threads t ON t.id = m.thread_id
                JOIN listings l ON l.id = t.listing_id
                WHERE (t.buyer_account_id = @AccountId OR t.seller_account_id = @AccountId)
                  AND m.sender_account_id <> @AccountId
                  AND m.created_at > COALESCE(
                      CASE WHEN t.buyer_account_id = @AccountId
                           THEN t.buyer_last_read_at
                           ELSE t.seller_last_read_at END,
                      TIMESTAMPTZ 'epoch')
                ORDER BY m.id DESC
                LIMIT @Take
                """,
                new { AccountId = accountId, Take = take },
                cancellationToken: cancellationToken));

        return rows.Select(r => new UnreadMessageAlertDto
        {
            MessageId = r.MessageId,
            ThreadId = r.ThreadId,
            AdNo = r.AdNo,
            BodyPreview = r.BodyPreview ?? "",
            CreatedAt = r.CreatedAt
        }).ToArray();
    }

    private sealed class UnreadAlertRow
    {
        public long MessageId { get; init; }
        public long ThreadId { get; init; }
        public required string AdNo { get; init; }
        public string? BodyPreview { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    private sealed class ThreadRow
    {
        public long Id { get; init; }
        public long ListingId { get; init; }
        public required string AdNo { get; init; }
        public required string ListingTitle { get; init; }
        public string? CoverImageUrl { get; init; }
        public decimal Price { get; init; }
        public required string Currency { get; init; }
        public string? PriceUnit { get; init; }
        public required string PrimaryIntent { get; init; }
        public required string ListingStatus { get; init; }
        public long BuyerAccountId { get; init; }
        public long SellerAccountId { get; init; }
        public string? CounterpartyName { get; init; }
    }

    private sealed class ThreadListRow
    {
        public long Id { get; init; }
        public long ListingId { get; init; }
        public required string AdNo { get; init; }
        public required string ListingTitle { get; init; }
        public string? CoverImageUrl { get; init; }
        public decimal Price { get; init; }
        public required string Currency { get; init; }
        public string? PriceUnit { get; init; }
        public required string PrimaryIntent { get; init; }
        public string? CounterpartyName { get; init; }
        public string? LastMessagePreview { get; init; }
        public DateTimeOffset? LastMessageAt { get; init; }
        public bool AmBuyer { get; init; }
        public bool IsUnread { get; init; }
    }

    private sealed class MessageRow
    {
        public long Id { get; init; }
        public long ThreadId { get; init; }
        public long SenderAccountId { get; init; }
        public required string Body { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
