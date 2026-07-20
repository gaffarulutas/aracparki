using AracParki.Application.Accounts.Services;

namespace AracParki.UnitTests;

public sealed class MaskPhoneTests
{
    [Theory]
    [InlineData("5321234567", "5•• ••• •• 67")]
    [InlineData("05321234567", "5•• ••• •• 67")]
    [InlineData("905321234567", "5•• ••• •• 67")]
    [InlineData("+90 532 123 45 67", "5•• ••• •• 67")]
    [InlineData("12", "••••")]
    [InlineData(null, "••••")]
    public void MaskPhone_hides_middle_digits(string? input, string expected)
    {
        Assert.Equal(expected, AccountService.MaskPhone(input));
    }
}
