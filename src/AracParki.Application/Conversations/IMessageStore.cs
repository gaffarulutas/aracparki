using AracParki.Application.Conversations.Dtos;

namespace AracParki.Application.Conversations;

public interface IMessageStore
{
    Task<long> GetOrCreateThreadAsync(
        long listingId,
        long buyerAccountId,
        long sellerAccountId,
        CancellationToken cancellationToken);

    Task<MessageThreadDetailDto?> GetThreadForAccountAsync(
        long threadId,
        long accountId,
        CancellationToken cancellationToken);

    Task<MessageThreadMetaDto?> GetThreadMetaForAccountAsync(
        long threadId,
        long accountId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MessageThreadListItemDto>> ListThreadsForAccountAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken);

    /// <summary>Returns null when the account is not a participant; empty list when allowed but no rows.</summary>
    Task<IReadOnlyList<MessageDto>?> ListMessagesAsync(
        long threadId,
        long accountId,
        long? afterId,
        int take,
        CancellationToken cancellationToken);

    Task<long> InsertMessageAsync(
        long threadId,
        long senderAccountId,
        string body,
        CancellationToken cancellationToken);

    Task MarkReadAsync(long threadId, long accountId, CancellationToken cancellationToken);

    Task<int> CountUnreadThreadsAsync(long accountId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UnreadMessageAlertDto>> ListUnreadIncomingAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken);
}
