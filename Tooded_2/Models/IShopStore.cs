namespace Tooded_2;
public interface IShopStore : IDisposable
{
    bool IsDemoCatalog { get; }

    string CatalogStatus => IsDemoCatalog ? UiText.Get("DemoCatalogStatus") : UiText.Get("DatabaseCatalogStatus");

    IReadOnlyList<ShopProduct> GetProducts();
    IReadOnlyList<Promotion> GetPromotions() => Array.Empty<Promotion>();
    ShopOrder Checkout(IReadOnlyList<CartLine> lines);
}
