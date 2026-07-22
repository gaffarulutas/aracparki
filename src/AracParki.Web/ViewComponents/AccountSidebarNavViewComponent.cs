using System.Security.Claims;
using AracParki.Application.Accounts.Dtos;
using AracParki.Application.Accounts.Services;
using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.ViewComponents;

public sealed class AccountSidebarNavViewComponent(AccountNavCountsService counts) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new AccountNavCountsDto();
        var raw = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(raw, out var accountId) && accountId > 0)
        {
            model = await counts.GetAsync(accountId, HttpContext.RequestAborted);
        }

        return View(model);
    }
}
