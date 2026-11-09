using System.Text.Json;

namespace Tooded_2;
public sealed class ShopConfiguration
{
    public Dictionary<int, ProductDetails> Products { get; set; } = new();
    public Dictionary<string, int> ImportedKeys { get; set; } = new();
    public List<Promotion> Promotions { get; set; } = new();
    public bool Initialized { get; set; }

    public static string Serialize(ShopConfiguration value) => JsonSerializer.Serialize(value);
    public static ShopConfiguration Parse(string value) => JsonSerializer.Deserialize<ShopConfiguration>(value) ?? throw new InvalidDataException("Invalid shop settings");
}
