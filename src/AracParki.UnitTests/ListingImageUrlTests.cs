using AracParki.Application.Listings;
using AracParki.Application.Media;
using Microsoft.Extensions.Options;

namespace AracParki.UnitTests;

public sealed class ListingImageUrlTests
{
    [Fact]
    public void Allows_local_upload_prefix()
    {
        Assert.True(ListingImageUrl.IsAllowed("/uploads/listings/1/a.jpg"));
    }

    [Fact]
    public void Rejects_localhost_and_private_https()
    {
        Assert.False(ListingImageUrl.IsAllowed("https://127.0.0.1/x.jpg"));
        Assert.False(ListingImageUrl.IsAllowed("https://localhost/x.jpg"));
        Assert.False(ListingImageUrl.IsAllowed("https://192.168.1.10/x.jpg"));
    }

    [Fact]
    public void When_media_configured_restricts_host()
    {
        var media = new CloudflareMediaSettings
        {
            Enabled = true,
            WorkerBaseUrl = "https://media.aracparki.com",
            IngestSecret = "secret",
            PublicBaseUrl = "https://media.aracparki.com"
        };

        Assert.True(ListingImageUrl.IsAllowed("https://media.aracparki.com/m/masters/1/abc/v1?v=card", media));
        Assert.False(ListingImageUrl.IsAllowed("https://cdn.example.com/a.jpg", media));
    }

    [Fact]
    public void Policy_uses_options()
    {
        var policy = new ListingImageUrlPolicy(Options.Create(new CloudflareMediaSettings
        {
            Enabled = true,
            WorkerBaseUrl = "https://img.example.com",
            IngestSecret = "x",
            PublicBaseUrl = "https://img.example.com"
        }));

        Assert.True(policy.IsAllowed("https://img.example.com/m/x?v=card"));
        Assert.False(policy.IsAllowed("https://other.example.com/m/x?v=card"));
    }

    [Fact]
    public void TryGetStorageKey_from_delivery_url()
    {
        Assert.True(ListingImageUrl.TryGetStorageKey(
            "https://media.aracparki.com/m/masters/1/abc/v1?v=card",
            out var key));
        Assert.Equal("masters/1/abc/v1", key);
        Assert.False(ListingImageUrl.TryGetStorageKey("/uploads/listings/1/a.jpg", out _));
        Assert.False(ListingImageUrl.TryGetStorageKey("https://media.aracparki.com/m/../etc/passwd", out _));
    }
}
