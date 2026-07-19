using System.Text.RegularExpressions;

namespace AracParki.Application.Accounts;

/// <summary>Sahibinden-style password rules for marketplace accounts.</summary>
public static partial class PasswordRules
{
    public const string Hint =
        "En az 8 karakter, 1 harf ve 1 rakam; ad/e-posta ve 3 aynı karakter yan yana olamaz.";

    public static IReadOnlyList<string> Validate(string? password, string? displayName, string? email)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Şifre gerekli.");
            return errors;
        }

        if (password.Length < 8)
        {
            errors.Add("Şifre en az 8 karakter olmalı.");
        }

        if (!password.Any(char.IsLetter))
        {
            errors.Add("Şifre en az bir harf içermeli.");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Şifre en az bir rakam içermeli.");
        }

        if (HasTripleRepeat().IsMatch(password))
        {
            errors.Add("Şifrede aynı karakter üç kez yan yana olamaz.");
        }

        var lower = password.ToLowerInvariant();
        foreach (var part in ForbiddenParts(displayName, email))
        {
            if (part.Length >= 3 && lower.Contains(part, StringComparison.Ordinal))
            {
                errors.Add("Şifre adını veya e-posta adresini içeremez.");
                break;
            }
        }

        return errors;
    }

    public static bool IsValid(string? password, string? displayName, string? email)
        => Validate(password, displayName, email).Count == 0;

    private static IEnumerable<string> ForbiddenParts(string? displayName, string? email)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            foreach (var part in displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (part.Length >= 3)
                {
                    yield return part.ToLowerInvariant();
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var local = email.Split('@')[0].Trim().ToLowerInvariant();
            if (local.Length >= 3)
            {
                yield return local;
            }
        }
    }

    [GeneratedRegex(@"(.)\1\1", RegexOptions.CultureInvariant)]
    private static partial Regex HasTripleRepeat();
}
