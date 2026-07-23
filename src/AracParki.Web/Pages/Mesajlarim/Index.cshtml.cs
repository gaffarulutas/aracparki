using System.Security.Claims;
using AracParki.Application.Conversations.Dtos;
using AracParki.Application.Conversations.Services;
using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.Mesajlarim;

public sealed class IndexModel(MessagingService messaging) : AccountPageModel
{
    public IReadOnlyList<MessageThreadListItemDto> Threads { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
            return Challenge();

        Threads = await messaging.ListThreadsAsync(accountId, 50, cancellationToken);
        SetAccountMeta("Mesajlarım", "İlan sahipleri ve alıcılarla yazışmaların");
        return Page();
    }

    private bool TryGetAccountId(out long accountId)
    {
        accountId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out accountId) && accountId > 0;
    }
}
