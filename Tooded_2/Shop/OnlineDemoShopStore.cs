namespace Tooded_2.Shop;

/// <summary>Downloads once per session so refresh cannot reset purchased stock.</summary>
public sealed class OnlineDemoShopStore : IShopStore
{
    private DemoShopStore? inner;
    public bool IsDemoCatalog => true;
    private string? catalogStatusKey;
    public string CatalogStatus => catalogStatusKey == null ? "shoping67" : UiText.Get(catalogStatusKey);

    public IReadOnlyList<ShopProduct> GetProducts()
    {
        // Called on the catalog worker, never on the UI thread.
        if (inner == null)
        {
            var result = TechCatalog.LoadAsync().GetAwaiter().GetResult();
            inner = new DemoShopStore(result.Products, TechCatalog.InitialPromotions());
            catalogStatusKey = result.StatusKey;
        }
        return inner.GetProducts();
    }

    public IReadOnlyList<Promotion> GetPromotions() => inner?.GetPromotions() ?? Array.Empty<Promotion>();

    public DemoOrder Checkout(IReadOnlyList<CartLine> lines) =>
        (inner ?? throw new InvalidOperationException(UiText.Get("TheCatalogIsStillLoading"))).Checkout(lines);

    public void Dispose() => inner?.Dispose();
}
