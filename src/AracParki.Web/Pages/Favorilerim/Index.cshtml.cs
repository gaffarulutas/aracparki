using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Favorilerim;

public sealed class IndexModel : AccountPageModel
{
    public IActionResult OnGet()
    {
        SetAccountMeta("Favorilerim", "Beğendiğin iş makinesi ilanları");
        return Page();
    }
}
