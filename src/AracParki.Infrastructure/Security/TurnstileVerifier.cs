using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AracParki.Application.Abstractions;
using AracParki.Application.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AracParki.Infrastructure.Security;

public sealed class TurnstileVerifier(
    IHttpClientFactory httpClientFactory,
    IOptions<TurnstileSettings> options,
    ILogger<TurnstileVerifier> logger) : ITurnstileVerifier
{
    public const string HttpClientName = "CloudflareTurnstile";
    private const string SiteVerifyPath = "turnstile/v0/siteverify";

    public async Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
        {
            logger.LogError("CloudflareTurnstile is not configured (SiteKey/Secret).");
            return false;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var fields = new Dictionary<string, string>
        {
            ["secret"] = settings.Secret,
            ["response"] = token
        };
        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            fields["remoteip"] = remoteIp;
        }

        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var content = new FormUrlEncodedContent(fields);
            using var response = await client.PostAsync(SiteVerifyPath, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Turnstile siteverify HTTP {Status}", (int)response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<SiteVerifyResponse>(cancellationToken);
            if (result?.Success == true)
            {
                return true;
            }

            logger.LogInformation(
                "Turnstile siteverify failed: {Errors}",
                result?.ErrorCodes is { Length: > 0 } codes ? string.Join(',', codes) : "unknown");
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Turnstile siteverify request failed");
            return false;
        }
    }

    private sealed class SiteVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
