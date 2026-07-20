using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.Bildirimler;

public sealed class IndexModel : AccountPageModel
{
    public IActionResult OnGet()
    {
        SetAccountMeta("Bildirimler", "İlan ve hesap bildirimlerin");
        return Page();
    }
}
