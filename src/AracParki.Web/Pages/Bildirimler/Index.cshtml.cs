using System.Security.Claims;
using System.Text.Json;
using AracParki.Application.Notifications;
using AracParki.Application.Notifications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.Bildirimler;

public sealed class IndexModel(INotificationService notifications) : AccountPageModel
{
    public IReadOnlyList<NotificationDto> Items { get; private set; } = [];
    public int UnreadCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        Items = await notifications.ListAsync(accountId, 50, cancellationToken);
        UnreadCount = Items.Count(x => x.IsUnread);
        SetAccountMeta("Bildirimler", "İlan ve hesap bildirimlerin");
        return Page();
    }

    public async Task<IActionResult> OnPostMarkReadAsync(
        long id,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        await notifications.MarkReadAsync(accountId, id, cancellationToken);
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        await notifications.MarkAllReadAsync(accountId, cancellationToken);
        TempData["AuthNotice"] = "Tüm bildirimler okundu işaretlendi.";
        return RedirectToPage();
    }

    public static string? TryGetUrl(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(dataJson);
            if (doc.RootElement.TryGetProperty("url", out var urlProp)
                && urlProp.ValueKind == JsonValueKind.String)
            {
                var url = urlProp.GetString();
                if (string.IsNullOrWhiteSpace(url)
                    || !url.StartsWith('/')
                    || url.StartsWith("//", StringComparison.Ordinal)
                    || url.Contains('\\', StringComparison.Ordinal)
                    || url.Contains("://", StringComparison.Ordinal))
                {
                    return null;
                }

                return url;
            }
        }
        catch (JsonException)
        {
            // ignore malformed payload
        }

        return null;
    }

    private bool TryGetAccountId(out long accountId)
    {
        accountId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out accountId) && accountId > 0;
    }
}
