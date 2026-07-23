namespace AracParki.Application.Abstractions;

public interface ITurnstileVerifier
{
    /// <summary>
    /// Verifies a <c>cf-turnstile-response</c> token with Cloudflare siteverify.
    /// Returns false when not configured, token missing, or Cloudflare rejects it.
    /// </summary>
    Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default);
}
