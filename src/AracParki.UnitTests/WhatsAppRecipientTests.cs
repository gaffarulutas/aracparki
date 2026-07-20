using AracParki.Infrastructure.Messaging;

namespace AracParki.UnitTests;

public sealed class WhatsAppRecipientTests
{
    [Theory]
    [InlineData("05551112233", "+90", "905551112233")]
    [InlineData("5551112233", "90", "905551112233")]
    [InlineData("905551112233", "+90", "905551112233")]
    [InlineData("90 555 111 22 33", "+90", "905551112233")]
    public void ToWhatsAppRecipient_formats_tr_numbers(string input, string cc, string expected)
    {
        var actual = WhatsAppOtpSender.ToWhatsAppRecipient(input, cc);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToWhatsAppRecipient_rejects_short()
    {
        Assert.Null(WhatsAppOtpSender.ToWhatsAppRecipient("123", "+90"));
    }
}
