namespace Tooded_2.Shop;

public sealed record ShopProduct
{
    public ProductDetails Details { get; init; } = new();
    public decimal? OriginalPrice { get; init; }
    public string? PromotionId { get; init; }
    public string DisplayName => string.IsNullOrWhiteSpace(Details.Name.Value) ? Name : Details.Name.Value;
    public string DisplayDescription => string.IsNullOrWhiteSpace(Details.Description.Value) ? Description : Details.Description.Value;
    public string DisplayCategory => CatalogRules.IsTechnical(this) ? T.Category(Details.Category) : Category;
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
        if (quantity <= 0) throw new InvalidOperationException(T.Legacy("Vali vähemalt üks toode."));
        SetQuantity(product, checked(Quantity(product.Id) + quantity));
    }

    public void SetQuantity(ShopProduct product, int quantity)
    {
        if (quantity < 0 || quantity > product.Stock)
            throw new InvalidOperationException(string.Format(T.L("Toodet „{0}” on laos {1} tk.", "Товар «{0}»: в наличии {1} шт.", "Product {0}: {1} in stock."), product.DisplayName, product.Stock));
        if (product.Price < 0) throw new InvalidOperationException(T.Legacy("Toote hind ei saa olla negatiivne."));
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
        new[] { T.L("shoping67 · NÄIDISTELLIMUS", "shoping67 · УЧЕБНЫЙ ЗАКАЗ", "shoping67 · DEMO ORDER"), $"{T.L("Tellimus", "Заказ", "Order")}: {Number}", $"{T.L("Kuupäev", "Дата", "Date")}: {Created:dd.MM.yyyy HH:mm}",
            T.L("TASUTUD DEMOMAKSEGA · Raha ei võetud.", "УЧЕБНАЯ ОПЛАТА · Деньги не списаны.", "DEMO PAYMENT · No money was charged."), "" }
        .Concat(Lines.Select(l => $"{l.Product.DisplayName} | {l.Quantity} × {l.Product.Price:0.00} € = {l.Total:0.00} €"))
        .Concat(new[] { "", $"{T.L("KOKKU", "ИТОГО", "TOTAL")}: {Total:0.00} €", T.L("Tegemist on õppemängu tellimusega. Kaupu ei saadeta.", "Это учебный заказ. Товары не отправляются.", "This is a demo order. No goods are shipped.") }));

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
    IReadOnlyList<Promotion> GetPromotions() => Array.Empty<Promotion>();
    DemoOrder Checkout(IReadOnlyList<CartLine> lines);
}

public static class OrderValidation
{
    public static void Validate(IReadOnlyList<CartLine> lines, IReadOnlyList<ShopProduct> products)
    {
        if (lines.Count == 0) throw new InvalidOperationException(T.Legacy("Ostukorv on tühi."));
        if (lines.Select(l => l.Product.Id).Distinct().Count() != lines.Count)
            throw new InvalidOperationException(T.Legacy("Ostukorvis on korduvad tooteread."));
        foreach (var line in lines)
        {
            var current = products.SingleOrDefault(p => p.Id == line.Product.Id);
            if (current == null || line.Quantity <= 0 || current.Stock < line.Quantity)
                throw new InvalidOperationException(T.L("Laoseis muutus. Uuenda kataloogi ja ostukorvi.", "Наличие изменилось. Обновите каталог и корзину.", "Stock changed. Refresh the catalog and basket."));
            if (current.Price < 0 || current.Price != line.Product.Price)
                throw new InvalidOperationException(T.L("Hind või soodustus muutus. Lisa toode uuesti korvi.", "Цена или скидка изменилась. Добавьте товар в корзину заново.", "The price or promotion changed. Add the product to your basket again."));
        }
    }
}
