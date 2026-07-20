using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AracParki.Application.Messaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AracParki.Infrastructure.Messaging;

/// <summary>
/// Meta WhatsApp Cloud API sender — Turkish OTP template payload mirrors waponi-api WhatsAppService.
/// </summary>
public sealed class WhatsAppOtpSender(
    IHttpClientFactory httpClientFactory,
    IOptions<WhatsAppSettings> options,
    IMemoryCache memoryCache,
    ILogger<WhatsAppOtpSender> logger) : IWhatsAppOtpSender
{
    public const string HttpClientName = nameof(WhatsAppOtpSender);
    private const string RateLimitCacheKeyPrefix = "whatsapp-otp-send-";

    private static readonly JsonSerializerOptions JsonOptions = new();


    private readonly WhatsAppSettings _settings = options.Value;

    public async Task<(bool Ok, string? Error)> SendTurkishOtpAsync(
        string normalizedPhoneDigits,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhoneDigits) || string.IsNullOrWhiteSpace(otpCode))
        {
            return (false, "Telefon veya doğrulama kodu eksik.");
        }

        if (!_settings.IsConfigured)
        {
            logger.LogError("WhatsAppSettings AccountSid/AuthToken missing");
            return (false, "WhatsApp entegrasyon ayarları yapılandırılmamış.");
        }

        var toNumber = ToWhatsAppRecipient(normalizedPhoneDigits, _settings.FromNumberCountryCode);
        if (toNumber is null)
        {
            return (false, "WhatsApp için telefon numarası geçersiz.");
        }

        var rateLimitError = CheckOtpRateLimit(toNumber);
        if (rateLimitError is not null)
        {
            return (false, rateLimitError);
        }

        var endpoint = BuildEndpoint();
        var payload = BuildTurkishOtpPayload(toNumber, otpCode);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.AuthToken.Trim());
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json");

            var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "WhatsApp OTP failed. Status {StatusCode}, To {To}, Template {Template}, Body {Body}",
                    (int)response.StatusCode,
                    toNumber,
                    _settings.TurkishTemplateName,
                    content);
                return (false, $"WhatsApp doğrulama kodu gönderilemedi. ({(int)response.StatusCode})");
            }

            RecordOtpSend(toNumber);
            logger.LogInformation(
                "WhatsApp OTP sent via {Template} to …{Suffix}",
                _settings.TurkishTemplateName,
                toNumber.Length >= 4 ? toNumber[^4..] : "****");
            return (true, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "WhatsApp OTP send exception");
            return (false, "WhatsApp doğrulama kodu gönderilemedi.");
        }
    }

    /// <summary>
    /// Builds Meta <c>to</c> digits: country code (no +) + national number (no leading 0).
    /// Same convention as waponi-api WhatsAppService.
    /// </summary>
    internal static string? ToWhatsAppRecipient(string normalizedDigits, string? countryCode)
    {
        var digits = new string(normalizedDigits.Where(char.IsDigit).ToArray());
        if (digits.Length is < 10 or > 15)
        {
            return null;
        }

        var cc = new string((countryCode ?? "90").Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(cc))
        {
            cc = "90";
        }

        if (digits.StartsWith(cc, StringComparison.Ordinal) && digits.Length >= cc.Length + 10)
        {
            return digits;
        }

        var national = digits.TrimStart('0');
        if (national.Length is < 10 or > 12)
        {
            return null;
        }

        return cc + national;
    }

    private object BuildTurkishOtpPayload(string toNumber, string otpCode) => new
    {
        messaging_product = "whatsapp",
        to = toNumber,
        type = "template",
        template = new
        {
            name = _settings.TurkishTemplateName,
            language = new { code = _settings.TurkishTemplateLanguageCode },
            components = new object[]
            {
                new
                {
                    type = "body",
                    parameters = new object[]
                    {
                        new { type = "text", text = otpCode },
                        new { type = "text", text = _settings.BrandName }
                    }
                },
                new
                {
                    type = "button",
                    sub_type = "url",
                    index = 0,
                    parameters = new object[]
                    {
                        new { type = "text", text = otpCode }
                    }
                }
            }
        }
    };

    private string BuildEndpoint()
    {
        var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl)
            ? "https://graph.facebook.com"
            : _settings.BaseUrl.TrimEnd('/');
        var apiVersion = string.IsNullOrWhiteSpace(_settings.ApiVersion)
            ? "v25.0"
            : _settings.ApiVersion.Trim('/');
        return $"{baseUrl}/{apiVersion}/{_settings.AccountSid.Trim()}/messages";
    }

    private string? CheckOtpRateLimit(string phoneNumber)
    {
        var maxRequests = Math.Max(1, _settings.OtpRateLimitMaxRequests);
        var window = TimeSpan.FromMinutes(Math.Max(1, _settings.OtpRateLimitWindowMinutes));
        var cacheKey = RateLimitCacheKeyPrefix + phoneNumber;

        if (!memoryCache.TryGetValue(cacheKey, out List<DateTimeOffset>? requests) || requests is null)
        {
            return null;
        }

        var cutoff = DateTimeOffset.UtcNow - window;
        var valid = requests.Where(r => r > cutoff).ToList();
        if (valid.Count < maxRequests)
        {
            return null;
        }

        var oldest = valid.Min();
        var retryAfterSeconds = Math.Max(
            1,
            (int)Math.Ceiling((oldest + window - DateTimeOffset.UtcNow).TotalSeconds));

        logger.LogWarning(
            "WhatsApp OTP rate limit for …{Suffix}: {Count}/{Max}",
            phoneNumber[^4..],
            valid.Count,
            maxRequests);

        return $"Bu numaraya çok fazla kod gönderildi. {retryAfterSeconds} sn sonra tekrar dene.";
    }

    private void RecordOtpSend(string phoneNumber)
    {
        var window = TimeSpan.FromMinutes(Math.Max(1, _settings.OtpRateLimitWindowMinutes));
        var cacheKey = RateLimitCacheKeyPrefix + phoneNumber;
        var now = DateTimeOffset.UtcNow;
        var cutoff = now - window;

        if (memoryCache.TryGetValue(cacheKey, out List<DateTimeOffset>? requests) && requests is not null)
        {
            var valid = requests.Where(r => r > cutoff).ToList();
            valid.Add(now);
            memoryCache.Set(cacheKey, valid, window);
            return;
        }

        memoryCache.Set(cacheKey, new List<DateTimeOffset> { now }, window);
    }
}
