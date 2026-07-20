using Microsoft.AspNetCore.Mvc;

namespace AracParki.Web.Pages.KayitliAramalar;

public sealed class IndexModel : AccountPageModel
{
    public IActionResult OnGet()
    {
        SetAccountMeta("Kayıtlı aramalar", "Kaydettiğin filtreler ve arama kriterleri");
        return Page();
    }
}
