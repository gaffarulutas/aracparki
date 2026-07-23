using System.Security.Claims;
using AracParki.Application.Conversations.Dtos;
using AracParki.Application.Conversations.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AracParki.Web.Pages.Mesajlarim;

public sealed class DetayModel(MessagingService messaging) : AccountPageModel
{
    [BindProperty(SupportsGet = true)]
    public long ThreadId { get; set; }

    [BindProperty]
    public string? Body { get; set; }

    public MessageThreadDetailDto? Thread { get; private set; }
    public string? FormError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
            return Challenge();

        if (ThreadId <= 0)
            return NotFound();

        Thread = await messaging.GetThreadAsync(ThreadId, accountId, cancellationToken);
        if (Thread is null)
            return NotFound();

        SetMeta(Thread);
        return Page();
    }

    public async Task<IActionResult> OnGetMessagesAsync(long? afterId, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
            return Unauthorized();

        if (ThreadId <= 0)
            return NotFound();

        var messages = await messaging.ListMessagesAsync(ThreadId, accountId, afterId, cancellationToken);
        if (messages is null)
            return NotFound();

        if (messages.Count > 0)
            await messaging.MarkReadAsync(ThreadId, accountId, cancellationToken);

        return new JsonResult(messages.Select(m => new
        {
            id = m.Id,
            body = m.Body,
            createdAt = m.CreatedAt,
            isMine = m.IsMine
        }));
    }

    [EnableRateLimiting("messaging-send")]
    public async Task<IActionResult> OnPostSendAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
            return Challenge();

        if (ThreadId <= 0)
            return NotFound();

        try
        {
            await messaging.SendAsync(ThreadId, accountId, Body ?? "", cancellationToken);
            return RedirectToPage(new { threadId = ThreadId });
        }
        catch (InvalidOperationException ex)
        {
            FormError = ex.Message;
            Thread = await messaging.GetThreadAsync(ThreadId, accountId, cancellationToken);
            if (Thread is null)
                return NotFound();

            SetMeta(Thread);
            return Page();
        }
    }

    private void SetMeta(MessageThreadDetailDto thread)
        => SetAccountMeta("Mesaj", thread.ListingTitle);

    private bool TryGetAccountId(out long accountId)
    {
        accountId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out accountId) && accountId > 0;
    }
}
