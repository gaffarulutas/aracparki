namespace AracParki.Application.Conversations.Dtos;

/// <summary>Lightweight thread identity for send / ACL without loading message history.</summary>
public sealed class MessageThreadMetaDto
{
    public long Id { get; init; }
    public long BuyerAccountId { get; init; }
    public long SellerAccountId { get; init; }
    public required string AdNo { get; init; }
}

public sealed class MessageDto
{
    public long Id { get; init; }
    public long ThreadId { get; init; }
    public long SenderAccountId { get; init; }
    public required string Body { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsMine { get; init; }
}

public sealed class MessageThreadListItemDto
{
    public long Id { get; init; }
    public long ListingId { get; init; }
    public required string AdNo { get; init; }
    public required string ListingTitle { get; init; }
    public required string CoverImageUrl { get; init; }
    public decimal Price { get; init; }
    public required string Currency { get; init; }
    public string? PriceUnit { get; init; }
    public required string PrimaryIntent { get; init; }
    public required string CounterpartyName { get; init; }
    public string? LastMessagePreview { get; init; }
    public DateTimeOffset? LastMessageAt { get; init; }
    public bool IsUnread { get; init; }
    public bool AmBuyer { get; init; }
}

public sealed class MessageThreadDetailDto
{
    public long Id { get; init; }
    public long ListingId { get; init; }
    public required string AdNo { get; init; }
    public required string ListingTitle { get; init; }
    public required string CoverImageUrl { get; init; }
    public decimal Price { get; init; }
    public required string Currency { get; init; }
    public string? PriceUnit { get; init; }
    public required string PrimaryIntent { get; init; }
    public required string ListingStatus { get; init; }
    public long BuyerAccountId { get; init; }
    public long SellerAccountId { get; init; }
    public required string CounterpartyName { get; init; }
    public bool AmBuyer { get; init; }
    public IReadOnlyList<MessageDto> Messages { get; init; } = [];
}

/// <summary>Unread inbound message for nav badges / mobile OS alerts.</summary>
public sealed class UnreadMessageAlertDto
{
    public long MessageId { get; init; }
    public long ThreadId { get; init; }
    public required string AdNo { get; init; }
    public required string BodyPreview { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
