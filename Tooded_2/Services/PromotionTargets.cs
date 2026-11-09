namespace Tooded_2;
public static class PromotionTargets
{
    public static IReadOnlyList<ShopProduct> Eligible(IEnumerable<ShopProduct> products, string category) => products.Where(p => p.Details.Visible && CatalogRules.IsTechnical(p) && p.Details.SourceKey.Length > 0 && (category.Length == 0 || p.Details.Category == category)).ToArray();
    public static void Validate(Promotion promotion, IEnumerable<ShopProduct> catalog)
    {
        promotion.Validate();
        var eligible = Eligible(catalog, promotion.Category);
        var selected = promotion.ProductKeys.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (eligible.Count == 0 || selected.Any(key => !eligible.Any(p => p.Details.SourceKey.Equals(key, StringComparison.OrdinalIgnoreCase))))
            throw new InvalidOperationException(UiText.Get("PromotionTargetInvalid"));
    }
}
