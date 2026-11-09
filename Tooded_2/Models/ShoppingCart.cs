namespace Tooded_2;
public sealed class ShoppingCart
{
    private readonly Dictionary<int, CartLine> lines = new();
    public IReadOnlyList<CartLine> Lines => lines.Values.ToArray();
    public int Count => lines.Values.Sum(l => l.Quantity);
    public decimal Total => lines.Values.Sum(l => l.Total);

    public int Quantity(int productId) => lines.GetValueOrDefault(productId)?.Quantity ?? 0;
    public void Add(ShopProduct product, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException(UiText.Get("SelectAtLeastOneProduct"));
        SetQuantity(product, checked(Quantity(product.Id) + quantity));
    }

    public void SetQuantity(ShopProduct product, int quantity)
    {
        if (quantity < 0 || quantity > product.Stock)
            throw new InvalidOperationException(string.Format(UiText.Get("Product01InStock"), product.DisplayName, product.Stock));
        if (product.Price < 0)
            throw new InvalidOperationException(UiText.Get("TheProductPriceCannotBeNegative"));
        if (quantity == 0)
            lines.Remove(product.Id);
        else
            lines[product.Id] = new CartLine(product, quantity);
    }

    public void Remove(int productId) => lines.Remove(productId);
    public void Clear() => lines.Clear();
}
