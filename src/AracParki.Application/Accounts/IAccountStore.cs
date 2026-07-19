using AracParki.Application.Accounts.Dtos;

namespace AracParki.Application.Accounts;

public interface IAccountStore
{
    Task<AccountDto?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<AccountDto?> FindByIdAsync(long id, CancellationToken cancellationToken);
    Task<long> CreateAsync(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken);
    Task UpdatePasswordHashAsync(long accountId, string passwordHash, CancellationToken cancellationToken);
    Task MarkEmailConfirmedAsync(long accountId, CancellationToken cancellationToken);
    Task SaveResetTokenAsync(long accountId, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken);
    Task<(long AccountId, string TokenHash)?> FindValidResetTokenAsync(string tokenHash, CancellationToken cancellationToken);
    Task MarkResetTokenUsedAsync(string tokenHash, CancellationToken cancellationToken);
    Task SaveEmailVerificationTokenAsync(long accountId, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken);
    Task<(long AccountId, string TokenHash)?> FindValidEmailVerificationTokenAsync(string tokenHash, CancellationToken cancellationToken);
    Task MarkEmailVerificationTokenUsedAsync(string tokenHash, CancellationToken cancellationToken);
}
