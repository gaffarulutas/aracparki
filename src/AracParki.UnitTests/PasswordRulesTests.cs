using AracParki.Application.Accounts;

namespace AracParki.UnitTests;

public sealed class PasswordRulesTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("abcdefgh")]
    [InlineData("12345678")]
    [InlineData("aaa12345")]
    public void Rejects_weak_passwords(string password)
    {
        Assert.NotEmpty(PasswordRules.Validate(password, "Ali Veli", "ali@example.com"));
    }

    [Fact]
    public void Rejects_password_containing_name_or_email()
    {
        Assert.Contains(
            PasswordRules.Validate("Ali12345x", "Ali Veli", "user@example.com"),
            e => e.Contains("adını", StringComparison.OrdinalIgnoreCase) || e.Contains("e-posta", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            PasswordRules.Validate("user9999", "Zeynep", "user@example.com"),
            e => e.Contains("e-posta", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Accepts_strong_password()
    {
        Assert.Empty(PasswordRules.Validate("Makine42!", "Ali Veli", "ali@example.com"));
    }
}
