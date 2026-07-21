namespace AracParki.Application.Media;

public sealed class CloudflareMediaSettings
{
    public const string SectionName = "CloudflareMedia";

    /// <summary>
    /// When true, uploads go to the Cloudflare media Worker (no local filesystem).
    /// When false, <see cref="Listings.LocalListingImageStorage"/> path is used for local/dev.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>Media Worker base URL, e.g. https://media.aracparki.com</summary>
    public string WorkerBaseUrl { get; set; } = string.Empty;

    /// <summary>Shared secret for Worker ingest (Authorization: Bearer …).</summary>
    public string IngestSecret { get; set; } = string.Empty;

    /// <summary>
    /// Public origin used in delivery URLs (usually same as WorkerBaseUrl).
    /// Allowed HTTPS hosts for listing images are derived from this.
    /// </summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>Default watermark template code stored in DB / Worker KV.</summary>
    public string DefaultWatermarkCode { get; set; } = "default";

    public bool IsConfigured =>
        Enabled
        && !string.IsNullOrWhiteSpace(WorkerBaseUrl)
        && !string.IsNullOrWhiteSpace(IngestSecret);

    public string ResolvedPublicBaseUrl =>
        string.IsNullOrWhiteSpace(PublicBaseUrl) ? WorkerBaseUrl.TrimEnd('/') : PublicBaseUrl.TrimEnd('/');
}
