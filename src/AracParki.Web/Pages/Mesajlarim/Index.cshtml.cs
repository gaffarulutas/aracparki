using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.Mesajlarim;

public sealed class IndexModel : AccountPageModel
{
    public IActionResult OnGet()
    {
        SetAccountMeta("Mesajlarım", "İlan sahipleri ve alıcılarla yazışmaların");
        return Page();
    }
}
