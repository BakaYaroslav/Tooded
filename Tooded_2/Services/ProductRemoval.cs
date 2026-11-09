namespace Tooded_2;
public static class ProductRemoval
{
    public static void Apply(ShopConfiguration configuration, int productId)
    {
        if (configuration.Products.TryGetValue(productId, out var details) && details.SourceKey.Length > 0)
            configuration.ImportedKeys.TryAdd(details.SourceKey, productId);
        // Keep the imported identity as a tombstone so opening the catalog cannot resurrect the product.
        var keys = configuration.ImportedKeys.Where(p => p.Value == productId).Select(p => p.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        configuration.Products.Remove(productId);
        configuration.Promotions = configuration.Promotions.Select(offer =>
        {
            if (string.IsNullOrWhiteSpace(offer.ProductKeys))
                return offer;
            var selected = offer.ProductKeys.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var remaining = selected.Where(key => !keys.Contains(key)).ToArray();
            if (remaining.Length == selected.Length)
                return offer;
            // Removing the last explicit target must never broaden a discount to the whole category.
            return offer with
            {
                ProductKeys = string.Join(",", remaining),
                Enabled = offer.Enabled && remaining.Length > 0
            };
        }).ToList();
    }
}
