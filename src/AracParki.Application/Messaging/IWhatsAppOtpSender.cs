namespace AracParki.Application.Messaging;

public interface IWhatsAppOtpSender
{
    /// <summary>
    /// Sends the Turkish OTP template (<c>otp_tr_general_template</c>) via Meta Cloud API.
    /// </summary>
    /// <param name="normalizedPhoneDigits">Digits-only phone from <c>AccountService.NormalizePhone</c>.</param>
    Task<(bool Ok, string? Error)> SendTurkishOtpAsync(
        string normalizedPhoneDigits,
        string otpCode,
        CancellationToken cancellationToken = default);
}
