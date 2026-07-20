using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.Ayarlar;

public sealed class IndexModel : AccountPageModel
{
    public IActionResult OnGet()
    {
        SetAccountMeta("Ayarlar", "Şifre, bildirim ve hesap tercihleri");
        return Page();
    }
}
