namespace Tooded_2;
public sealed record Promotion
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public CatalogText Title { get; init; } = new();
    public CatalogText Description { get; init; } = new();
    public string Category { get; init; } = "";
    public string ProductKeys { get; init; } = "";
    public decimal Percent { get; init; }
    public DateTimeOffset Starts { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset Ends { get; init; } = DateTimeOffset.UtcNow.AddDays(14);
    public bool Enabled { get; init; } = true;
    public int Position { get; init; }
    public string Image { get; init; } = "";

    public bool Active(DateTimeOffset now) => Enabled && Starts <= now && now < Ends;
    public bool Matches(ShopProduct product) => (Category.Length == 0 || Category == product.Details.Category) && (string.IsNullOrWhiteSpace(ProductKeys) || ProductKeys.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Contains(product.Details.SourceKey, StringComparer.OrdinalIgnoreCase));
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title.Et))
            throw new InvalidOperationException(UiText.Get("EnterAPromotionTitle"));
        if (Percent < 0 || Percent > 90 || Ends <= Starts || !CatalogRules.Categories.Prepend("").Contains(Category))
            throw new InvalidOperationException(UiText.Get("CheckDatesCategoryAndDiscount090"));
    }
}
