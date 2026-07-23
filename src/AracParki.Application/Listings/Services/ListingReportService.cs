using AracParki.Application.Listings.Dtos;
using AracParki.Application.Notifications;
using AracParki.Domain.Listings;
using AracParki.Domain.Notifications;

namespace AracParki.Application.Listings.Services;

public sealed class ListingReportService(
    IListingReportStore store,
    INotificationService notifications)
{
    public const int MessageMaxLength = 250;
    public const int AdminNotesMaxLength = 1000;

    public async Task<long> CreateAsync(
        long listingId,
        string adNo,
        long reporterAccountId,
        long? ownerAccountId,
        string reasonCode,
        string? message,
        CancellationToken cancellationToken)
    {
        if (listingId <= 0)
            throw new ArgumentOutOfRangeException(nameof(listingId));
        ArgumentException.ThrowIfNullOrWhiteSpace(adNo);
        if (reporterAccountId <= 0)
            throw new ArgumentOutOfRangeException(nameof(reporterAccountId));
        if (ownerAccountId is long owner && owner == reporterAccountId)
            throw new InvalidOperationException("Kendi ilanınızı şikayet edemezsiniz.");
        if (!ListingReportReason.IsKnown(reasonCode))
            throw new ArgumentException("Geçerli bir şikayet nedeni seçin.", nameof(reasonCode));

        var trimmedMessage = NormalizeOptional(message, MessageMaxLength, "Açıklama");
        var normalizedAdNo = adNo.Trim();

        if (await store.HasActiveReportAsync(reporterAccountId, listingId, cancellationToken))
            throw new InvalidOperationException("Bu ilan için zaten açık bir şikayetiniz var.");

        var id = await store.CreateAsync(
            listingId,
            normalizedAdNo,
            reporterAccountId,
            reasonCode.Trim(),
            trimmedMessage,
            cancellationToken);

        await notifications.NotifyAsync(
            reporterAccountId,
            NotificationTypes.ListingReportReceived,
            "Şikayetiniz alındı",
            $"{normalizedAdNo} numaralı ilan için şikayetiniz incelemeye alındı.",
            new Dictionary<string, object?>
            {
                ["reportId"] = id,
                ["adNo"] = normalizedAdNo,
                ["url"] = $"/ilan/{normalizedAdNo}"
            },
            cancellationToken);

        return id;
    }

    public Task<ListingReportDetailDto?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Task.FromResult<ListingReportDetailDto?>(null);

        return store.GetByIdAsync(id, cancellationToken);
    }

    public Task<IReadOnlyList<ListingReportListItemDto>> ListAsync(
        string? status,
        int take,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeStatus(status);
        return store.ListAsync(normalized, Math.Clamp(take, 1, 100), cancellationToken);
    }

    public Task<ListingReportCountsDto> GetCountsAsync(CancellationToken cancellationToken)
        => store.GetCountsAsync(cancellationToken);

    public async Task MarkReviewingAsync(
        long id,
        long adminAccountId,
        string? adminNotes,
        CancellationToken cancellationToken)
    {
        await SetStatusAsync(id, ListingReportStatus.Reviewing, adminAccountId, adminNotes, cancellationToken);
    }

    public async Task MarkActionedAsync(
        long id,
        long adminAccountId,
        string? adminNotes,
        CancellationToken cancellationToken)
    {
        var (report, shouldNotify) = await SetStatusAsync(
            id,
            ListingReportStatus.Actioned,
            adminAccountId,
            adminNotes,
            cancellationToken);

        if (!shouldNotify)
            return;

        await notifications.NotifyAsync(
            report.ReporterAccountId,
            NotificationTypes.ListingReportActioned,
            "Şikayetiniz sonuçlandı",
            $"{report.AdNo} numaralı ilan için şikayetinizde işlem yapıldı.",
            new Dictionary<string, object?>
            {
                ["reportId"] = report.Id,
                ["adNo"] = report.AdNo,
                ["url"] = $"/ilan/{report.AdNo}",
                ["status"] = ListingReportStatus.Actioned
            },
            cancellationToken);
    }

    public async Task MarkDismissedAsync(
        long id,
        long adminAccountId,
        string? adminNotes,
        CancellationToken cancellationToken)
    {
        var (report, shouldNotify) = await SetStatusAsync(
            id,
            ListingReportStatus.Dismissed,
            adminAccountId,
            adminNotes,
            cancellationToken);

        if (!shouldNotify)
            return;

        await notifications.NotifyAsync(
            report.ReporterAccountId,
            NotificationTypes.ListingReportDismissed,
            "Şikayetiniz değerlendirildi",
            $"{report.AdNo} numaralı ilan için şikayetinizde işlem yapılmadı.",
            new Dictionary<string, object?>
            {
                ["reportId"] = report.Id,
                ["adNo"] = report.AdNo,
                ["url"] = $"/ilan/{report.AdNo}",
                ["status"] = ListingReportStatus.Dismissed
            },
            cancellationToken);
    }

    private async Task<(ListingReportDetailDto Report, bool ShouldNotifyReporter)> SetStatusAsync(
        long id,
        string status,
        long adminAccountId,
        string? adminNotes,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        if (adminAccountId <= 0)
            throw new ArgumentOutOfRangeException(nameof(adminAccountId));
        if (!ListingReportStatus.IsKnown(status))
            throw new ArgumentException("Geçersiz durum.", nameof(status));

        var notes = NormalizeOptional(adminNotes, AdminNotesMaxLength, "Admin notu");

        var existing = await store.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Şikayet bulunamadı.");

        if (existing.Status == status
            && string.Equals(existing.AdminNotes ?? "", notes ?? "", StringComparison.Ordinal))
        {
            return (existing, false);
        }

        if (!ListingReportStatus.IsActive(existing.Status) && existing.Status != status)
        {
            throw new InvalidOperationException("Bu şikayet zaten sonuçlandırılmış.");
        }

        var wasActive = ListingReportStatus.IsActive(existing.Status);
        var updated = await store.UpdateStatusAsync(id, status, adminAccountId, notes, cancellationToken);
        if (!updated)
            throw new InvalidOperationException("Şikayet güncellenemedi.");

        var report = await store.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Şikayet bulunamadı.");

        var shouldNotify = wasActive
            && status is ListingReportStatus.Actioned or ListingReportStatus.Dismissed;

        return (report, shouldNotify);
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = string.IsNullOrWhiteSpace(status)
            ? ListingReportStatus.Open
            : status.Trim();
        return ListingReportStatus.IsKnown(normalized)
            ? normalized
            : ListingReportStatus.Open;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{label} en fazla {maxLength} karakter olabilir.");

        return trimmed;
    }
}
