using System.Text.Json;
using AracParki.Application.Notifications.Dtos;

namespace AracParki.Application.Notifications;

public interface INotificationService
{
    Task NotifyAsync(
        long accountId,
        string type,
        string title,
        string body,
        IReadOnlyDictionary<string, object?>? data,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationDto>> ListAsync(long accountId, int take, CancellationToken cancellationToken);

    Task<int> CountUnreadAsync(long accountId, CancellationToken cancellationToken);

    Task MarkReadAsync(long accountId, long notificationId, CancellationToken cancellationToken);

    Task MarkAllReadAsync(long accountId, CancellationToken cancellationToken);
}

public sealed class NotificationService(INotificationStore store) : INotificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task NotifyAsync(
        long accountId,
        string type,
        string title,
        string body,
        IReadOnlyDictionary<string, object?>? data,
        CancellationToken cancellationToken)
    {
        if (accountId <= 0)
            throw new ArgumentOutOfRangeException(nameof(accountId));
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        var dataJson = JsonSerializer.Serialize(data ?? new Dictionary<string, object?>(), JsonOptions);
        return store.CreateAsync(
            accountId,
            type.Trim(),
            title.Trim(),
            body.Trim(),
            dataJson,
            cancellationToken);
    }

    public Task<IReadOnlyList<NotificationDto>> ListAsync(
        long accountId,
        int take,
        CancellationToken cancellationToken)
    {
        if (accountId <= 0)
            return Task.FromResult<IReadOnlyList<NotificationDto>>([]);

        return store.ListByAccountAsync(accountId, Math.Clamp(take, 1, 100), cancellationToken);
    }

    public Task<int> CountUnreadAsync(long accountId, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
            return Task.FromResult(0);

        return store.CountUnreadAsync(accountId, cancellationToken);
    }

    public Task MarkReadAsync(long accountId, long notificationId, CancellationToken cancellationToken)
    {
        if (accountId <= 0 || notificationId <= 0)
            return Task.CompletedTask;

        return store.MarkReadAsync(accountId, notificationId, cancellationToken);
    }

    public Task MarkAllReadAsync(long accountId, CancellationToken cancellationToken)
    {
        if (accountId <= 0)
            return Task.CompletedTask;

        return store.MarkAllReadAsync(accountId, cancellationToken);
    }
}
