namespace Tooded_2;

/// <summary>Проверяет, подходит ли товар под выбранные покупателем фильтры.</summary>
public sealed record CatalogFilter
{
    public string Category { get; init; } = "";
    public string Query { get; init; } = "";
    public decimal Minimum { get; init; }
    public decimal Maximum { get; init; } = 1000000;
    public bool InStock { get; init; }
    public Dictionary<string, HashSet<string>> Facets { get; init; } = new();

    public bool Matches(ShopProduct product)
    {
        if (!product.Details.Visible || !CatalogRules.IsTechnical(product)) return false;
        if (Category.Length > 0 && product.Details.Category != Category) return false;
        if (InStock && product.Stock <= 0) return false;
        if (product.Price < Minimum || product.Price > Maximum) return false;

        string searchableText = string.Join(" ", product.Name, product.DisplayName,
            product.Details.Brand, string.Join(" ", product.Details.Specs.Values));
        if (!searchableText.Contains(Query.Trim(), StringComparison.OrdinalIgnoreCase)) return false;

        // Разные характеристики соединяются через И; значения одной характеристики — через ИЛИ.
        foreach (var filter in Facets)
        {
            string productValue = CatalogRules.FacetValue(product, filter.Key);
            if (filter.Value.Count > 0 && !filter.Value.Contains(productValue)) return false;
        }
        return true;
    }
}
