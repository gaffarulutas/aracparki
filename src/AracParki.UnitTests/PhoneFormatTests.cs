using AracParki.Application.Common;

namespace AracParki.UnitTests;

public sealed class PhoneFormatTests
{
    [Theory]
    [InlineData("5321234567", "5321234567")]
    [InlineData("05321234567", "5321234567")]
    [InlineData("905321234567", "5321234567")]
    [InlineData("+90 532 123 45 67", "5321234567")]
    [InlineData("02165550101", "2165550101")]
    public void PhoneDigits_normalizes_tr_numbers(string input, string expected)
    {
        Assert.Equal(expected, Formatters.PhoneDigits(input));
    }

    [Theory]
    [InlineData("5321234567", "+90 532 123 45 67")]
    [InlineData("05321234567", "+90 532 123 45 67")]
    [InlineData("905321234567", "+90 532 123 45 67")]
    [InlineData("02165550101", "+90 216 555 01 01")]
    public void PhoneDisplay_formats_with_country_code(string input, string expected)
    {
        Assert.Equal(expected, Formatters.PhoneDisplay(input));
    }

    [Theory]
    [InlineData("5321234567", "+905321234567")]
    [InlineData("05321234567", "+905321234567")]
    [InlineData("905321234567", "+905321234567")]
    [InlineData("02165550101", "+902165550101")]
    public void PhoneTel_returns_e164(string input, string expected)
    {
        Assert.Equal(expected, Formatters.PhoneTel(input));
    }

    [Fact]
    public void PhoneTel_rejects_short()
    {
        Assert.Null(Formatters.PhoneTel("123"));
    }
}
