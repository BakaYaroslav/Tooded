using Tooded_2.Shop;

T.SetLanguage("et");
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


Check("Tech catalog contains only real brands in the three supported categories", () =>
{
    var products = TechCatalog.Reference(); Equal(30, products.Count);
    Equal(3, products.Select(p => p.Details.Category).Distinct().Count());
    Equal(true, products.All(p => !p.Details.ReferenceConfiguration && p.Details.Brand != "shoping67"));
    Equal(products.Count, products.Select(p => p.Details.SourceKey).Distinct().Count());
});
Check("RTX filters select the documented ASUS laptop SKU", () =>
{
    var filter = new CatalogFilter { Category = "computers", Facets = new() { ["gpu"] = new(StringComparer.OrdinalIgnoreCase) { "RTX 4070 Laptop GPU" } } };
    var products = TechCatalog.Reference().Where(filter.Matches).ToArray();
    Equal(1, products.Length); Equal("laptop", products[0].Details.Kind);
    Equal("RTX 4070 Laptop GPU", products[0].Details.Specs["gpu"]);
    Equal("ASUS ROG Strix G16 G614JI-AS94", products[0].Name);
});
Check("Facet values use OR within a facet and AND across facets, with price and stock", () =>
{
    var filter = new CatalogFilter { Category = "computers", Maximum = 2000, InStock = true,
        Facets = new() { ["gpu"] = new() { "RTX 4070 Laptop GPU", "RTX 4060 Laptop GPU" }, ["ram"] = new() { "16 GB" } } };
    Equal(1, TechCatalog.Reference().Count(filter.Matches));
    Equal(0, TechCatalog.Reference().Count((filter with { Query = "nonexistent GPU" }).Matches));
    Equal(true, new CatalogFilter { Query = "RTX 4070" }.Matches(TechCatalog.Reference().First(p => p.Details.Specs.GetValueOrDefault("gpu") == "RTX 4070 Laptop GPU")));
});
Check("Promotions honor start/end boundaries, category and products without stacking", () =>
{
    var now = DateTimeOffset.UtcNow;
    var p = TechCatalog.Reference().First(p => p.Details.Category == "accessories") with { Price = 100 };
    var first = new Promotion { Category = "accessories", Percent = 10, Starts = now.AddDays(-1), Ends = now.AddDays(1) };
    var second = first with { Id = "second", Percent = 20, ProductKeys = p.Details.SourceKey };
    Equal(80m, PromotionEngine.Apply(p, new[] { first, second }, now).Price);
    Equal(100m, PromotionEngine.Apply(p, new[] { first with { Ends = now } }, now).Price);
    Equal(100m, PromotionEngine.Apply(p, new[] { first with { Starts = now.AddSeconds(1) } }, now).Price);
    Equal(100m, PromotionEngine.Apply(p, new[] { first with { Enabled = false } }, now).Price);
    Equal(100m, PromotionEngine.Apply(p, new[] { first with { Category = "phones" } }, now).Price);
    Equal(100m, PromotionEngine.Apply(p, new[] { first with { ProductKeys = "another-key" } }, now).Price);
    Equal(90m, PromotionEngine.Apply(p, new[] { first with { Starts = now } }, now).Price);
});
Check("Discounts do not compound on refresh and validation rejects expired-price carts", () =>
{
    var now = DateTimeOffset.UtcNow; var p = Product(price: 100);
    var promotion = new Promotion { Percent = 10, Starts = now.AddDays(-1), Ends = now.AddDays(1) };
    var discounted = PromotionEngine.Apply(p, new[] { promotion }, now);
    Equal(90m, PromotionEngine.Apply(discounted, new[] { promotion }, now).Price);
    Reject(() => OrderValidation.Validate(new[] { new CartLine(discounted, 1) }, new[] { p }));
    using var store = new DemoShopStore(new[] { p }, new[] { promotion });
    var order = store.Checkout(new[] { new CartLine(store.GetProducts()[0], 2) });
    Equal(180m, order.Total); Equal(3, store.GetProducts()[0].Stock);
});
Check("Configuration roundtrip preserves admin translations, visibility, source keys and promotions", () =>
{
    var product = TechCatalog.Reference()[0];
    var config = new ShopConfiguration { Initialized = true,
        Products = new() { [9] = product.Details with { Visible = false } },
        ImportedKeys = new() { [product.Details.SourceKey] = 9 }, Promotions = TechCatalog.InitialPromotions() };
    var parsed = ShopConfiguration.Parse(ShopConfiguration.Serialize(config));
    Equal(false, parsed.Products[9].Visible); Equal(9, parsed.ImportedKeys[product.Details.SourceKey]);
    Equal(config.Promotions[0].Title.Ru, parsed.Promotions[0].Title.Ru);
});
Check("Unknown API specifications remain unknown instead of inventing a GPU or memory size", () =>
{
    var products = TechCatalog.ParseDummyJson("""{"products":[{"id":78,"title":"Apple MacBook Pro 14 Inch Space Grey","category":"laptops","price":700,"stock":3,"brand":"Apple","description":"Source description"}]}""", "computers");
    Equal(1, products.Count); Equal(0, products[0].Details.Specs.Count);
    Equal(false, products[0].Details.ReferenceConfiguration); Equal("dummyjson:78", products[0].Details.SourceKey);
});
Check("Estonian-only mode ignores old language choices and needs no foreign translations", () =>
{
    var product = Product() with { Details = new ProductDetails { Category = "phones", Name = new("Telefon", "Телефон", "Phone") } };
    var cart = new ShoppingCart(); cart.Add(product, 2);
    T.SetLanguage("ru"); Equal("et", T.Language); Equal("Telefon", product.DisplayName);
    T.SetLanguage("en"); Equal("Telefonid", T.Category("phones")); Equal("Vali", T.L("Vali", "Выбрать", "Select"));
    Equal("et-EE", System.Globalization.CultureInfo.CurrentCulture.Name);
    new Promotion { Title = new("Ainult eestikeelne pealkiri") }.Validate();
    Equal("", new LocalText("", "Русский текст", "English text").Value);
    Equal(2, cart.Count); Equal(39.98m, cart.Total);
});
Check("Invalid promotion dates, discounts and missing Estonian titles cannot be published", () =>
{
    var value = new Promotion { Title = new("ET", "RU", "EN") };
    value.Validate();
    Reject(() => (value with { Percent = 95 }).Validate());
    Reject(() => (value with { Ends = value.Starts }).Validate());
    Reject(() => (value with { Title = new("", "RU", "EN") }).Validate());
});


Check("All real catalog models have a bundled PNG photograph and a traceable source", () =>
{
    foreach (var product in TechCatalog.Reference())
    {
        Equal(true, Uri.TryCreate(product.Details.PhotoUrl, UriKind.Absolute, out _));
        Equal(true, Uri.TryCreate(product.Details.SourceUrl, UriKind.Absolute, out _));
        var bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Images", product.Image!));
        Equal(true, bytes.Length > 1024 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4e && bytes[3] == 0x47);
    }
});
Check("A reused source ID cannot turn a backpack into a phone", () =>
{
    var rows = TechCatalog.ParseDummyJson("""{"products":[{"id":121,"title":"Fjallraven Foldsack Backpack","brand":"Apple","category":"smartphones","price":109.95,"stock":20}]}""", "phones");
    Equal(0, rows.Count);
    var bad = new ShopProduct { Name = "Fjallraven Foldsack Backpack", Details = new ProductDetails { Category = "phones", Kind = "phone", Brand = "Apple" } };
    Equal(false, new CatalogFilter { Category = "phones" }.Matches(bad));
});
Check("Saved catalog repair archives invented models and mismatched source rows without deleting history", () =>
{
    var real = TechCatalog.Reference().First(p => p.Details.SourceKey == "dummyjson:121");
    var config = new ShopConfiguration
    {
        Products = new()
        {
            [1] = real.Details with { Name = new("Backpack", "Backpack", "Backpack") },
            [2] = real.Details with { SourceKey = "ref:pc-4070", Brand = "shoping67", ReferenceConfiguration = true },
            [3] = real.Details with { SourceKey = "local:3" }
        },
        ImportedKeys = new() { [real.Details.SourceKey] = 1, ["ref:pc-4070"] = 2 }
    };
    var names = new Dictionary<int, string> { [1] = "Fjallraven Foldsack Backpack", [2] = "shoping67 PC", [3] = real.Name };
    CatalogRepair.Reconcile(config, names, TechCatalog.Reference());
    Equal(3, config.Products.Count); Equal(false, config.Products[1].Visible); Equal(false, config.Products[2].Visible);
    Equal(true, config.Products[3].Visible); Equal(false, config.ImportedKeys.ContainsKey(real.Details.SourceKey));
    CatalogRepair.Reconcile(config, names, TechCatalog.Reference()); Equal(3, config.Products.Count);
});


Check("Promotion targeting rejects a phone in accessories and excludes hidden and retired products", () =>
{
    var catalog = TechCatalog.Reference();
    var phone = catalog.Single(p => p.Details.SourceKey == "dummyjson:123");
    var promotion = new Promotion { Title = new("ET", "RU", "EN"), Category = "accessories", ProductKeys = phone.Details.SourceKey, Percent = 15 };
    Reject(() => PromotionTargets.Validate(promotion, catalog));
    var corrected = promotion with { Category = "phones" };
    PromotionTargets.Validate(corrected, catalog);
    Equal(934.99m, PromotionEngine.Apply(phone with { Price = 1099.99m }, new[] { corrected }, DateTimeOffset.UtcNow).Price);
    Reject(() => PromotionTargets.Validate(corrected with { ProductKeys = "ref:apple-iphone-13-pro" }, catalog));
    Equal(false, PromotionTargets.Eligible(catalog.Select(p => p.Id == phone.Id ? p with { Details = p.Details with { Visible = false } } : p), "phones").Any(p => p.Id == phone.Id));
    Equal(true, PromotionTargets.Eligible(catalog, "accessories").All(p => p.Details.Category == "accessories"));
});


Check("Deleting a product removes its metadata but keeps import suppression after reload and repair", () =>
{
    var product = TechCatalog.Reference()[0];
    var config = new ShopConfiguration { Products = new() { [52] = product.Details }, ImportedKeys = new() { [product.Details.SourceKey] = 52 } };
    ProductRemoval.Apply(config, 52);
    var reloaded = ShopConfiguration.Parse(ShopConfiguration.Serialize(config));
    CatalogRepair.Reconcile(reloaded, new Dictionary<int, string>(), TechCatalog.Reference());
    Equal(false, reloaded.Products.ContainsKey(52));
    Equal(true, reloaded.ImportedKeys.ContainsKey(product.Details.SourceKey));
    Equal(52, reloaded.ImportedKeys[product.Details.SourceKey]);
    ProductRemoval.Apply(reloaded, 52); Equal(52, reloaded.ImportedKeys[product.Details.SourceKey]);
});
Check("Deleting the last selected promotion target disables its discount instead of applying it to all products", () =>
{
    var first = TechCatalog.Reference()[0]; var second = TechCatalog.Reference()[1];
    var single = new Promotion { ProductKeys = first.Details.SourceKey, Percent = 15 };
    var multiple = new Promotion { ProductKeys = first.Details.SourceKey + "," + second.Details.SourceKey, Percent = 20 };
    var all = new Promotion { Category = "computers", ProductKeys = "", Percent = 5 };
    var config = new ShopConfiguration { Products = new() { [52] = first.Details }, Promotions = new() { single, multiple, all } };
    ProductRemoval.Apply(config, 52);
    Equal(false, config.Promotions[0].Enabled);
    Equal(true, config.Promotions[1].Enabled); Equal(second.Details.SourceKey, config.Promotions[1].ProductKeys);
    Equal(true, config.Promotions[2].Enabled); Equal("", config.Promotions[2].ProductKeys);
});
Check("Deleting a legacy row without catalog metadata is supported and leaves unrelated products intact", () =>
{
    var product = TechCatalog.Reference()[0];
    var config = new ShopConfiguration { Products = new() { [53] = product.Details }, ImportedKeys = new() { [product.Details.SourceKey] = 53 } };
    ProductRemoval.Apply(config, 1); Equal(1, config.Products.Count); Equal(53, config.ImportedKeys[product.Details.SourceKey]);
});


Check("One-click demo payment validates email, commits stock once and sends one confirmation", () =>
{
    var product = Product(stock: 3); using var store = new DemoShopStore(new[] { product });
    var cart = new ShoppingCart(); cart.Add(product, 2); int sent = 0; DemoOrder? emailed = null;
    var flow = new CheckoutFlow(store, order => { sent++; emailed = order; return Task.CompletedTask; });
    var result = flow.PayAsync(cart, " buyer@example.com ").GetAwaiter().GetResult();
    Equal(true, result.EmailSent); Equal("buyer@example.com", result.Order.CustomerEmail);
    Equal(result.Order.Number, emailed!.Number); Equal(1, sent); Equal(0, cart.Count); Equal(1, store.GetProducts()[0].Stock);
    flow.RetryEmailAsync().GetAwaiter().GetResult(); Equal(1, sent);
    Reject(() => flow.PayAsync(cart, "buyer@example.com").GetAwaiter().GetResult()); Equal(1, store.GetProducts()[0].Stock);
});
Check("Mail failure does not undo or repeat payment and retry reuses the same order", () =>
{
    var product = Product(stock: 3); using var store = new DemoShopStore(new[] { product });
    var cart = new ShoppingCart(); cart.Add(product, 1); int attempts = 0;
    var flow = new CheckoutFlow(store, _ => ++attempts == 1 ? Task.FromException(new System.Net.Mail.SmtpException("Simulated mail failure")) : Task.CompletedTask);
    var first = flow.PayAsync(cart, "buyer@example.com").GetAwaiter().GetResult();
    Equal(false, first.EmailSent); Equal(0, cart.Count); Equal(2, store.GetProducts()[0].Stock);
    var second = flow.RetryEmailAsync().GetAwaiter().GetResult();
    Equal(true, second.EmailSent); Equal(first.Order.Number, second.Order.Number); Equal(2, attempts); Equal(2, store.GetProducts()[0].Stock);
});
Check("Invalid email and failed stock validation preserve the basket and send no email", () =>
{
    var product = Product(stock: 1); using var store = new DemoShopStore(new[] { product with { Stock = 0 } });
    var cart = new ShoppingCart(); cart.Add(product, 1); int sent = 0;
    var flow = new CheckoutFlow(store, _ => { sent++; return Task.CompletedTask; });
    Reject(() => flow.PayAsync(cart, "invalid").GetAwaiter().GetResult());
    Reject(() => flow.PayAsync(cart, "buyer@example.com").GetAwaiter().GetResult());
    Equal(1, cart.Count); Equal(0, sent); Equal(true, flow.Result == null);
    foreach (var invalid in new[] { "", "x@y", "Name <buyer@example.com>", "buyer@example.com\r\nBcc: someone@example.com" }) Equal(false, OrderEmail.ValidAddress(invalid));
});
Check("Repeated pay clicks while a mail is pending cannot create a second order", () =>
{
    var product = Product(stock: 3); using var store = new DemoShopStore(new[] { product });
    var cart = new ShoppingCart(); cart.Add(product, 1);
    var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    int sent = 0;
    var flow = new CheckoutFlow(store, _ => { sent++; entered.TrySetResult(true); return release.Task; });
    var first = flow.PayAsync(cart, "buyer@example.com"); entered.Task.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
    Reject(() => flow.PayAsync(cart, "buyer@example.com").GetAwaiter().GetResult());
    release.SetResult(true); first.GetAwaiter().GetResult(); Equal(1, sent); Equal(2, store.GetProducts()[0].Stock);
});
Check("Order email gives a two-week calendar delivery date, order reference and escaped item names", () =>
{
    var product = Product() with { Name = "<script>alert(1)</script>" };
    var order = new DemoOrder("DEMO-TEST", new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero), new[] { new CartLine(product, 1) }) { CustomerEmail = "buyer@example.com" };
    Equal(new DateOnly(2026, 11, 13), order.ExpectedDelivery);
    string html = OrderEmail.Html(order);
    Equal(true, html.Contains("13.11.2026")); Equal(true, html.Contains("on vastu võetud ja töös"));
    Equal(true, html.Contains("DEMO-TEST")); Equal(false, html.Contains("<script>")); Equal(true, html.Contains("&lt;script&gt;"));
    Equal(true, order.Receipt.Contains("13.11.2026"));
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
