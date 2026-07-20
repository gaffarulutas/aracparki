using System.Text.Json;
using AracParki.Application.Listings;

namespace AracParki.Web.Pages.IlanVer;

public static class WizardDraftStore
{
    public const string SessionKey = "ilan-ver-draft";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static WizardDraft Get(ISession session)
    {
        var raw = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new WizardDraft();
        }

        try
        {
            return JsonSerializer.Deserialize<WizardDraft>(raw, JsonOptions) ?? new WizardDraft();
        }
        catch (JsonException)
        {
            return new WizardDraft();
        }
    }

    public static WizardDraft FromJson(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new WizardDraft();
        }

        try
        {
            return JsonSerializer.Deserialize<WizardDraft>(raw, JsonOptions) ?? new WizardDraft();
        }
        catch (JsonException)
        {
            return new WizardDraft();
        }
    }

    public static string ToJson(WizardDraft draft)
        => JsonSerializer.Serialize(draft, JsonOptions);

    public static void Save(ISession session, WizardDraft draft)
        => session.SetString(SessionKey, ToJson(draft));

    public static void Clear(ISession session) => session.Remove(SessionKey);

    public static async Task PersistAsync(
        ISession session,
        IWizardDraftStore store,
        long? accountId,
        WizardDraft draft,
        CancellationToken cancellationToken)
    {
        Save(session, draft);
        if (accountId is null)
        {
            return;
        }

        await store.SaveAsync(accountId.Value, draft.Step, ToJson(draft), cancellationToken);
    }

    public static async Task ClearAllAsync(
        ISession session,
        IWizardDraftStore store,
        long? accountId,
        CancellationToken cancellationToken)
    {
        Clear(session);
        if (accountId is not null)
        {
            await store.ClearAsync(accountId.Value, cancellationToken);
        }
    }

    public static async Task<WizardDraft> LoadAsync(
        ISession session,
        IWizardDraftStore store,
        long? accountId,
        CancellationToken cancellationToken)
    {
        var sessionDraft = Get(session);
        if (accountId is null)
        {
            return sessionDraft;
        }

        if (sessionDraft.HasCategory || sessionDraft.Step > 1)
        {
            return sessionDraft;
        }

        var payload = await store.GetPayloadAsync(accountId.Value, cancellationToken);
        var dbDraft = FromJson(payload);
        if (dbDraft.HasCategory || dbDraft.Step > 1)
        {
            Save(session, dbDraft);
            return dbDraft;
        }

        return sessionDraft;
    }
}
