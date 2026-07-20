namespace AracParki.Application.Listings;

public interface IWizardDraftStore
{
    Task<string?> GetPayloadAsync(long accountId, CancellationToken cancellationToken);
    Task SaveAsync(long accountId, int step, string payloadJson, CancellationToken cancellationToken);
    Task ClearAsync(long accountId, CancellationToken cancellationToken);
}
