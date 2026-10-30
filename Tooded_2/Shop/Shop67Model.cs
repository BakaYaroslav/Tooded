using System.Text.Json;

namespace Tooded_2.Shop;

public sealed record LocalText(string Et = "", string Ru = "", string En = "")
{
    public string Value => T.Language switch { "ru" => string.IsNullOrWhiteSpace(Ru) ? En : Ru, "en" => En, _ => string.IsNullOrWhiteSpace(Et) ? En : Et };
}

public sealed record ProductDetails
{
    public string SourceKey { get; init; } = "";
    public string SourceUrl { get; init; } = "";
    public string Category { get; init; } = "";
    public string Kind { get; init; } = "";
    public string Brand { get; init; } = "";
    public LocalText Name { get; init; } = new();
    public LocalText Description { get; init; } = new();
    public Dictionary<string, string> Specs { get; init; } = new();
    public bool Visible { get; init; } = true;
    public bool ReferenceConfiguration { get; init; }
}

public sealed record Promotion
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public LocalText Title { get; init; } = new();
    public LocalText Description { get; init; } = new();
    public string Category { get; init; } = "";
    public string ProductKeys { get; init; } = "";
    public decimal Percent { get; init; }
    public DateTimeOffset Starts { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset Ends { get; init; } = DateTimeOffset.UtcNow.AddDays(14);
    public bool Enabled { get; init; } = true;
    public int Position { get; init; }
    public string Image { get; init; } = "";
    public bool Active(DateTimeOffset now) => Enabled && Starts <= now && now < Ends;
    public bool Matches(ShopProduct product) => (Category.Length == 0 || Category == product.Details.Category)
        && (string.IsNullOrWhiteSpace(ProductKeys) || ProductKeys.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Contains(product.Details.SourceKey, StringComparer.OrdinalIgnoreCase));
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title.Et) || string.IsNullOrWhiteSpace(Title.Ru) || string.IsNullOrWhiteSpace(Title.En))
            throw new InvalidOperationException(T.L("Sisesta pealkiri kolmes keeles.", "Введи заголовок на трёх языках.", "Enter a title in all three languages."));
        if (Percent < 0 || Percent > 90 || Ends <= Starts || !CatalogRules.Categories.Prepend("").Contains(Category))
            throw new InvalidOperationException(T.L("Kontrolli kuupäevi, kategooriat ja allahindlust (0–90%).", "Проверь даты, категорию и скидку (0–90%).", "Check dates, category and discount (0–90%)."));
    }
}

public static class PromotionEngine
{
    public static ShopProduct Apply(ShopProduct product, IEnumerable<Promotion> promotions, DateTimeOffset now)
    {
        decimal basis = product.OriginalPrice ?? product.Price;
        var offer = promotions.Where(p => p.Active(now) && p.Matches(product) && p.Percent > 0 && p.Percent <= 90)
            .OrderByDescending(p => p.Percent).ThenBy(p => p.Position).FirstOrDefault();
        return product with { OriginalPrice = basis, Price = offer == null ? basis : decimal.Round(basis * (1 - offer.Percent / 100), 2, MidpointRounding.AwayFromZero), PromotionId = offer?.Id };
    }
}

public sealed record CatalogFilter
{
    public string Category { get; init; } = "";
    public string Query { get; init; } = "";
    public decimal Minimum { get; init; }
    public decimal Maximum { get; init; } = 1000000;
    public bool InStock { get; init; }
    public Dictionary<string, HashSet<string>> Facets { get; init; } = new();
    public bool Matches(ShopProduct p) => p.Details.Visible && CatalogRules.Categories.Contains(p.Details.Category)
        && (Category.Length == 0 || p.Details.Category == Category) && (!InStock || p.Stock > 0)
        && p.Price >= Minimum && p.Price <= Maximum
        && (Query.Length == 0 || string.Join(" ", p.Name, p.DisplayName, p.Details.Brand,
            string.Join(" ", p.Details.Specs.Values)).Contains(Query.Trim(), StringComparison.OrdinalIgnoreCase))
        && Facets.All(f => f.Value.Count == 0 || f.Value.Contains(CatalogRules.FacetValue(p, f.Key)));
}

public static class CatalogRules
{
    public static readonly string[] Categories = { "phones", "computers", "accessories" };
    public static string[] Facets(string category) => category switch
    {
        "phones" => new[] { "brand", "ram", "storage", "screen", "os", "5g" },
        "computers" => new[] { "kind", "brand", "gpu", "cpu", "ram", "storage", "screen", "os" },
        "accessories" => new[] { "kind", "brand", "connection", "layout", "switches", "microphone" },
        _ => new[] { "brand" }
    };
    public static string FacetValue(ShopProduct p, string key) => key switch
    { "brand" => p.Details.Brand, "kind" => p.Details.Kind, _ => p.Details.Specs.GetValueOrDefault(key, "") };
    public static bool IsTechnical(ShopProduct p) => Categories.Contains(p.Details.Category);
}

public sealed class ShopConfiguration
{
    public Dictionary<int, ProductDetails> Products { get; set; } = new();
    public Dictionary<string, int> ImportedKeys { get; set; } = new();
    public List<Promotion> Promotions { get; set; } = new();
    public bool Initialized { get; set; }
    public static string Serialize(ShopConfiguration value) => JsonSerializer.Serialize(value);
    public static ShopConfiguration Parse(string value) => JsonSerializer.Deserialize<ShopConfiguration>(value) ?? throw new InvalidDataException("Invalid shop settings");
}
