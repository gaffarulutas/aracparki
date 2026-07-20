namespace AracParki.Application.Accounts;

public interface IPhoneOtpStore
{
    Task SaveAsync(long accountId, string phone, string codeHash, DateTimeOffset expiresAt, CancellationToken cancellationToken);
    Task<(string Phone, string CodeHash)?> GetLatestAsync(long accountId, CancellationToken cancellationToken);
    Task ConsumeLatestAsync(long accountId, CancellationToken cancellationToken);
}

public interface IPhoneOtpService
{
    Task<(bool Ok, string? Error, string? DevCode)> SendAsync(long accountId, string phone, CancellationToken cancellationToken);
    Task<(bool Ok, string? Error)> VerifyAsync(long accountId, string phone, string code, CancellationToken cancellationToken);
}
