namespace Tooded_2;
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
