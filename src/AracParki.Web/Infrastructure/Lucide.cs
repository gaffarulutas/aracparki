using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AracParki.Web.Infrastructure;

/// <summary>Renders vendored Lucide SVG icons (lucide-static@1.25.0).</summary>
public static partial class Lucide
{
    private static readonly ConcurrentDictionary<string, string> BodyCache = new(StringComparer.OrdinalIgnoreCase);
    private static string _iconsRoot = string.Empty;

    public static void Configure(IWebHostEnvironment env)
    {
        _iconsRoot = Path.Combine(env.WebRootPath, "lib", "lucide", "icons");
    }

    public static string Svg(string name, string? cssClass = "ap-icon", int size = 24, string strokeWidth = "2")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var safe = name.Trim().ToLowerInvariant();
        if (!NamePattern().IsMatch(safe))
        {
            return string.Empty;
        }

        var body = BodyCache.GetOrAdd(safe, LoadBody);
        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }

        var cls = string.IsNullOrWhiteSpace(cssClass)
            ? $"lucide lucide-{safe}"
            : $"{cssClass} lucide lucide-{safe}";

        return $"<svg class=\"{cls}\" xmlns=\"http://www.w3.org/2000/svg\" width=\"{size}\" height=\"{size}\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"{strokeWidth}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{body}</svg>";
    }

    private static string LoadBody(string name)
    {
        if (string.IsNullOrEmpty(_iconsRoot))
        {
            return string.Empty;
        }

        var path = Path.Combine(_iconsRoot, name + ".svg");
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        var raw = File.ReadAllText(path);
        var match = SvgInner().Match(raw);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"<svg[^>]*>(.*)</svg>", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex SvgInner();
}

[HtmlTargetElement("ap-icon", TagStructure = TagStructure.WithoutEndTag)]
public sealed class LucideIconTagHelper : TagHelper
{
    public string Name { get; set; } = string.Empty;

    public string? Class { get; set; } = "ap-icon";

    public int Size { get; set; } = 20;

    public string StrokeWidth { get; set; } = "2";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.SetHtmlContent(Lucide.Svg(Name, Class, Size, StrokeWidth));
    }
}
