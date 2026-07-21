namespace AracParki.Domain.Accounts;

/// <summary>DB role values on accounts.role (lowercase). Claims use AuthRoles.Admin.</summary>
public static class AccountRole
{
    public const string User = "user";
    public const string Admin = "admin";

    public static readonly HashSet<string> Known = [User, Admin];

    public static bool IsAdmin(string? role) =>
        string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase);
}
