using AracParki.Domain.Listings;

namespace AracParki.UnitTests;

public sealed class ListingIntentTests
{
    [Theory]
    [InlineData(ListingIntent.Satilik, "Satılık")]
    [InlineData(ListingIntent.Kiralik, "Kiralık")]
    [InlineData(ListingIntent.All, "Tümü")]
    public void Label_maps_known_intents(string intent, string expected)
    {
        Assert.Equal(expected, ListingIntent.Label(intent));
    }
}

public sealed class EquipmentConditionTests
{
    [Theory]
    [InlineData(EquipmentCondition.New, "Sıfır")]
    [InlineData(EquipmentCondition.Used, "İkinci el")]
    public void Label_maps_conditions(string condition, string expected)
    {
        Assert.Equal(expected, EquipmentCondition.Label(condition));
    }
}
