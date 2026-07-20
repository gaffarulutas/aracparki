using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages;

[Authorize]
public abstract class AccountPageModel : PageModel
{
    protected void SetAccountMeta(string title, string description)
    {
        ViewData["PageKey"] = "account";
        ViewData["Title"] = title + " | Araç Parkı";
        ViewData["Description"] = description;
        ViewData["Robots"] = "noindex, nofollow";
    }
}
