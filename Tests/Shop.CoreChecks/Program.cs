using Tooded_2.Shop;

int passed = 0;
void Check(string name, Action test)
{
    test(); passed++; Console.WriteLine($"PASS {name}");
}
void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}; actual {actual}");
}
void Reject(Action action)
{
    try { action(); } catch (InvalidOperationException) { return; }
    throw new Exception("Expected operation to be rejected");
}
ShopProduct Product(int id = 1, int stock = 5, decimal price = 19.99m) => new()
{ Id = id, Name = $"Product {id}", Stock = stock, Price = price };

Check("Adding the same product merges quantities and keeps decimal totals", () =>
{
    var cart = new ShoppingCart(); var p = Product(); cart.Add(p, 1); cart.Add(p, 2);
    Equal(1, cart.Lines.Count); Equal(3, cart.Count); Equal(59.97m, cart.Total);
});
Check("Quantities cannot exceed stock, be negative or be added as zero", () =>
{
    var cart = new ShoppingCart(); var p = Product(stock: 2); cart.Add(p, 2);
    Reject(() => cart.Add(p, 1)); Reject(() => cart.Add(p, 0)); Reject(() => cart.SetQuantity(p, -1));
    Reject(() => cart.Add(Product(id: 2, stock: 0), 1)); Equal(2, cart.Count);
});
Check("Editing, removing and clearing recalculate the cart", () =>
{
    var cart = new ShoppingCart(); var p = Product(); cart.Add(p, 3); cart.SetQuantity(p, 1); Equal(19.99m, cart.Total);
    cart.SetQuantity(p, 0); Equal(0, cart.Count); cart.Add(p, 1); cart.Remove(p.Id); Equal(0m, cart.Total);
    cart.Add(p, 1); cart.Clear(); Equal(0, cart.Lines.Count);
});
Check("Empty checkout and invalid prices are rejected", () =>
{
    using var store = new DemoShopStore(new[] { Product() }); Reject(() => store.Checkout(Array.Empty<CartLine>()));
    Reject(() => new ShoppingCart().Add(Product(price: -1), 1));
});
Check("Checkout uses current stock and is atomic for all products", () =>
{
    var first = Product(1, 3); var second = Product(2, 1);
    using var store = new DemoShopStore(new[] { first, second });
    Reject(() => store.Checkout(new[] { new CartLine(first, 1), new CartLine(second, 2) }));
    Equal(3, store.GetProducts()[0].Stock); Equal(1, store.GetProducts()[1].Stock);
});
Check("Stale price, missing product and duplicate lines are rejected", () =>
{
    var p = Product(); using var store = new DemoShopStore(new[] { p });
    Reject(() => store.Checkout(new[] { new CartLine(p with { Price = 1m }, 1) }));
    Reject(() => store.Checkout(new[] { new CartLine(Product(99), 1) }));
    Reject(() => store.Checkout(new[] { new CartLine(p, 1), new CartLine(p, 1) }));
    Equal(5, store.GetProducts()[0].Stock);
});
Check("Successful checkout updates stock and retains the immutable receipt after cart clears", () =>
{
    var p = Product(stock: 2); using var store = new DemoShopStore(new[] { p }); var cart = new ShoppingCart(); cart.Add(p, 2);
    var order = store.Checkout(cart.Lines); cart.Clear(); Equal(0, store.GetProducts()[0].Stock); Equal(39.98m, order.Total);
    Equal(2, order.Lines[0].Quantity); Equal(true, order.Number.StartsWith("DEMO-"));
    Equal(true, order.Receipt.Contains("Raha ei võetud")); Reject(() => store.Checkout(order.Lines));
});
Check("Concurrent checkouts cannot oversell the last item", () =>
{
    var p = Product(stock: 1); using var store = new DemoShopStore(new[] { p }); int successes = 0;
    Parallel.For(0, 10, _ =>
    {
        try { store.Checkout(new[] { new CartLine(p, 1) }); Interlocked.Increment(ref successes); }
        catch (InvalidOperationException) { }
    });
    Equal(1, successes); Equal(0, store.GetProducts()[0].Stock);
});
Check("The packaged catalog has descriptions, real image files and an out-of-stock example", () =>
{
    var catalog = ShopCatalog.Load(); Equal(6, catalog.Count); Equal(6, catalog.Select(p => p.Id).Distinct().Count());
    Equal(true, catalog.Any(p => p.Stock == 0));
    foreach (var product in catalog)
    {
        Equal(true, product.Description.Length > 80); Equal(true, product.Specifications.Count > 0);
        var path = Path.Combine(AppContext.BaseDirectory, "Images", product.Image!);
        var bytes = File.ReadAllBytes(path); Equal(true, bytes.Length > 1024 && bytes[0] == 0xff && bytes[1] == 0xd8);
    }
});
Check("Rejected checkout preserves the cart and does not produce a sale", () =>
{
    var product = Product(stock: 1); var cart = new ShoppingCart(); cart.Add(product, 1);
    using var store = new DemoShopStore(new[] { product with { Stock = 0 } });
    Reject(() => store.Checkout(cart.Lines)); Equal(1, cart.Count); Equal(19.99m, cart.Total);
});
Check("A complete catalog order totals correctly, decrements only selected stock and has a unique reference", () =>
{
    var catalog = ShopCatalog.Load(); var cart = new ShoppingCart(); using var store = new DemoShopStore(catalog);
    cart.Add(catalog[0], 2); cart.Add(catalog[2], 1);
    var order = store.Checkout(cart.Lines); Equal(362.99m, order.Total);
    Equal(10, store.GetProducts()[0].Stock); Equal(23, store.GetProducts()[2].Stock); Equal(catalog[1].Stock, store.GetProducts()[1].Stock);
    var next = store.Checkout(new[] { new CartLine(catalog[2], 1) }); Equal(false, order.Number == next.Number);
});

Check("Internet products map API fields, categories and demo inventory", () =>
{
    var products = OnlineCatalog.Parse("""[{"id":1,"title":"Headphones","price":12.345,"category":"electronics","description":"Test description","image":null}]""");
    Equal(1, products.Count); Equal("Headphones", products[0].Name);
    Equal("Elektroonika", products[0].Category); Equal(12.35m, products[0].Price);
    Equal(20, products[0].Stock); Equal("Test description", products[0].Description);
});
Check("Empty, duplicate and invalid internet catalogs are rejected", () =>
{
    foreach (var payload in new[]
    {
        "[]", "null", "[null]", """[{"id":1,"title":"X","price":-1}]""",
        """[{"id":1,"title":" "}]""", """[{"id":1,"title":"X"},{"id":1,"title":"Y"}]"""
    })
    {
        bool rejected = false;
        try { OnlineCatalog.Parse(payload); }
        catch (InvalidDataException) { rejected = true; }
        Equal(true, rejected);
    }
});
Check("Successful downloads cache products and network failures use the cache", () =>
{
    string directory = Path.Combine(Path.GetTempPath(), "Tooded-test-" + Guid.NewGuid().ToString("N"));
    try
    {
        using var handler = new CatalogHandler();
        using var client = new System.Net.Http.HttpClient(handler);
        var loader = new OnlineCatalog(client, directory);
        var first = loader.LoadAsync(false).GetAwaiter().GetResult();
        Equal(1, first.Products.Count); Equal(true, first.Status.Contains("internetist"));
        handler.Fail = true;
        var cached = loader.LoadAsync(false).GetAwaiter().GetResult();
        Equal(first.Products[0].Name, cached.Products[0].Name);
        Equal(true, cached.Status.Contains("varem alla laaditud"));
    }
    finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
});
Check("No internet and no cache uses bundled demo products or rejects database import", () =>
{
    string directory = Path.Combine(Path.GetTempPath(), "Tooded-test-" + Guid.NewGuid().ToString("N"));
    using var handler = new CatalogHandler { Fail = true };
    using var client = new System.Net.Http.HttpClient(handler);
    var loader = new OnlineCatalog(client, directory);
    Equal(6, loader.LoadAsync().GetAwaiter().GetResult().Products.Count);
    Reject(() => loader.LoadAsync(false).GetAwaiter().GetResult());
});

Console.WriteLine($"{passed} checks passed.");

sealed class CatalogHandler : System.Net.Http.HttpMessageHandler
{
    public bool Fail { get; set; }
    protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(
        System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (Fail) throw new System.Net.Http.HttpRequestException("Simulated offline connection");
        if (request.RequestUri?.AbsoluteUri != OnlineCatalog.Endpoint)
            throw new Exception("Unexpected internet request in the test");
        return Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new System.Net.Http.StringContent("""[{"id":1,"title":"Test headphones","category":"electronics","price":25.99,"description":"API product","image":null}]""")
        });
    }
}
