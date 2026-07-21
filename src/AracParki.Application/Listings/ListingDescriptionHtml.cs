using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace AracParki.Application.Listings;

/// <summary>
/// Sanitizes Quill HTML for listing descriptions and supports legacy plain-text rows.
/// Uses AngleSharp 1.5.2+ (GHSA-pgww-w46g-26qg patched) with a strict allowlist —
/// no HtmlSanitizer / AngleSharp.Css beta chain.
/// </summary>
public static partial class ListingDescriptionHtml
{
    public const int MaxLength = 8000;

    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "P", "BR", "STRONG", "B", "EM", "I", "U", "UL", "OL", "LI", "A",
    };

    private static readonly HashSet<string> DropTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "SCRIPT", "STYLE", "IFRAME", "OBJECT", "EMBED", "LINK", "META", "BASE",
        "FORM", "INPUT", "BUTTON", "TEXTAREA", "SELECT", "SVG", "MATH", "TEMPLATE",
    };

    private static readonly HashSet<string> AllowedSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "http", "https", "mailto",
    };

    private static readonly HtmlParser Parser = new();

    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "";
        }

        var document = Parser.ParseDocument("<!DOCTYPE html><html><body></body></html>");
        var body = document.Body!;
        body.InnerHtml = input;

        SanitizeElement(body);

        var sanitized = body.InnerHtml.Trim();
        return IsEffectivelyEmpty(sanitized) ? "" : sanitized;
    }

    public static bool IsBlank(string? input) => string.IsNullOrWhiteSpace(Sanitize(input));

    public static string ToPlainText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "";
        }

        if (!LooksLikeHtml(input))
        {
            return input.Trim();
        }

        var sanitized = Sanitize(input);
        if (sanitized.Length == 0)
        {
            return "";
        }

        var withBreaks = BlockBreakRegex().Replace(sanitized, "\n");
        var stripped = TagRegex().Replace(withBreaks, "");
        return WebUtility.HtmlDecode(stripped).Trim();
    }

    /// <summary>
    /// Trusted HTML fragment for Razor <c>Html.Raw</c>. Legacy plain text is encoded + &lt;br&gt;.
    /// </summary>
    public static string ToSafeDisplayHtml(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return "";
        }

        if (!LooksLikeHtml(stored))
        {
            return WebUtility.HtmlEncode(stored).Replace("\n", "<br>\n", StringComparison.Ordinal);
        }

        return Sanitize(stored);
    }

    public static bool LooksLikeHtml(string value) =>
        value.AsSpan().TrimStart().StartsWith("<", StringComparison.Ordinal);

    private static void SanitizeElement(IElement element)
    {
        foreach (var child in element.ChildNodes.ToArray())
        {
            switch (child)
            {
                case IElement childElement:
                    SanitizeElement(childElement);
                    break;
                case IComment or IDocumentType:
                    child.RemoveFromParent();
                    break;
            }
        }

        if (element.LocalName is "body" or "html")
        {
            return;
        }

        if (!AllowedTags.Contains(element.NodeName))
        {
            if (DropTags.Contains(element.NodeName))
            {
                element.Remove();
            }
            else
            {
                UnwrapElement(element);
            }

            return;
        }

        var href = element.NodeName == "A" ? element.GetAttribute("href") : null;
        var target = element.NodeName == "A" ? element.GetAttribute("target") : null;

        foreach (var name in element.Attributes.Select(a => a.Name).ToArray())
        {
            element.RemoveAttribute(name);
        }

        if (element.NodeName != "A")
        {
            return;
        }

        if (TryNormalizeHref(href, out var safeHref))
        {
            element.SetAttribute("href", safeHref);
            element.SetAttribute("rel", "noopener noreferrer");
            if (string.Equals(target, "_blank", StringComparison.OrdinalIgnoreCase))
            {
                element.SetAttribute("target", "_blank");
            }
        }
        else
        {
            UnwrapElement(element);
        }
    }

    private static bool TryNormalizeHref(string? href, out string safeHref)
    {
        safeHref = "";
        if (string.IsNullOrWhiteSpace(href))
        {
            return false;
        }

        var trimmed = href.Trim();
        if (trimmed.StartsWith('#')
            || trimmed.StartsWith('/')
            || trimmed.StartsWith("./", StringComparison.Ordinal)
            || trimmed.StartsWith("../", StringComparison.Ordinal))
        {
            if (trimmed.Contains(':', StringComparison.Ordinal))
            {
                return false;
            }

            safeHref = trimmed;
            return true;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!AllowedSchemes.Contains(uri.Scheme))
        {
            return false;
        }

        safeHref = uri.AbsoluteUri;
        return true;
    }

    private static void UnwrapElement(IElement element)
    {
        var parent = element.Parent;
        if (parent is null)
        {
            element.Remove();
            return;
        }

        while (element.HasChildNodes)
        {
            parent.InsertBefore(element.FirstChild!, element);
        }

        element.Remove();
    }

    private static bool IsEffectivelyEmpty(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return true;
        }

        var text = TagRegex().Replace(html, "");
        text = WebUtility.HtmlDecode(text);
        text = text.Replace('\u00a0', ' ').Trim();
        return text.Length == 0;
    }

    [GeneratedRegex(@"</?(?:p|div|li|h[1-6]|ul|ol|br\s*/?)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BlockBreakRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.CultureInvariant)]
    private static partial Regex TagRegex();
}
