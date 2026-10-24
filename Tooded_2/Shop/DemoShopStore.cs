namespace Tooded_2.Shop;

/// <summary>Explicit offline mode. Stock changes last only until the application closes.</summary>
public sealed class DemoShopStore(IEnumerable<ShopProduct> catalog) : IShopStore
{
    private readonly object gate = new();
    private List<ShopProduct> products = catalog.ToList();
    public bool IsDemoCatalog => true;
    public IReadOnlyList<ShopProduct> GetProducts()
    {
        lock (gate) return products.ToArray();
    }

    public DemoOrder Checkout(IReadOnlyList<CartLine> lines)
    {
        lock (gate)
        {
            OrderValidation.Validate(lines, products);
            var order = DemoOrder.Create(lines);
            var quantities = lines.ToDictionary(l => l.Product.Id, l => l.Quantity);
            products = products.Select(p => p with { Stock = p.Stock - quantities.GetValueOrDefault(p.Id) }).ToList();
            return order;
        }
    }

    public void Dispose() { }
}
