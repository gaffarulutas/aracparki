using AracParki.Application.Accounts;
using AracParki.Application.Accounts.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace AracParki.Web.Infrastructure;

public static class AuthCookie
{
    public const string SecurityStampClaimType = "sstamp";
    public const string EmailConfirmedClaimType = "email_confirmed";

    public static ClaimsPrincipal CreatePrincipal(AccountDto account)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Name, account.DisplayName),
            new(SecurityStampClaimType, account.SecurityStamp),
            new(EmailConfirmedClaimType, account.EmailConfirmed ? "1" : "0")
        };

        if (!string.IsNullOrWhiteSpace(account.Phone))
        {
            claims.Add(new Claim("phone", account.Phone));
        }

        return new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static bool IsEmailConfirmed(ClaimsPrincipal user)
        => user.FindFirstValue(EmailConfirmedClaimType) == "1";

    public static void ConfigureSecurityStampValidation(CookieAuthenticationOptions options)
    {
        options.Events.OnValidatePrincipal = ValidateSecurityStampAsync;
    }

    private static async Task ValidateSecurityStampAsync(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var stamp = principal.FindFirstValue(SecurityStampClaimType);
        if (!long.TryParse(idValue, out var accountId) || string.IsNullOrWhiteSpace(stamp))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        var store = context.HttpContext.RequestServices.GetRequiredService<IAccountStore>();
        var account = await store.FindByIdAsync(accountId, context.HttpContext.RequestAborted);
        if (account is null || !string.Equals(account.SecurityStamp, stamp, StringComparison.Ordinal))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        // Keep soft-gate banner in sync after confirmation without forcing re-login.
        var claim = principal.FindFirst(EmailConfirmedClaimType)?.Value == "1";
        if (claim != account.EmailConfirmed)
        {
            context.ReplacePrincipal(CreatePrincipal(account));
            context.ShouldRenew = true;
        }
    }
}
