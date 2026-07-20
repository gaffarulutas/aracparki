using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AracParki.Web.Pages.Hesabim;

[Authorize]
public sealed class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectPermanent("/bilgilerim");
}
