using System.Globalization;
using AracParki.Application.Listings.Queries;
using AracParki.Domain.Listings;
using Microsoft.AspNetCore.WebUtilities;

namespace AracParki.Web.Infrastructure;

public static class ListingRoutes
{
    public const string Home = "/";
    public const string List = "/ilanlar";
    public const string DetailPrefix = "/ilan";
    public const string SpecQueryPrefix = "oz_";

    public static string Detail(string adNo) => $"{DetailPrefix}/{Uri.EscapeDataString(adNo)}";

    public static string ListUrl(ListingSearchQuery query)
    {
        var dict = new Dictionary<string, string?>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(query.Intent) && query.Intent != ListingIntent.All)
        {
            dict["tip"] = query.Intent;
        }

        if (query.CategoryId is > 0)
        {
            dict["kategoriId"] = query.CategoryId.Value.ToString(CultureInfo.InvariantCulture);
        }
        else if (!string.IsNullOrWhiteSpace(query.Category))
        {
            dict["kategori"] = query.Category;
        }

        if (query.BrandId is > 0)
        {
            dict["markaId"] = query.BrandId.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (query.ModelId is > 0)
        {
            dict["modelId"] = query.ModelId.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (query.CityId is > 0)
        {
            dict["ilId"] = query.CityId.Value.ToString(CultureInfo.InvariantCulture);
        }
        else if (!string.IsNullOrWhiteSpace(query.City))
        {
            dict["il"] = query.City;
        }

        if (query.DistrictId is > 0)
        {
            dict["ilceId"] = query.DistrictId.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(query.Condition))
        {
            dict["durum"] = query.Condition;
        }

        if (!string.IsNullOrWhiteSpace(query.SellerType))
        {
            dict["satici"] = query.SellerType;
        }

        if (query.YearMin is not null) dict["yilMin"] = query.YearMin.Value.ToString(CultureInfo.InvariantCulture);
        if (query.YearMax is not null) dict["yilMax"] = query.YearMax.Value.ToString(CultureInfo.InvariantCulture);
        if (query.HoursMin is not null) dict["saatMin"] = query.HoursMin.Value.ToString(CultureInfo.InvariantCulture);
        if (query.HoursMax is not null) dict["saatMax"] = query.HoursMax.Value.ToString(CultureInfo.InvariantCulture);
        if (query.WeightMin is not null) dict["tonMin"] = query.WeightMin.Value.ToString(CultureInfo.InvariantCulture);
        if (query.WeightMax is not null) dict["tonMax"] = query.WeightMax.Value.ToString(CultureInfo.InvariantCulture);
        if (query.PriceMin is not null) dict["fiyatMin"] = query.PriceMin.Value.ToString(CultureInfo.InvariantCulture);
        if (query.PriceMax is not null) dict["fiyatMax"] = query.PriceMax.Value.ToString(CultureInfo.InvariantCulture);
        if (query.HorsepowerMin is not null) dict["hpMin"] = query.HorsepowerMin.Value.ToString(CultureInfo.InvariantCulture);
        if (query.HorsepowerMax is not null) dict["hpMax"] = query.HorsepowerMax.Value.ToString(CultureInfo.InvariantCulture);
        if (query.CapacityKgMin is not null) dict["kgMin"] = query.CapacityKgMin.Value.ToString(CultureInfo.InvariantCulture);
        if (query.CapacityKgMax is not null) dict["kgMax"] = query.CapacityKgMax.Value.ToString(CultureInfo.InvariantCulture);

        if (query.IncludesOperator is true)
        {
            dict["operator"] = "1";
        }

        if (!string.IsNullOrWhiteSpace(query.PriceUnit))
        {
            dict["birim"] = query.PriceUnit;
        }

        if (query.VerifiedOnly)
        {
            dict["dogrulanmis"] = "1";
        }

        if (query.AttachmentIds.Count > 0)
        {
            dict["ekipman"] = string.Join(',', query.AttachmentIds.Where(x => x > 0).Distinct());
        }

        foreach (var (key, value) in query.SpecValues)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            {
                dict[SpecQueryPrefix + key] = value;
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            dict["q"] = query.Query;
        }

        if (!string.IsNullOrWhiteSpace(query.Sort) && query.Sort != ListingSort.Newest)
        {
            dict["sort"] = query.Sort;
        }

        if (query.Page > 1)
        {
            dict["sayfa"] = query.Page.ToString(CultureInfo.InvariantCulture);
        }

        return dict.Count == 0 ? List : QueryHelpers.AddQueryString(List, dict);
    }

    public static ListingSearchQuery FromRequest(IQueryCollection query)
    {
        var tip = query["tip"].ToString();
        var sort = query["sort"].ToString();
        var pageRaw = query["sayfa"].ToString();
        var condition = NullIfEmpty(query["durum"].ToString());
        var sellerType = NullIfEmpty(query["satici"].ToString());
        var priceUnit = NullIfEmpty(query["birim"].ToString());

        if (!ListingIntent.Known.Contains(tip))
        {
            tip = ListingIntent.All;
        }

        if (!ListingSort.Known.Contains(sort))
        {
            sort = ListingSort.Newest;
        }

        if (condition is not null && !EquipmentCondition.Known.Contains(condition))
        {
            condition = null;
        }

        if (sellerType is not null && !SellerType.Known.Contains(sellerType))
        {
            sellerType = null;
        }

        if (priceUnit is not null && !PriceUnit.Known.Contains(priceUnit))
        {
            priceUnit = null;
        }

        var specValues = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in query.Keys)
        {
            if (!key.StartsWith(SpecQueryPrefix, StringComparison.Ordinal) || key.Length <= SpecQueryPrefix.Length)
            {
                continue;
            }

            var attrKey = key[SpecQueryPrefix.Length..];
            var value = NullIfEmpty(query[key].ToString());
            if (value is not null)
            {
                specValues[attrKey] = value;
            }
        }

        var attachmentIds = new List<int>();
        foreach (var raw in query["ekipman"])
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
                {
                    attachmentIds.Add(id);
                }
            }
        }

        var (equalityJson, minJson) = SpecFilterBuilder.Build(specValues);

        return new ListingSearchQuery
        {
            Intent = string.IsNullOrWhiteSpace(tip) ? ListingIntent.All : tip,
            CategoryId = ParseInt(query["kategoriId"]),
            BrandId = ParseInt(query["markaId"]),
            ModelId = ParseInt(query["modelId"]),
            CityId = ParseInt(query["ilId"]),
            DistrictId = ParseInt(query["ilceId"]),
            Category = NullIfEmpty(query["kategori"].ToString()),
            City = NullIfEmpty(query["il"].ToString()),
            Condition = condition,
            SellerType = sellerType,
            YearMin = ParseInt(query["yilMin"]),
            YearMax = ParseInt(query["yilMax"]),
            HoursMin = ParseInt(query["saatMin"]),
            HoursMax = ParseInt(query["saatMax"]),
            WeightMin = ParseDecimal(query["tonMin"]),
            WeightMax = ParseDecimal(query["tonMax"]),
            PriceMin = ParseDecimal(query["fiyatMin"]),
            PriceMax = ParseDecimal(query["fiyatMax"]),
            HorsepowerMin = ParseInt(query["hpMin"]),
            HorsepowerMax = ParseInt(query["hpMax"]),
            CapacityKgMin = ParseInt(query["kgMin"]),
            CapacityKgMax = ParseInt(query["kgMax"]),
            IncludesOperator = IsTruthy(query["operator"].ToString()) ? true : null,
            PriceUnit = priceUnit,
            VerifiedOnly = IsTruthy(query["dogrulanmis"].ToString()),
            AttachmentIds = attachmentIds.Distinct().ToArray(),
            SpecValues = specValues,
            SpecsFilterJson = equalityJson,
            SpecMinJson = minJson,
            Query = NullIfEmpty(query["q"].ToString()) is { Length: >= 2 } q ? q : null,
            Sort = string.IsNullOrWhiteSpace(sort) ? ListingSort.Newest : sort,
            Page = int.TryParse(pageRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var page) && page > 0
                ? Math.Min(page, 500)
                : 1,
            PageSize = 24
        };
    }

    public static string SpecOptionLabel(string value) => value switch
    {
        "steel_track" => "Çelik palet",
        "rubber_track" => "Kauçuk palet",
        "standard" => "Standart",
        "reduced" => "Kısaltılmış",
        "zero" => "Sıfır kuyruk",
        "diesel" => "Dizel",
        "lpg" => "LPG",
        "electric" => "Elektrik",
        "simplex" => "Simplex",
        "duplex" => "Duplex",
        "triplex" => "Triplex",
        "mobile" => "Mobil",
        "crawler" => "Paletli",
        "tower" => "Kule",
        _ => value.Replace("_", " ", StringComparison.Ordinal)
    };

    private static bool IsTruthy(string? value)
        => value is "1" or "true" or "True" or "on" or "yes";

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseInt(string? raw)
        => int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && v > 0 ? v : null;

    private static decimal? ParseDecimal(string? raw)
        => decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : null;
}

public static class CategoryIcons
{
    public static string Svg(string iconKey)
    {
        var body = iconKey switch
        {
            "backhoe" => "<circle cx=\"6.5\" cy=\"17.5\" r=\"2\"/><circle cx=\"15.5\" cy=\"17.5\" r=\"2\"/><path d=\"M4.5 15.5h13V11H9.5L7.5 13.5H4.5z\"/><path d=\"M11 11L16 5\"/><path d=\"M14 7.5h4\"/>",
            "loader" => "<circle cx=\"6.5\" cy=\"17.5\" r=\"2\"/><circle cx=\"14.5\" cy=\"17.5\" r=\"2\"/><path d=\"M4.5 15.5h12V10.5H8z\"/><path d=\"M12 10.5L17 6\"/><path d=\"M17 6h2.5v3\"/>",
            "forklift" => "<circle cx=\"7\" cy=\"17.5\" r=\"2\"/><circle cx=\"15\" cy=\"17.5\" r=\"2\"/><path d=\"M5 15.5h12V11H9z\"/><path d=\"M9 11V7h3\"/><path d=\"M17 8v8\"/><path d=\"M17 9h3\"/>",
            "crane" => "<path d=\"M6 21h6m-3 0V3L3 9h18M9 3l10 6\"/><path d=\"M17 9v4a2 2 0 1 1-2 2\"/>",
            "dozer" => "<path d=\"M4 13.5h10v4H4z\"/><path d=\"M5.5 16V10.5h8V16\"/><path d=\"M9 10.5V8h3.5\"/><path d=\"M17 9v9\"/><path d=\"M17 11h2.5v5H17\"/>",
            "grader" => "<circle cx=\"5.5\" cy=\"17.5\" r=\"2\"/><circle cx=\"18.5\" cy=\"17.5\" r=\"2\"/><path d=\"M4 15.5h16V11H10l-2 2.5H4z\"/><path d=\"M11 11V7.5h3.5\"/><path d=\"M3 18.8h18\"/>",
            "roller" => "<circle cx=\"7.5\" cy=\"15.5\" r=\"3.4\"/><circle cx=\"16.5\" cy=\"15.8\" r=\"2.8\"/><path d=\"M10.8 14h2.4\"/><path d=\"M16.5 13V9.5h2\"/><path d=\"M6 12.2V9.8h3\"/>",
            "mini" => "<circle cx=\"7\" cy=\"17.5\" r=\"2\"/><circle cx=\"15\" cy=\"17.5\" r=\"2\"/><path d=\"M5 15.5h12V12H8z\"/><path d=\"M10 12L14 7\"/><path d=\"M14 7h3\"/>",
            "lift" => "<circle cx=\"6.5\" cy=\"17.5\" r=\"2\"/><circle cx=\"13.5\" cy=\"17.5\" r=\"2\"/><path d=\"M4.5 15.5h11V11H8z\"/><path d=\"M10 11L18 5.5\"/><path d=\"M18 5.5h3\"/><path d=\"M21 5.5V8.5\"/>",
            "concrete" => "<circle cx=\"6.5\" cy=\"17.5\" r=\"2\"/><circle cx=\"16.5\" cy=\"17.5\" r=\"2\"/><path d=\"M4.5 15.5h15V12l-2.5-4H9.5L7.5 11H4.5z\"/><ellipse cx=\"13.5\" cy=\"9.5\" rx=\"3.2\" ry=\"2.6\"/>",
            "crusher" => "<circle cx=\"7\" cy=\"17.5\" r=\"2\"/><circle cx=\"14\" cy=\"17.5\" r=\"2\"/><path d=\"M5 15.5h11V11H9z\"/><path d=\"M12 11L17 5\"/><path d=\"M17 5l2.2 1.2-3.5 6\"/>",
            _ => "<path d=\"M4 16.5h10v2H4z\"/><path d=\"M6 16.5V10h7v6.5\"/><path d=\"M9 10L15 5\"/><path d=\"M15 5h3v3\"/>"
        };

        return "<svg class=\"cat-ico\" viewBox=\"0 0 24 24\" aria-hidden=\"true\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.7\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" + body + "</svg>";
    }
}
