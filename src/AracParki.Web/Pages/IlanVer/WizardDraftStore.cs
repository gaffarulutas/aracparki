using System.Text.Json;

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

    public static void Save(ISession session, WizardDraft draft)
        => session.SetString(SessionKey, JsonSerializer.Serialize(draft, JsonOptions));

    public static void Clear(ISession session) => session.Remove(SessionKey);
}
