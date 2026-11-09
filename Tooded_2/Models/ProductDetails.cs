namespace Tooded_2;
public sealed record ProductDetails
{
    public string SourceKey { get; init; } = "";
    public string SourceUrl { get; init; } = "";
    public string PhotoUrl { get; init; } = "";
    public string Category { get; init; } = "";
    public string Kind { get; init; } = "";
    public string Brand { get; init; } = "";
    public CatalogText Name { get; init; } = new();
    public CatalogText Description { get; init; } = new();
    public Dictionary<string, string> Specs { get; init; } = new();
    public bool Visible { get; init; } = true;
    public bool ReferenceConfiguration { get; init; }
}
