namespace Tooded_2;
public static class CatalogRules
{
    public static readonly string[] Categories =
    {
        "phones",
        "computers",
        "accessories"
    };
    public static string[] Facets(string category) => category switch
    {
        "phones" => new[]
        {
            "brand",
            "ram",
            "storage",
            "screen",
            "os",
            "5g"
        },
        "computers" => new[]
        {
            "kind",
            "brand",
            "gpu",
            "cpu",
            "ram",
            "storage",
            "screen",
            "os"
        },
        "accessories" => new[]
        {
            "kind",
            "brand",
            "connection",
            "layout",
            "switches",
            "microphone"
        },
        _ => new[]
        {
            "brand"
        }
    };
    public static string FacetValue(ShopProduct p, string key) => key switch
    {
        "brand" => p.Details.Brand,
        "kind" => p.Details.Kind,
        _ => p.Details.Specs.GetValueOrDefault(key, "")};
    public static bool IsTechnical(ShopProduct p)
    {
        if (p.Details.Brand.Equals("shoping67", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("shoping67", StringComparison.OrdinalIgnoreCase))
            return false;
        string title = p.Name + " " + p.Details.Name.En;
        if (new[]
        {
            "backpack",
            "foldsack",
            "jewel",
            "t-shirt",
            "shirt",
            "dress",
            "jeans"
        }.Any(word => title.Contains(word, StringComparison.OrdinalIgnoreCase)))
            return false;
        return p.Details.Category switch
        {
            "phones" => p.Details.Kind == "phone",
            "computers" => p.Details.Kind is "laptop" or "desktop",
            "accessories" => p.Details.Kind is "mouse" or "keyboard" or "headphones",
            _ => false
        };
    }
}
