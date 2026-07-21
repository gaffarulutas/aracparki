namespace AracParki.Application.Corporate;

/// <summary>
/// Kurumsal evraklar için PRIVATE depo. Dosyalar hiçbir zaman public URL almaz;
/// indirme yalnızca yetkilendirilmiş handler üzerinden stream edilir.
/// </summary>
public interface ICorporateDocumentStorage
{
    /// <summary>Dosyayı kaydeder ve storage key döner.</summary>
    Task<string> SaveAsync(
        long corporateAccountId,
        Stream content,
        string contentType,
        string originalFileName,
        CancellationToken cancellationToken);

    /// <summary>Storage key ile dosyayı okuma için açar; bulunamazsa null.</summary>
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
