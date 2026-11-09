namespace Tooded_2;
public sealed record CatalogDownload(IReadOnlyList<ShopProduct> Products, string StatusKey)
{
    public string Status => UiText.Get(StatusKey);
}
