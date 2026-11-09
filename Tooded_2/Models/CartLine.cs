namespace Tooded_2;
public sealed record CartLine(ShopProduct Product, int Quantity)
{
    public decimal Total => Product.Price * Quantity;
}
