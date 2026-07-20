using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AracParki.Application.Abstractions;
using AracParki.Application.Accounts;
using AracParki.Application.Accounts.Dtos;
using AracParki.Application.Accounts.Services;
using AracParki.Application.Email;
using AracParki.Web.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AracParki.UnitTests;

public sealed class SoftEmailGateTests
{
    private readonly IAccountStore _store = Substitute.For<IAccountStore>();
    private readonly AccountService _accounts;

    public SoftEmailGateTests()
    {
        var emailSender = Substitute.For<IEmailSender>();
        var authEmail = new AuthEmailService(emailSender, Options.Create(new AppSettings()));
        _accounts = new AccountService(_store, authEmail, NullLogger<AccountService>.Instance);
    }

    [Fact]
    public async Task LoginAsync_allows_unconfirmed_account()
    {
        var hasher = new PasswordHasher<string>();
        const string email = "user@example.com";
        const string password = "Secret1x";
        var account = Unconfirmed(email, hasher.HashPassword(email, password));

        _store.FindByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(account);

        var (ok, error, result) = await _accounts.LoginAsync(email, password, CancellationToken.None);

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.False(result!.EmailConfirmed);
    }

    [Fact]
    public async Task PeekEmailVerification_returns_pending_for_usable_token()
    {
        const string raw = "AABBCCDDEEFF00112233445566778899AABBCCDDEEFF00112233445566778899";
        var hash = Hash(raw);
        var account = Unconfirmed("a@b.com", "hash");

        _store.FindAccountByVerificationTokenHashAsync(hash, Arg.Any<CancellationToken>())
            .Returns((account.Id, false, true));
        _store.FindByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var (pending, already, dto, error) =
            await _accounts.PeekEmailVerificationAsync(raw, CancellationToken.None);

        Assert.True(pending);
        Assert.False(already);
        Assert.NotNull(dto);
        Assert.Null(error);
    }

    [Fact]
    public async Task PeekEmailVerification_returns_already_confirmed()
    {
        const string raw = "11223344556677889900AABBCCDDEEFF11223344556677889900AABBCCDDEEFF";
        var hash = Hash(raw);
        var account = Confirmed("a@b.com", "hash");

        _store.FindAccountByVerificationTokenHashAsync(hash, Arg.Any<CancellationToken>())
            .Returns((account.Id, true, false));
        _store.FindByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var (pending, already, dto, error) =
            await _accounts.PeekEmailVerificationAsync(raw, CancellationToken.None);

        Assert.False(pending);
        Assert.True(already);
        Assert.NotNull(dto);
        Assert.Null(error);
    }

    [Fact]
    public async Task PeekEmailVerification_rejects_superseded_token()
    {
        const string raw = "99887766554433221100FFEEDDCCBBAA99887766554433221100FFEEDDCCBBAA";
        var hash = Hash(raw);
        var account = Unconfirmed("a@b.com", "hash");

        _store.FindAccountByVerificationTokenHashAsync(hash, Arg.Any<CancellationToken>())
            .Returns((account.Id, false, false));
        _store.FindByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var (pending, already, dto, error) =
            await _accounts.PeekEmailVerificationAsync(raw, CancellationToken.None);

        Assert.False(pending);
        Assert.False(already);
        Assert.Null(dto);
        Assert.Equal("Geçersiz veya süresi dolmuş bağlantı.", error);
    }

    [Fact]
    public async Task ConfirmEmailAsync_consumes_valid_token()
    {
        const string raw = "00112233445566778899AABBCCDDEEFF00112233445566778899AABBCCDDEEFF";
        var hash = Hash(raw);
        var account = Confirmed("a@b.com", "hash");

        _store.TryConfirmEmailWithTokenAsync(hash, Arg.Any<CancellationToken>()).Returns(account.Id);
        _store.FindByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var (ok, error, dto, already) =
            await _accounts.ConfirmEmailAsync(raw, CancellationToken.None);

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(dto);
        Assert.False(already);
    }

    [Fact]
    public async Task ConfirmEmailAsync_is_idempotent_when_already_confirmed()
    {
        const string raw = "FFEEDDCCBBAA00998877665544332211FFEEDDCCBBAA00998877665544332211";
        var hash = Hash(raw);
        var account = Confirmed("a@b.com", "hash");

        _store.TryConfirmEmailWithTokenAsync(hash, Arg.Any<CancellationToken>()).Returns((long?)null);
        _store.FindAccountByVerificationTokenHashAsync(hash, Arg.Any<CancellationToken>())
            .Returns((account.Id, true, false));
        _store.FindByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var (ok, error, dto, already) =
            await _accounts.ConfirmEmailAsync(raw, CancellationToken.None);

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(dto);
        Assert.True(already);
    }

    [Fact]
    public void AuthCookie_IsEmailConfirmed_reads_claim()
    {
        var confirmed = AuthCookie.CreatePrincipal(Confirmed("a@b.com", "h"));
        var pending = AuthCookie.CreatePrincipal(Unconfirmed("b@c.com", "h"));

        Assert.True(AuthCookie.IsEmailConfirmed(confirmed));
        Assert.False(AuthCookie.IsEmailConfirmed(pending));
        Assert.Equal("1", confirmed.FindFirstValue(AuthCookie.EmailConfirmedClaimType));
        Assert.Equal("0", pending.FindFirstValue(AuthCookie.EmailConfirmedClaimType));
    }

    private static string Hash(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

    private static AccountDto Unconfirmed(string email, string passwordHash) => new()
    {
        Id = 7,
        Email = email,
        PasswordHash = passwordHash,
        FirstName = "Ada",
        LastName = "Yılmaz",
        SecurityStamp = "stamp",
        EmailConfirmedAt = null
    };

    private static AccountDto Confirmed(string email, string passwordHash) => new()
    {
        Id = 7,
        Email = email,
        PasswordHash = passwordHash,
        FirstName = "Ada",
        LastName = "Yılmaz",
        SecurityStamp = "stamp",
        EmailConfirmedAt = DateTimeOffset.UtcNow
    };
}
