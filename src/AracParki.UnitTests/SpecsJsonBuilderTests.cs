using AracParki.Application.Catalog.Dtos;
using AracParki.Application.Listings;

namespace AracParki.UnitTests;

public sealed class SpecsJsonBuilderTests
{
    private static CategoryAttributeDto Attr(
        string key,
        string label,
        string dataType = "enum",
        bool required = false,
        string? enumJson = null) => new()
    {
        Id = 1,
        Key = key,
        Label = label,
        DataType = dataType,
        IsRequired = required,
        EnumOptionsJson = enumJson
    };

    [Fact]
    public void Empty_raw_ok_when_no_required_attributes()
    {
        var (ok, error, json, _) = SpecsJsonBuilder.TryBuild(null, [Attr("color", "Renk")]);
        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal("{}", json);
    }

    [Fact]
    public void Empty_raw_fails_when_required_attribute_missing()
    {
        var attrs = new[]
        {
            Attr("fuel", "Yakıt", "enum", required: true, enumJson: """["diesel","electric"]""")
        };
        var (ok, error, _, _) = SpecsJsonBuilder.TryBuild(null, attrs);
        Assert.False(ok);
        Assert.Contains("Yakıt", error);
    }

    [Fact]
    public void TryValidateJson_rejects_missing_required()
    {
        var attrs = new[]
        {
            Attr("fuel", "Yakıt", "enum", required: true, enumJson: """["diesel"]""")
        };
        var (ok, error, _) = SpecsJsonBuilder.TryValidateJson("{}", attrs);
        Assert.False(ok);
        Assert.Contains("Yakıt", error);
    }

    [Fact]
    public void TryValidateJson_accepts_complete_object()
    {
        var attrs = new[]
        {
            Attr("fuel", "Yakıt", "enum", required: true, enumJson: """["diesel","electric"]""")
        };
        var (ok, error, json) = SpecsJsonBuilder.TryValidateJson("""{"fuel":"diesel"}""", attrs);
        Assert.True(ok);
        Assert.Null(error);
        Assert.Contains("diesel", json);
    }
}
