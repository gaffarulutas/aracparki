using AracParki.Application.Conversations.Dtos;
using AracParki.Application.Listings;
using AracParki.Application.Notifications;
using AracParki.Domain.Listings;
using AracParki.Domain.Notifications;

namespace AracParki.Application.Conversations.Services;

public sealed class MessagingService(
    IMessageStore store,
    IListingQuery listings,
    INotificationService notifications)
{
    public const int BodyMaxLength = 2000;

    public async Task<long> StartOrGetThreadAsync(
        string adNo,
        long buyerAccountId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adNo);
        if (buyerAccountId <= 0)
            throw new ArgumentOutOfRangeException(nameof(buyerAccountId));

        var listing = await listings.GetByAdNoAsync(
            adNo.Trim(),
            ListingAccessContext.Anonymous,
            cancellationToken);
        if (listing is null || listing.Status != ListingStatus.Published)
            throw new InvalidOperationException("İlan bulunamadı veya yayında değil.");

        if (listing.ExpiresAt is DateTimeOffset exp && exp <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("İlanın süresi dolmuş.");

        if (listing.OwnerAccountId is not long sellerId || sellerId <= 0)
            throw new InvalidOperationException("Bu ilan için mesaj gönderilemiyor.");

        if (sellerId == buyerAccountId)
            throw new InvalidOperationException("Kendi ilanınıza mesaj gönderemezsiniz.");

        return await store.GetOrCreateThreadAsync(listing.Id, buyerAccountId, sellerId, cancellationToken);
    }

    public Task<IReadOnlyList<MessageThreadListItemDto>> ListThreadsAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken)
    {
        if (accountId <= 0)
            return Task.FromResult<IReadOnlyList<MessageThreadListItemDto>>([]);

        return store.ListThreadsForAccountAsync(accountId, Math.Clamp(take, 1, 100), cancellationToken);
    }

    public async Task<MessageThreadDetailDto?> GetThreadAsync(
        long threadId,
        long accountId,
        CancellationToken cancellationToken)
    {
        if (threadId <= 0 || accountId <= 0)
            return null;

        var thread = await store.GetThreadForAccountAsync(threadId, accountId, cancellationToken);
        if (thread is null)
            return null;

        await store.MarkReadAsync(threadId, accountId, cancellationToken);
        return thread;
    }

    public Task<IReadOnlyList<MessageDto>?> ListMessagesAsync(
        long threadId,
        long accountId,
        long? afterId,
        CancellationToken cancellationToken)
    {
        if (threadId <= 0 || accountId <= 0)
            return Task.FromResult<IReadOnlyList<MessageDto>?>(null);

        return store.ListMessagesAsync(threadId, accountId, afterId, 200, cancellationToken);
    }

    public async Task<long> SendAsync(
        long threadId,
        long senderAccountId,
        string body,
        CancellationToken cancellationToken)
    {
        if (threadId <= 0)
            throw new ArgumentOutOfRangeException(nameof(threadId));
        if (senderAccountId <= 0)
            throw new ArgumentOutOfRangeException(nameof(senderAccountId));

        var normalized = NormalizeBody(body);
        var thread = await store.GetThreadMetaForAccountAsync(threadId, senderAccountId, cancellationToken)
            ?? throw new InvalidOperationException("Konuşma bulunamadı.");

        var messageId = await store.InsertMessageAsync(threadId, senderAccountId, normalized, cancellationToken);
        await store.MarkReadAsync(threadId, senderAccountId, cancellationToken);

        var recipientId = senderAccountId == thread.BuyerAccountId
            ? thread.SellerAccountId
            : thread.BuyerAccountId;

        var preview = normalized.Length > 120 ? normalized[..117] + "…" : normalized;
        await notifications.NotifyAsync(
            recipientId,
            NotificationTypes.MessageReceived,
            "Yeni mesaj",
            $"{thread.AdNo}: {preview}",
            new Dictionary<string, object?>
            {
                ["threadId"] = threadId,
                ["adNo"] = thread.AdNo,
                ["url"] = $"/mesajlarim/{threadId}"
            },
            cancellationToken);

        return messageId;
    }

    public Task MarkReadAsync(long threadId, long accountId, CancellationToken cancellationToken)
    {
        if (threadId <= 0 || accountId <= 0)
            return Task.CompletedTask;

        return store.MarkReadAsync(threadId, accountId, cancellationToken);
    }

    public Task<int> CountUnreadThreadsAsync(long accountId, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
            return Task.FromResult(0);

        return store.CountUnreadThreadsAsync(accountId, cancellationToken);
    }

    public Task<IReadOnlyList<UnreadMessageAlertDto>> ListUnreadIncomingAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken)
    {
        if (accountId <= 0)
            return Task.FromResult<IReadOnlyList<UnreadMessageAlertDto>>([]);

        return store.ListUnreadIncomingAsync(accountId, Math.Clamp(take, 1, 50), cancellationToken);
    }

    private static string NormalizeBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new InvalidOperationException("Mesaj boş olamaz.");

        var normalized = body.Replace("\r\n", "\n", StringComparison.Ordinal);
        // Keep \n and \t; drop other C0 controls (incl. NUL) that break clients / logs.
        Span<char> buffer = normalized.Length <= 2048
            ? stackalloc char[normalized.Length]
            : new char[normalized.Length];
        var n = 0;
        foreach (var ch in normalized)
        {
            if (ch is '\n' or '\t' || ch >= ' ')
                buffer[n++] = ch;
        }

        var trimmed = new string(buffer[..n]).Trim();
        if (trimmed.Length == 0)
            throw new InvalidOperationException("Mesaj boş olamaz.");
        if (trimmed.Length > BodyMaxLength)
            throw new InvalidOperationException($"Mesaj en fazla {BodyMaxLength} karakter olabilir.");

        return trimmed;
    }
}
