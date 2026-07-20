using AracParki.Application.Listings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AracParki.Infrastructure.Listings;

public sealed class LocalListingImageStorage(
    IHostEnvironment environment,
    ILogger<LocalListingImageStorage> logger) : IListingImageStorage
{
    public async Task<string> SaveAsync(
        long accountId,
        Stream content,
        string contentType,
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

        var fileName = $"{Guid.NewGuid():N}{ext}";
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

        var url = $"{ListingImageUrl.UploadPrefix}{accountId}/{fileName}";
        logger.LogInformation("Saved listing image {Url} for account {AccountId}", url, accountId);
        return url;
    }
}
