using AracParki.Application.Notifications.Dtos;

namespace AracParki.Application.Notifications;

public interface INotificationStore
{
    Task<long> CreateAsync(
        long accountId,
        string type,
        string title,
        string body,
        string dataJson,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationDto>> ListByAccountAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountUnreadAsync(long accountId, CancellationToken cancellationToken);

    Task<bool> MarkReadAsync(long accountId, long notificationId, CancellationToken cancellationToken);

    Task<int> MarkAllReadAsync(long accountId, CancellationToken cancellationToken);
}
