using AracParki.Application.Catalog.Dtos;
using AracParki.Application.Listings;

namespace AracParki.UnitTests;

public sealed class SpecDisplayTests
{
    [Theory]
    [InlineData("steel_track", "Çelik palet")]
    [InlineData("diesel", "Dizel")]
    [InlineData("powershift", "Powershift şanzıman")]
    public void SpecOptionLabels_maps_keys(string key, string expected)
        => Assert.Equal(expected, SpecOptionLabels.For(key));

    [Fact]
    public void ToDisplayRows_uses_turkish_enum_labels()
    {
        var attrs = new List<CategoryAttributeDto>
        {
            new()
            {
                Id = 1,
                Key = "undercarriage",
                Label = "Yürüyüş",
                DataType = "enum",
                EnumOptionsJson = """["steel_track","rubber_track"]"""
            },
            new()
            {
                Id = 2,
                Key = "ac_cabin",
                Label = "Klimalı kabin",
                DataType = "bool"
            }
        };

        var rows = SpecsJsonBuilder.ToDisplayRows(
            """{"undercarriage":"steel_track","ac_cabin":true}""",
            attrs);

        Assert.Equal(2, rows.Count);
        Assert.Equal("Yürüyüş", rows[0].Label);
        Assert.Equal("Çelik palet", rows[0].Value);
        Assert.Equal("Klimalı kabin", rows[1].Label);
        Assert.Equal("Evet", rows[1].Value);
    }
}
