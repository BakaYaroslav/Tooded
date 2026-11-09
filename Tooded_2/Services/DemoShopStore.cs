namespace Tooded_2;
/// <summary>Учебное хранилище: каталог загружается один раз, остатки живут до закрытия программы.</summary>
public sealed class DemoShopStore : IShopStore
{
    private readonly object stockLock = new();
    private List<ShopProduct>? products;
    private List<Promotion> promotions;
    private string? catalogStatusKey;
    // Готовый каталог передают тесты. Обычный --demo получает его через TechCatalog.
    public DemoShopStore(IEnumerable<ShopProduct>? catalog = null, IEnumerable<Promotion>? promotions = null)
    {
        products = catalog?.ToList();
        this.promotions = promotions?.ToList() ?? new();
    }

    public bool IsDemoCatalog => true;
    public string CatalogStatus => UiText.Get(catalogStatusKey ?? "DemoCatalogStatus");

    public IReadOnlyList<Promotion> GetPromotions() => promotions.ToArray();
    public IReadOnlyList<ShopProduct> GetProducts()
    {
        lock (stockLock)
        {
            if (products == null)
            {
                var catalog = TechCatalog.LoadAsync().GetAwaiter().GetResult();
                products = catalog.Products.ToList();
                promotions = TechCatalog.InitialPromotions();
                catalogStatusKey = catalog.StatusKey;
            }

            return products.Select(product => PromotionEngine.Apply(product, promotions, DateTimeOffset.UtcNow)).ToArray();
        }
    }

    public ShopOrder Checkout(IReadOnlyList<CartLine> lines)
    {
        lock (stockLock)
        {
            var currentProducts = GetProducts().Where(product => product.Details.Visible).ToArray();
            OrderValidation.Validate(lines, currentProducts);
            var order = ShopOrder.Create(lines);
            var quantities = lines.ToDictionary(line => line.Product.Id, line => line.Quantity);
            products = products!.Select(product => product with { Stock = product.Stock - quantities.GetValueOrDefault(product.Id) }).ToList();
            return order;
        }
    }

    public void Dispose()
    {
    }
}
