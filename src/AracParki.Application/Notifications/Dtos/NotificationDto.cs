namespace AracParki.Application.Notifications.Dtos;

public sealed class NotificationDto
{
    public long Id { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required string DataJson { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }

    public bool IsUnread => ReadAt is null;
}
