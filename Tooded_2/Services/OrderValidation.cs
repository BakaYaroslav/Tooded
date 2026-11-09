namespace Tooded_2;
public static class OrderValidation
{
    public static void Validate(IReadOnlyList<CartLine> lines, IReadOnlyList<ShopProduct> products)
    {
        if (lines.Count == 0)
            throw new InvalidOperationException(UiText.Get("YourBasketIsEmpty"));
        if (lines.Select(l => l.Product.Id).Distinct().Count() != lines.Count)
            throw new InvalidOperationException(UiText.Get("DuplicateCartLines"));
        foreach (var line in lines)
        {
            var current = products.SingleOrDefault(p => p.Id == line.Product.Id);
            if (current == null || line.Quantity <= 0 || current.Stock < line.Quantity)
                throw new InvalidOperationException(UiText.Get("StockChanged"));
            if (current.Price < 0 || current.Price != line.Product.Price)
                throw new InvalidOperationException(UiText.Get("PriceChanged"));
        }
    }
}
