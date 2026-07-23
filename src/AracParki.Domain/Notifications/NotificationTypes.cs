namespace AracParki.Domain.Notifications;

/// <summary>Stable notification type codes for web + future mobile clients.</summary>
public static class NotificationTypes
{
    public const string ListingReportReceived = "listing.report.received";
    public const string ListingReportActioned = "listing.report.actioned";
    public const string ListingReportDismissed = "listing.report.dismissed";
}
