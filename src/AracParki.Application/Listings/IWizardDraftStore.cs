namespace AracParki.Application.Listings;

public sealed class WizardDraftMeta
{
    public int Step { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string? PayloadJson { get; init; }
}

public interface IWizardDraftStore
{
    Task<string?> GetPayloadAsync(long accountId, CancellationToken cancellationToken);
    Task<WizardDraftMeta?> GetMetaAsync(long accountId, CancellationToken cancellationToken);
    Task SaveAsync(long accountId, int step, string payloadJson, CancellationToken cancellationToken);
    Task ClearAsync(long accountId, CancellationToken cancellationToken);
}
