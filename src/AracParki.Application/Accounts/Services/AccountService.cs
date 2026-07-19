using System.Security.Cryptography;
using System.Text;
using AracParki.Application.Accounts.Dtos;
using Microsoft.AspNetCore.Identity;

namespace AracParki.Application.Accounts.Services;

public sealed class AccountService
{
    private readonly IAccountStore _store;
    private readonly PasswordHasher<string> _hasher = new();

    public AccountService(IAccountStore store)
    {
        _store = store;
    }

    /// <summary>Creates account + email verification token. Does not sign the user in.</summary>
    public async Task<(bool Ok, string? Error, AccountDto? Account, string? VerifyToken)> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        email = NormalizeEmail(email);
        firstName = firstName.Trim();
        lastName = lastName.Trim();
        var displayName = $"{firstName} {lastName}".Trim();

        var passwordErrors = PasswordRules.Validate(password, displayName, email);
        if (passwordErrors.Count > 0)
        {
            return (false, passwordErrors[0], null, null);
        }

        if (await _store.FindByEmailAsync(email, cancellationToken) is not null)
        {
            return (false, "Bu e-posta adresiyle bir hesap zaten var.", null, null);
        }

        var hash = _hasher.HashPassword(email, password);
        var id = await _store.CreateAsync(email, hash, firstName, lastName, phone: null, cancellationToken);
        var account = await _store.FindByIdAsync(id, cancellationToken);
        var verifyToken = await IssueEmailVerificationTokenAsync(id, cancellationToken);
        return (true, null, account, verifyToken);
    }

    public async Task<(bool Ok, string? Error, AccountDto? Account)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        email = NormalizeEmail(email);
        var account = await _store.FindByEmailAsync(email, cancellationToken);
        if (account is null)
        {
            return (false, "E-posta veya şifre hatalı.", null);
        }

        var result = _hasher.VerifyHashedPassword(email, account.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return (false, "E-posta veya şifre hatalı.", null);
        }

        if (!account.EmailConfirmed)
        {
            return (false, "email_unconfirmed", account);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var newHash = _hasher.HashPassword(email, password);
            await _store.UpdatePasswordHashAsync(account.Id, newHash, cancellationToken);
        }

        return (true, null, account);
    }

    /// <summary>Anti-enumeration: always appears successful to the caller.</summary>
    public async Task<string?> RequestEmailVerificationAsync(string email, CancellationToken cancellationToken)
    {
        email = NormalizeEmail(email);
        var account = await _store.FindByEmailAsync(email, cancellationToken);
        if (account is null || account.EmailConfirmed)
        {
            return null;
        }

        return await IssueEmailVerificationTokenAsync(account.Id, cancellationToken);
    }

    public async Task<(bool Ok, string? Error, AccountDto? Account)> ConfirmEmailAsync(
        string rawToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return (false, "Geçersiz veya süresi dolmuş bağlantı.", null);
        }

        var hash = HashToken(rawToken.Trim());
        var row = await _store.FindValidEmailVerificationTokenAsync(hash, cancellationToken);
        if (row is null)
        {
            return (false, "Geçersiz veya süresi dolmuş bağlantı.", null);
        }

        await _store.MarkEmailConfirmedAsync(row.Value.AccountId, cancellationToken);
        await _store.MarkEmailVerificationTokenUsedAsync(hash, cancellationToken);
        var account = await _store.FindByIdAsync(row.Value.AccountId, cancellationToken);
        return (true, null, account);
    }

    /// <summary>
    /// Always succeeds from the caller's perspective (anti-enumeration).
    /// Returns a raw token only when an account exists (for local/dev reset link).
    /// </summary>
    public async Task<string?> RequestPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
        email = NormalizeEmail(email);
        var account = await _store.FindByEmailAsync(email, cancellationToken);
        if (account is null)
        {
            return null;
        }

        var raw = CreateRawToken();
        var hash = HashToken(raw);
        await _store.SaveResetTokenAsync(account.Id, hash, DateTimeOffset.UtcNow.AddHours(1), cancellationToken);
        return raw;
    }

    public async Task<(bool Ok, string? Error)> ResetPasswordAsync(
        string rawToken,
        string newPassword,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return (false, "Geçersiz veya süresi dolmuş bağlantı.");
        }

        var passwordErrors = PasswordRules.Validate(newPassword, displayName: null, email: null);
        if (passwordErrors.Count > 0)
        {
            return (false, passwordErrors[0]);
        }

        var hash = HashToken(rawToken.Trim());
        var row = await _store.FindValidResetTokenAsync(hash, cancellationToken);
        if (row is null)
        {
            return (false, "Geçersiz veya süresi dolmuş bağlantı.");
        }

        var account = await _store.FindByIdAsync(row.Value.AccountId, cancellationToken);
        if (account is null)
        {
            return (false, "Geçersiz veya süresi dolmuş bağlantı.");
        }

        var passwordHash = _hasher.HashPassword(account.Email, newPassword);
        await _store.UpdatePasswordHashAsync(account.Id, passwordHash, cancellationToken);
        await _store.MarkResetTokenUsedAsync(hash, cancellationToken);
        return (true, null);
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    private async Task<string> IssueEmailVerificationTokenAsync(long accountId, CancellationToken cancellationToken)
    {
        var raw = CreateRawToken();
        var hash = HashToken(raw);
        await _store.SaveEmailVerificationTokenAsync(
            accountId,
            hash,
            DateTimeOffset.UtcNow.AddDays(2),
            cancellationToken);
        return raw;
    }

    private static string CreateRawToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }
}
