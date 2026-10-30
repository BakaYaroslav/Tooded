using System.Text.Json;

namespace Tooded_2.Shop;

public static class ShopCatalog
{
    public static IReadOnlyList<ShopProduct> Load() =>
        JsonSerializer.Deserialize<List<ShopProduct>>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Catalog", "products.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidDataException("Tootekataloog on tühi või vigane.");
}
