namespace Tooded_2.Shop;

/// <summary>Downloads once per session so refresh cannot reset purchased stock.</summary>
public sealed class OnlineDemoShopStore : IShopStore
{
    private DemoShopStore? inner;
    public bool IsDemoCatalog => true;
    public string CatalogStatus { get; private set; } = "Veebikataloogi laadimine…";

    public IReadOnlyList<ShopProduct> GetProducts()
    {
        // Called on the catalog worker, never on the UI thread.
        if (inner == null)
        {
            var result = TechCatalog.LoadAsync().GetAwaiter().GetResult();
            inner = new DemoShopStore(result.Products, TechCatalog.InitialPromotions());
            CatalogStatus = result.Status;
        }
        return inner.GetProducts();
    }

    public IReadOnlyList<Promotion> GetPromotions() => inner?.GetPromotions() ?? Array.Empty<Promotion>();

    public DemoOrder Checkout(IReadOnlyList<CartLine> lines) =>
        (inner ?? throw new InvalidOperationException("Kataloogi alles laaditakse.")).Checkout(lines);

    public void Dispose() => inner?.Dispose();
}
