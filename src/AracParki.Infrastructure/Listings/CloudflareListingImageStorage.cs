using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using AracParki.Application.Listings;
using AracParki.Application.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AracParki.Infrastructure.Listings;

/// <summary>
/// Streams image bytes to the Cloudflare media Worker. Never writes to the local filesystem.
/// </summary>
public sealed class CloudflareListingImageStorage(
    IHttpClientFactory httpClientFactory,
    IOptions<CloudflareMediaSettings> options,
    ILogger<CloudflareListingImageStorage> logger) : IListingImageStorage
{
    public const string HttpClientName = "CloudflareMedia";

    public async Task<ListingImageSaveResult> SaveAsync(
        long accountId,
        Stream content,
        string contentType,
        string? originalFilename,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
        {
            throw new InvalidOperationException("CloudflareMedia is enabled but not fully configured.");
        }

        if (!ListingImageUrl.IsAllowedContentType(contentType))
        {
            throw new InvalidOperationException("Desteklenmeyen görsel formatı.");
        }

        // Buffer so Content-Length is known (Workers formData rejects chunked multipart).
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length == 0)
        {
            throw new InvalidOperationException("Dosya seçilmedi.");
        }

        buffer.Position = 0;
        var safeFilename = SanitizeMultipartFilename(originalFilename);

        using var form = new MultipartFormDataContent();
        // Cloudflare Workers' formData() requires quoted name/filename and rejects .NET's
        // default unquoted disposition (+ filename* / MIME encoded-word).
        var fileContent = new StreamContent(buffer);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        fileContent.Headers.ContentDisposition = BuildQuotedFileDisposition("file", safeFilename);
        form.Add(fileContent);

        var accountContent = new StringContent(accountId.ToString(), Encoding.UTF8);
        accountContent.Headers.ContentDisposition = BuildQuotedFieldDisposition("accountId");
        form.Add(accountContent);

        if (!string.IsNullOrWhiteSpace(originalFilename))
        {
            var nameContent = new StringContent(originalFilename.Trim(), Encoding.UTF8);
            nameContent.Headers.ContentDisposition = BuildQuotedFieldDisposition("originalFilename");
            form.Add(nameContent);
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/ingest") { Content = form };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.IngestSecret);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning(
                "Cloudflare media ingest failed ({Status}): {Body}",
                (int)response.StatusCode,
                body.Length > 500 ? body[..500] : body);
            throw new InvalidOperationException(MapIngestError(response.StatusCode, body));
        }

        var payload = await response.Content.ReadFromJsonAsync<IngestResponse>(cancellationToken)
                      ?? throw new InvalidOperationException("Media Worker boş yanıt döndü.");

        if (string.IsNullOrWhiteSpace(payload.DeliveryUrl)
            || string.IsNullOrWhiteSpace(payload.ImageId)
            || string.IsNullOrWhiteSpace(payload.StorageKey))
        {
            throw new InvalidOperationException("Media Worker eksik alan döndü.");
        }

        logger.LogInformation(
            "Ingested listing image {ImageId} for account {AccountId} ({Width}x{Height})",
            payload.ImageId,
            accountId,
            payload.Width,
            payload.Height);

        return new ListingImageSaveResult(
            DeliveryUrl: payload.DeliveryUrl,
            ImageId: payload.ImageId,
            StorageKey: payload.StorageKey,
            Version: payload.Version <= 0 ? 1 : payload.Version,
            Width: payload.Width,
            Height: payload.Height,
            ByteSize: payload.ByteSize,
            MimeType: string.IsNullOrWhiteSpace(payload.MimeType) ? "image/jpeg" : payload.MimeType,
            ChecksumSha256: payload.ChecksumSha256 ?? string.Empty,
            OriginalFilename: originalFilename);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
        {
            throw new InvalidOperationException("CloudflareMedia is enabled but not fully configured.");
        }

        if (string.IsNullOrWhiteSpace(storageKey)
            || storageKey.Contains("..", StringComparison.Ordinal)
            || !storageKey.StartsWith("masters/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Geçersiz depolama anahtarı.");
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/delete")
        {
            Content = JsonContent.Create(new { storageKey })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.IngestSecret);

        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation("Media object already absent: {StorageKey}", storageKey);
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning(
                "Cloudflare media delete failed ({Status}): {Body}",
                (int)response.StatusCode,
                body.Length > 300 ? body[..300] : body);
            throw new InvalidOperationException("Görsel depodan silinemedi.");
        }

        logger.LogInformation("Hard-deleted media object {StorageKey}", storageKey);
    }

    /// <summary>
    /// Workers multipart parser requires quoted name/filename tokens (browser-style).
    /// </summary>
    private static ContentDispositionHeaderValue BuildQuotedFileDisposition(string name, string fileName)
    {
        // Force raw quoted values; do not let Name/FileName setters emit filename* or MIME words.
        return ContentDispositionHeaderValue.Parse(
            $"form-data; name=\"{EscapeDispositionToken(name)}\"; filename=\"{EscapeDispositionToken(fileName)}\"");
    }

    private static ContentDispositionHeaderValue BuildQuotedFieldDisposition(string name)
    {
        return ContentDispositionHeaderValue.Parse(
            $"form-data; name=\"{EscapeDispositionToken(name)}\"");
    }

    private static string EscapeDispositionToken(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    /// <summary>
    /// Keep multipart filename ASCII-safe; original name is sent as a separate text field.
    /// </summary>
    private static string SanitizeMultipartFilename(string? originalFilename)
    {
        var name = Path.GetFileName(originalFilename?.Trim() ?? string.Empty);
        if (string.IsNullOrWhiteSpace(name))
        {
            return "upload.jpg";
        }

        var builder = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            if (ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '.' or '-' or '_')
            {
                builder.Append(ch);
            }
            else if (ch is ' ' or '(' or ')')
            {
                builder.Append('_');
            }
        }

        var sanitized = builder.ToString().Trim('.', '_');
        return string.IsNullOrWhiteSpace(sanitized) ? "upload.jpg" : sanitized[..Math.Min(sanitized.Length, 120)];
    }

    private static string MapIngestError(System.Net.HttpStatusCode status, string body)
    {
        if (status == System.Net.HttpStatusCode.RequestEntityTooLarge)
        {
            return "Görsel dosyası çok büyük.";
        }

        if (status == System.Net.HttpStatusCode.UnsupportedMediaType
            || body.Contains("UNSUPPORTED", StringComparison.OrdinalIgnoreCase)
            || body.Contains("mime", StringComparison.OrdinalIgnoreCase))
        {
            return "Yalnızca JPEG, PNG, WebP veya HEIC yükleyebilirsin.";
        }

        if (body.Contains("DIMENSION", StringComparison.OrdinalIgnoreCase)
            || body.Contains("megapixel", StringComparison.OrdinalIgnoreCase))
        {
            return "Görsel çözünürlüğü izin verilen sınırı aşıyor.";
        }

        return "Görsel yüklenemedi. Lütfen tekrar dene.";
    }

    private sealed class IngestResponse
    {
        [JsonPropertyName("deliveryUrl")]
        public string? DeliveryUrl { get; set; }

        [JsonPropertyName("imageId")]
        public string? ImageId { get; set; }

        [JsonPropertyName("storageKey")]
        public string? StorageKey { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("byteSize")]
        public long ByteSize { get; set; }

        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        [JsonPropertyName("checksumSha256")]
        public string? ChecksumSha256 { get; set; }
    }
}
