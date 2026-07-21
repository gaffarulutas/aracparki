using System.Security.Cryptography;
using AracParki.Application.Listings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AracParki.Infrastructure.Listings;

/// <summary>
/// Development fallback only. Production must use <see cref="CloudflareListingImageStorage"/>.
/// </summary>
public sealed class LocalListingImageStorage(
    IHostEnvironment environment,
    ILogger<LocalListingImageStorage> logger) : IListingImageStorage
{
    public async Task<ListingImageSaveResult> SaveAsync(
        long accountId,
        Stream content,
        string contentType,
        string? originalFilename,
        CancellationToken cancellationToken)
    {
        if (!ListingImageUrl.IsAllowedContentType(contentType))
        {
            throw new InvalidOperationException("Desteklenmeyen görsel formatı.");
        }

        var ext = ListingImageUrl.ExtensionForContentType(contentType);
        var webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
        var relativeDir = Path.Combine("uploads", "listings", accountId.ToString());
        var absoluteDir = Path.Combine(webRoot, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var imageId = Guid.NewGuid().ToString("N");
        var fileName = $"{imageId}{ext}";
        var absolutePath = Path.Combine(absoluteDir, fileName);

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

        await using var read = File.OpenRead(absolutePath);
        var checksum = Convert.ToHexString(await SHA256.HashDataAsync(read, cancellationToken)).ToLowerInvariant();
        var byteSize = new FileInfo(absolutePath).Length;
        var url = $"{ListingImageUrl.UploadPrefix}{accountId}/{fileName}";
        var storageKey = $"local/{accountId}/{imageId}/v1";

        logger.LogInformation("Saved listing image {Url} for account {AccountId} (local fallback)", url, accountId);

        return new ListingImageSaveResult(
            DeliveryUrl: url,
            ImageId: imageId,
            StorageKey: storageKey,
            Version: 1,
            Width: 0,
            Height: 0,
            ByteSize: byteSize,
            MimeType: contentType,
            ChecksumSha256: checksum,
            OriginalFilename: originalFilename);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // local/{accountId}/{imageId}/v1
        var parts = storageKey.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 3
            && string.Equals(parts[0], "local", StringComparison.OrdinalIgnoreCase)
            && long.TryParse(parts[1], out var accountId))
        {
            var imageId = parts[2];
            var webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
            var dir = Path.Combine(webRoot, "uploads", "listings", accountId.ToString());
            if (Directory.Exists(dir))
            {
                foreach (var path in Directory.EnumerateFiles(dir, imageId + ".*"))
                {
                    try
                    {
                        File.Delete(path);
                        logger.LogInformation("Hard-deleted local listing image {Path}", path);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to delete local listing image {Path}", path);
                        throw;
                    }
                }
            }

            return Task.CompletedTask;
        }

        // Fallback: delivery-style relative path /uploads/listings/{accountId}/{file}
        if (storageKey.StartsWith(ListingImageUrl.UploadPrefix, StringComparison.OrdinalIgnoreCase)
            && !storageKey.Contains("..", StringComparison.Ordinal))
        {
            var relative = storageKey.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolute = Path.Combine(environment.ContentRootPath, "wwwroot", relative);
            if (File.Exists(absolute))
            {
                File.Delete(absolute);
                logger.LogInformation("Hard-deleted local listing image {Path}", absolute);
            }
        }

        return Task.CompletedTask;
    }
}
