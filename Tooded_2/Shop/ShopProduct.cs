namespace Tooded_2.Shop;

public sealed record ShopProduct
{
    public ProductDetails Details { get; init; } = new();
    public decimal? OriginalPrice { get; init; }
    public string? PromotionId { get; init; }
    public string DisplayName => string.IsNullOrWhiteSpace(Details.Name.Value) ? Name : Details.Name.Value;
    public string DisplayDescription => string.IsNullOrWhiteSpace(Details.Description.Value) ? Description : Details.Description.Value;
    public string DisplayCategory => CatalogRules.IsTechnical(this) ? UiText.Category(Details.Category) : Category;
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
        if (quantity <= 0) throw new InvalidOperationException(UiText.Get("SelectAtLeastOneProduct"));
        SetQuantity(product, checked(Quantity(product.Id) + quantity));
    }

    public void SetQuantity(ShopProduct product, int quantity)
    {
        if (quantity < 0 || quantity > product.Stock)
            throw new InvalidOperationException(string.Format(UiText.Get("Product01InStock"), product.DisplayName, product.Stock));
        if (product.Price < 0) throw new InvalidOperationException(UiText.Get("TheProductPriceCannotBeNegative"));
        if (quantity == 0) lines.Remove(product.Id);
        else lines[product.Id] = new CartLine(product, quantity);
    }

    public void Remove(int productId) => lines.Remove(productId);
    public void Clear() => lines.Clear();
}

public sealed record DemoOrder(string Number, DateTimeOffset Created, IReadOnlyList<CartLine> Lines)
{
    public string CustomerEmail { get; init; } = "";
    public string CultureName { get; init; } = System.Globalization.CultureInfo.CurrentUICulture.Name;
    public DateOnly ExpectedDelivery => DateOnly.FromDateTime(Created.LocalDateTime).AddDays(14);
    public decimal Total => Lines.Sum(l => l.Total);
    public string Receipt => string.Join(Environment.NewLine,
        new[] { UiText.Get("Shoping67DEMOORDER"), $"{UiText.Get("Order")}: {Number}", $"{UiText.Get("Date")}: {Created:dd.MM.yyyy HH:mm}",
            UiText.Get("DEMOPAYMENTNoMoneyWasCharged"), "" }
        .Concat(new[] { UiText.Get("YourOrderIsBeingProcessed"), UiText.Format("EstimatedDelivery0DdMMYyyyInAboutTwoWeeks_1A8403", ExpectedDelivery), CustomerEmail.Length > 0 ? UiText.Get("Email") + CustomerEmail : "" })
        .Concat(Lines.Select(l => $"{l.Product.DisplayName} | {l.Quantity} × {l.Product.Price:0.00} € = {l.Total:0.00} €"))
        .Concat(new[] { "", $"{UiText.Get("TOTAL")}: {Total:0.00} €", UiText.Get("ThisIsADemoOrderNoGoodsAreShipped") }));

    public static DemoOrder Create(IReadOnlyList<CartLine> lines) =>
        new($"DEMO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            DateTimeOffset.Now, lines.ToArray());
}

public interface IShopStore : IDisposable
{
    bool IsDemoCatalog { get; }
    string CatalogStatus => IsDemoCatalog
        ? UiText.Get("DemoCatalogStockResetsWhenTheApplicationIsReopened")
        : UiText.Get("ShopCatalogDemoOrdersReduceStockAndAppearInSales");
    IReadOnlyList<ShopProduct> GetProducts();
    IReadOnlyList<Promotion> GetPromotions() => Array.Empty<Promotion>();
    DemoOrder Checkout(IReadOnlyList<CartLine> lines);
}

public static class OrderValidation
{
    public static void Validate(IReadOnlyList<CartLine> lines, IReadOnlyList<ShopProduct> products)
    {
        if (lines.Count == 0) throw new InvalidOperationException(UiText.Get("YourBasketIsEmpty"));
        if (lines.Select(l => l.Product.Id).Distinct().Count() != lines.Count)
            throw new InvalidOperationException(UiText.Get("TheBasketContainsDuplicateProductLines"));
        foreach (var line in lines)
        {
            var current = products.SingleOrDefault(p => p.Id == line.Product.Id);
            if (current == null || line.Quantity <= 0 || current.Stock < line.Quantity)
                throw new InvalidOperationException(UiText.Get("StockChangedRefreshTheCatalogAndBasket"));
            if (current.Price < 0 || current.Price != line.Product.Price)
                throw new InvalidOperationException(UiText.Get("ThePriceOrPromotionChangedAddTheProductToYourBasketAgain"));
        }
    }
}
