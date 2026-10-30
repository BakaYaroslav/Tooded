namespace Tooded_2.Shop;

public static class CatalogRepair
{
    public static void Reconcile(ShopConfiguration configuration, IReadOnlyDictionary<int, string> storedNames,
        IReadOnlyList<ShopProduct> catalog)
    {
        var known = catalog.ToDictionary(p => p.Details.SourceKey);
        foreach (var pair in configuration.Products.ToArray())
        {
            if (!storedNames.TryGetValue(pair.Key, out var name)) continue;
            var detail = pair.Value;
            var row = new ShopProduct { Name = name, Details = detail };
            bool retired = detail.SourceKey.StartsWith("ref:", StringComparison.OrdinalIgnoreCase)
                || detail.ReferenceConfiguration || !CatalogRules.IsTechnical(row);
            bool mismatchedSource = detail.SourceKey.StartsWith("dummyjson:") &&
                (!known.TryGetValue(detail.SourceKey, out var source)
                || !source.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                || source.Details.Category != detail.Category
                || !source.Details.Brand.Equals(detail.Brand, StringComparison.OrdinalIgnoreCase));
            if (retired || mismatchedSource)
            {
                // Preserve rows and sales history. Invalid imported identities may be safely reimported.
                configuration.Products[pair.Key] = detail with { Visible = false };
                if (configuration.ImportedKeys.GetValueOrDefault(detail.SourceKey) == pair.Key)
                    configuration.ImportedKeys.Remove(detail.SourceKey);
                continue;
            }
            if (known.TryGetValue(detail.SourceKey, out var model))
                configuration.Products[pair.Key] = detail with
                { PhotoUrl = model.Details.PhotoUrl, SourceUrl = model.Details.SourceUrl, ReferenceConfiguration = false };
        }
    }
}
