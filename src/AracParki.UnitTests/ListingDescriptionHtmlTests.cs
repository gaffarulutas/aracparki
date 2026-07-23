using AracParki.Application.Listings;

namespace AracParki.UnitTests;

public sealed class ListingDescriptionHtmlTests
{
    [Fact]
    public void Sanitize_strips_scripts_and_keeps_basic_markup()
    {
        var html = """<p>Bakımlı <strong>CAT</strong> <script>alert(1)</script></p><ul><li>Lastik yeni</li></ul>""";
        var sanitized = ListingDescriptionHtml.Sanitize(html);

        Assert.Contains("<strong>CAT</strong>", sanitized, StringComparison.Ordinal);
        Assert.Contains("<li>Lastik yeni</li>", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("script", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", sanitized, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_rejects_javascript_href()
    {
        var html = """<p><a href="javascript:alert(1)">tıkla</a></p>""";
        var sanitized = ListingDescriptionHtml.Sanitize(html);

        Assert.DoesNotContain("javascript", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<a", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tıkla", sanitized, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_keeps_https_links_with_noopener()
    {
        var html = """<p><a href="https://example.com" target="_blank">site</a></p>""";
        var sanitized = ListingDescriptionHtml.Sanitize(html);

        Assert.Contains("href=\"https://example.com/\"", sanitized, StringComparison.Ordinal);
        Assert.Contains("rel=\"noopener noreferrer\"", sanitized, StringComparison.Ordinal);
        Assert.Contains("target=\"_blank\"", sanitized, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_empty_quill_paragraph_is_blank()
    {
        Assert.True(ListingDescriptionHtml.IsBlank("<p><br></p>"));
        Assert.Equal("", ListingDescriptionHtml.Sanitize("<p><br></p>"));
    }

    [Fact]
    public void ToSafeDisplayHtml_encodes_legacy_plain_text()
    {
        var html = ListingDescriptionHtml.ToSafeDisplayHtml("Satılık <b>makine</b>\nTemiz");
        Assert.Contains("Satılık &lt;b&gt;makine&lt;/b&gt;", html, StringComparison.Ordinal);
        Assert.Contains("<br>", html, StringComparison.Ordinal);
    }

    [Fact]
    public void ToPlainText_from_html()
    {
        var text = ListingDescriptionHtml.ToPlainText("<p>Merhaba <strong>dünya</strong></p>");
        Assert.Contains("Merhaba", text, StringComparison.Ordinal);
        Assert.Contains("dünya", text, StringComparison.Ordinal);
        Assert.DoesNotContain("<", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_replaces_nbsp_with_regular_spaces()
    {
        var html = "<p>Caterpillar&nbsp;312D&nbsp;Ekskavatör</p>";
        var sanitized = ListingDescriptionHtml.Sanitize(html);

        Assert.DoesNotContain("&nbsp;", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\u00a0", sanitized, StringComparison.Ordinal);
        Assert.Contains("Caterpillar 312D Ekskavatör", sanitized, StringComparison.Ordinal);
    }
}
