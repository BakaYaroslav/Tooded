namespace Tooded_2.Shop;

public sealed record ShopProduct
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public string? Image { get; init; }
    public string Description { get; init; } = "";
    public Dictionary<string, string> Specifications { get; init; } = new();
}

public sealed record CartLine(ShopProduct Product, int Quantity)
{
    public decimal Total => Product.Price * Quantity;
}

public sealed class ShoppingCart
{
    private readonly Dictionary<int, CartLine> lines = new();
    public IReadOnlyList<CartLine> Lines => lines.Values.ToArray();
    public int Count => lines.Values.Sum(l => l.Quantity);
    public decimal Total => lines.Values.Sum(l => l.Total);
    public int Quantity(int productId) => lines.GetValueOrDefault(productId)?.Quantity ?? 0;

    public void Add(ShopProduct product, int quantity)
    {
        if (quantity <= 0) throw new InvalidOperationException("Vali vähemalt üks toode.");
        SetQuantity(product, checked(Quantity(product.Id) + quantity));
    }

    public void SetQuantity(ShopProduct product, int quantity)
    {
        if (quantity < 0 || quantity > product.Stock)
            throw new InvalidOperationException($"Toodet „{product.Name}” on laos {product.Stock} tk.");
        if (product.Price < 0) throw new InvalidOperationException("Toote hind ei saa olla negatiivne.");
        if (quantity == 0) lines.Remove(product.Id);
        else lines[product.Id] = new CartLine(product, quantity);
    }

    public void Remove(int productId) => lines.Remove(productId);
    public void Clear() => lines.Clear();
}

public sealed record DemoOrder(string Number, DateTimeOffset Created, IReadOnlyList<CartLine> Lines)
{
    public decimal Total => Lines.Sum(l => l.Total);
    public string Receipt => string.Join(Environment.NewLine,
        new[] { "TOODED · NÄIDISTELLIMUS", $"Tellimus: {Number}", $"Kuupäev: {Created:dd.MM.yyyy HH:mm}",
            "TASUTUD DEMOMAKSEGA · Raha ei võetud.", "" }
        .Concat(Lines.Select(l => $"{l.Product.Name} | {l.Quantity} × {l.Product.Price:0.00} € = {l.Total:0.00} €"))
        .Concat(new[] { "", $"KOKKU: {Total:0.00} €", "Tegemist on õppemängu tellimusega. Kaupu ei saadeta." }));

    public static DemoOrder Create(IReadOnlyList<CartLine> lines) =>
        new($"DEMO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            DateTimeOffset.Now, lines.ToArray());
}

public interface IShopStore : IDisposable
{
    bool IsDemoCatalog { get; }
    string CatalogStatus => IsDemoCatalog
        ? "Näidiskataloog · Laoseis taastub rakenduse uuesti avamisel."
        : "Poe kataloog · Demotellimused vähendavad laoseisu ja kajastuvad müügis.";
    IReadOnlyList<ShopProduct> GetProducts();
    DemoOrder Checkout(IReadOnlyList<CartLine> lines);
}

public static class OrderValidation
{
    public static void Validate(IReadOnlyList<CartLine> lines, IReadOnlyList<ShopProduct> products)
    {
        if (lines.Count == 0) throw new InvalidOperationException("Ostukorv on tühi.");
        if (lines.Select(l => l.Product.Id).Distinct().Count() != lines.Count)
            throw new InvalidOperationException("Ostukorvis on korduvad tooteread.");
        foreach (var line in lines)
        {
            var current = products.SingleOrDefault(p => p.Id == line.Product.Id);
            if (current == null || line.Quantity <= 0 || current.Stock < line.Quantity)
                throw new InvalidOperationException($"Toodet „{line.Product.Name}” ei ole piisavalt laos. Uuenda kataloogi ja ostukorvi.");
            if (current.Price < 0 || current.Price != line.Product.Price)
                throw new InvalidOperationException($"Toote „{line.Product.Name}” hind muutus. Eemalda see korvist ja lisa uuesti.");
        }
    }
}
