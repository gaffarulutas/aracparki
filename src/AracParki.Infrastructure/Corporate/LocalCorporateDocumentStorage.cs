using AracParki.Application.Corporate;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AracParki.Infrastructure.Corporate;

/// <summary>
/// Kurumsal evraklar için private disk deposu. Dosyalar wwwroot DIŞINDA tutulur
/// (App_Data/corporate-docs) — statik dosya middleware'i üzerinden asla servis edilmez.
/// </summary>
public sealed class LocalCorporateDocumentStorage(
    IHostEnvironment environment,
    ILogger<LocalCorporateDocumentStorage> logger) : ICorporateDocumentStorage
{
    private const string KeyPrefix = "corp-docs";

    private string RootDir => Path.Combine(environment.ContentRootPath, "App_Data", "corporate-docs");

    public async Task<string> SaveAsync(
        long corporateAccountId,
        Stream content,
        string contentType,
        string originalFileName,
        CancellationToken cancellationToken)
    {
        var ext = ExtensionFor(contentType, originalFileName);
        var fileId = Guid.NewGuid().ToString("N");
        var dir = Path.Combine(RootDir, corporateAccountId.ToString());
        Directory.CreateDirectory(dir);

        var absolutePath = Path.Combine(dir, fileId + ext);
        await using (var fs = new FileStream(
                         absolutePath,
                         FileMode.CreateNew,
                         FileAccess.Write,
                         FileShare.None,
                         81920,
                         useAsync: true))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }

        var storageKey = $"{KeyPrefix}/{corporateAccountId}/{fileId}{ext}";
        logger.LogInformation(
            "Saved corporate document {StorageKey} for corporate account {CorporateAccountId}",
            storageKey,
            corporateAccountId);
        return storageKey;
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolutePath = ResolvePath(storageKey);
        if (absolutePath is null || !File.Exists(absolutePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolutePath = ResolvePath(storageKey);
        if (absolutePath is not null && File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
            logger.LogInformation("Deleted corporate document {StorageKey}", storageKey);
        }

        return Task.CompletedTask;
    }

    /// <summary>Path traversal'a kapalı çözümleme: key köke sabitlenir ve doğrulanır.</summary>
    private string? ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)
            || !storageKey.StartsWith(KeyPrefix + "/", StringComparison.Ordinal)
            || storageKey.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        var relative = storageKey[(KeyPrefix.Length + 1)..].Replace('/', Path.DirectorySeparatorChar);
        var absolute = Path.GetFullPath(Path.Combine(RootDir, relative));
        return absolute.StartsWith(Path.GetFullPath(RootDir), StringComparison.Ordinal) ? absolute : null;
    }

    private static string ExtensionFor(string contentType, string originalFileName)
    {
        if (string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return ".pdf";
        }

        if (string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase))
        {
            return ".png";
        }

        if (string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return ".jpg";
        }

        var ext = Path.GetExtension(originalFileName ?? "");
        return string.IsNullOrEmpty(ext) ? ".bin" : ext.ToLowerInvariant();
    }
}
