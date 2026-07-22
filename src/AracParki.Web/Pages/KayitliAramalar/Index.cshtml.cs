using System.Security.Claims;
using AracParki.Application.Listings.Dtos;
using AracParki.Application.Listings.Services;
using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.KayitliAramalar;

public sealed class IndexModel(SavedSearchService savedSearches) : AccountPageModel
{
    public IReadOnlyList<SavedSearchDto> Items { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        Items = await savedSearches.ListAsync(accountId, cancellationToken);
        SetAccountMeta("Kayıtlı aramalar", "Kaydettiğin filtreler ve arama kriterleri");
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(string? url, string? label, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        var wantsJson = WantsJson();
        try
        {
            var saved = await savedSearches.SaveAsync(accountId, label, url, cancellationToken);
            if (wantsJson)
            {
                return new JsonResult(new { ok = true, id = saved.Id, url = saved.Url, name = saved.Name });
            }

            TempData["AuthNotice"] = "Arama kaydedildi.";
            return RedirectToPage();
        }
        catch (ArgumentException ex)
        {
            if (wantsJson)
            {
                return new JsonResult(new { ok = false, error = ex.Message }) { StatusCode = 400 };
            }

            TempData["AuthNotice"] = ex.Message;
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            if (wantsJson)
            {
                return new JsonResult(new { ok = false, error = ex.Message }) { StatusCode = 400 };
            }

            TempData["AuthNotice"] = ex.Message;
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Challenge();
        }

        await savedSearches.DeleteAsync(accountId, id, cancellationToken);
        TempData["AuthNotice"] = "Kayıtlı arama silindi.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(string? url, string? label, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized();
        }

        try
        {
            var exists = await savedSearches.ExistsByUrlAsync(accountId, url, cancellationToken);
            if (exists)
            {
                await savedSearches.DeleteByUrlAsync(accountId, url, cancellationToken);
                return new JsonResult(new { ok = true, saved = false });
            }

            var saved = await savedSearches.SaveAsync(accountId, label, url, cancellationToken);
            return new JsonResult(new { ok = true, saved = true, id = saved.Id });
        }
        catch (ArgumentException ex)
        {
            return new JsonResult(new { ok = false, error = ex.Message }) { StatusCode = 400 };
        }
        catch (InvalidOperationException ex)
        {
            return new JsonResult(new { ok = false, error = ex.Message }) { StatusCode = 400 };
        }
    }

    public async Task<IActionResult> OnGetStatusAsync(string? url, CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return new JsonResult(new { ok = true, saved = false, authenticated = false });
        }

        try
        {
            var exists = await savedSearches.ExistsByUrlAsync(accountId, url, cancellationToken);
            return new JsonResult(new { ok = true, saved = exists, authenticated = true });
        }
        catch (ArgumentException)
        {
            return new JsonResult(new { ok = true, saved = false, authenticated = true });
        }
    }

    private bool WantsJson()
        => string.Equals(Request.Headers.Accept.ToString(), "application/json", StringComparison.OrdinalIgnoreCase)
           || string.Equals(Request.Query["format"], "json", StringComparison.OrdinalIgnoreCase)
           || string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private bool TryGetAccountId(out long accountId)
    {
        accountId = 0;
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out accountId) && accountId > 0;
    }
}
