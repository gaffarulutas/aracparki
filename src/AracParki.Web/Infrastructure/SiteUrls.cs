using AracParki.Application.Email;
using Microsoft.Extensions.Options;

namespace AracParki.Web.Infrastructure;

/// <summary>Absolute public URLs for SEO (canonical, Open Graph) and emails.</summary>
public sealed class SiteUrls(IOptions<AppSettings> app, IHttpContextAccessor http)
{
    private readonly string _configured = (app.Value.PublicBaseUrl ?? string.Empty).TrimEnd('/');

    public string Origin
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_configured))
            {
                return _configured;
            }

            var req = http.HttpContext?.Request;
            if (req is null)
            {
                return "https://www.aracparki.com";
            }

            return $"{req.Scheme}://{req.Host.Value}".TrimEnd('/');
        }
    }

    public string Absolute(string? pathAndQuery)
    {
        if (string.IsNullOrWhiteSpace(pathAndQuery))
        {
            return Origin + "/";
        }

        if (pathAndQuery.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || pathAndQuery.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return pathAndQuery;
        }

        return Origin + (pathAndQuery.StartsWith('/') ? pathAndQuery : "/" + pathAndQuery);
    }

    public string CanonicalFromRequest(bool includeQuery = true)
    {
        var req = http.HttpContext?.Request;
        if (req is null)
        {
            return Origin + "/";
        }

        var path = $"{req.PathBase}{req.Path}";
        if (string.IsNullOrEmpty(path))
        {
            path = "/";
        }

        var query = includeQuery && req.QueryString.HasValue
            ? req.QueryString.Value
            : string.Empty;

        return Absolute(path + query);
    }
}
