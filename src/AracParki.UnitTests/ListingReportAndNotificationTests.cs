using AracParki.Domain.Listings;
using AracParki.Domain.Notifications;

namespace AracParki.UnitTests;

public sealed class ListingReportAndNotificationTests
{
    [Fact]
    public void ReportStatus_LabelsAndBadges_AreSemantic()
    {
        Assert.Equal("Yeni", ListingReportStatus.Label(ListingReportStatus.Open));
        Assert.Equal("İncelemede", ListingReportStatus.Label(ListingReportStatus.Reviewing));
        Assert.Equal("İşlem yapıldı", ListingReportStatus.Label(ListingReportStatus.Actioned));
        Assert.Equal("İşlem yapılmadı", ListingReportStatus.Label(ListingReportStatus.Dismissed));

        Assert.Equal("badge badge-info", ListingReportStatus.BadgeClass(ListingReportStatus.Open));
        Assert.Equal("badge badge-warn", ListingReportStatus.BadgeClass(ListingReportStatus.Reviewing));
        Assert.Equal("badge badge-ok", ListingReportStatus.BadgeClass(ListingReportStatus.Actioned));
        Assert.Equal("badge badge-muted", ListingReportStatus.BadgeClass(ListingReportStatus.Dismissed));

        Assert.True(ListingReportStatus.IsActive(ListingReportStatus.Open));
        Assert.False(ListingReportStatus.IsActive(ListingReportStatus.Actioned));
    }

    [Fact]
    public void ReportReasons_CoverMachineryCases()
    {
        Assert.Equal(5, ListingReportReason.All.Count);
        Assert.True(ListingReportReason.IsKnown(ListingReportReason.SoldOrRented));
        Assert.Contains("Makine", ListingReportReason.Label(ListingReportReason.SoldOrRented), StringComparison.Ordinal);
    }

    [Fact]
    public void NotificationTypes_UseStableDottedCodes()
    {
        Assert.Equal("listing.report.received", NotificationTypes.ListingReportReceived);
        Assert.Equal("listing.report.actioned", NotificationTypes.ListingReportActioned);
        Assert.Equal("listing.report.dismissed", NotificationTypes.ListingReportDismissed);
        Assert.Equal("message.received", NotificationTypes.MessageReceived);
    }
}
