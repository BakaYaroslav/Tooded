using System.Net.Http;
using System.Text.Json;

namespace Tooded_2.Shop;

public static class TechCatalog
{
    public const string ReferenceEndpoint = "https://raw.githubusercontent.com/BakaYaroslav/Tooded/refs/heads/codex/shop-cart-demo/Tooded_2/Catalog/tech-products.json";
    private static readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(12), MaxResponseContentBufferSize = 5 * 1024 * 1024 };
    private static readonly JsonSerializerOptions json = new() { PropertyNameCaseInsensitive = true };
    private static readonly string cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "shoping67", "catalog.json");

    public static IReadOnlyList<ShopProduct> Read(string text)
    {
        var products = JsonSerializer.Deserialize<List<ShopProduct>>(text, json) ?? throw new InvalidDataException("Invalid catalog");
        if (products.Count == 0 || products.Count > 1000 || products.Any(p => p == null || !CatalogRules.IsTechnical(p)
            || p.Price < 0 || p.Stock < 0 || string.IsNullOrWhiteSpace(p.Details.SourceKey) || string.IsNullOrWhiteSpace(p.Name))
            || products.Select(p => p.Details.SourceKey).Distinct().Count() != products.Count)
            throw new InvalidDataException("Invalid catalog products");
        return products;
    }

    public static IReadOnlyList<ShopProduct> Reference() => Read(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Catalog", "tech-products.json")));

    public static IReadOnlyList<ShopProduct> ParseDummyJson(string text, string category)
    {
        using var document = JsonDocument.Parse(text);
        var result = new List<ShopProduct>();
        foreach (var row in document.RootElement.GetProperty("products").EnumerateArray())
        {
            if (row.GetProperty("category").GetString() != (category == "phones" ? "smartphones" : "laptops")) continue;
            int id = row.GetProperty("id").GetInt32();
            string title = row.GetProperty("title").GetString() ?? "";
            decimal price = row.GetProperty("price").GetDecimal();
            if (id <= 0 || title.Length == 0 || price < 0) continue;
            string brand = row.TryGetProperty("brand", out var b) ? b.GetString() ?? "" : "";
            string description = row.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
            result.Add(new ShopProduct
            {
                Id = id, Name = title, Price = price, Stock = Math.Max(0, row.GetProperty("stock").GetInt32()),
                Image = row.TryGetProperty("thumbnail", out var image) ? image.GetString() : null,
                Details = new ProductDetails
                {
                    SourceKey = "dummyjson:" + id, SourceUrl = "https://dummyjson.com/products/" + id,
                    Category = category, Kind = category == "computers" ? "laptop" : "phone", Brand = brand,
                    Name = new(title, title, title),
                    Description = new("Veebikataloogist imporditud " + title + ". Tehnilised andmed, mida allikas ei avalda, on märkimata.",
                        "Товар из интернет-каталога: " + title + ". Характеристики, которых нет у источника, не указаны.", description),
                    Specs = new() // DummyJSON does not provide verified CPU/GPU/RAM: never infer them from a title.
                }
            });
        }
        return result;
    }

    private static async Task<IReadOnlyList<ShopProduct>> Download(string url, string? category)
    {
        try
        {
            string text = await client.GetStringAsync(url);
            return category == null ? Read(text) : ParseDummyJson(text, category);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or InvalidDataException or KeyNotFoundException or InvalidOperationException or FormatException)
        { return Array.Empty<ShopProduct>(); }
    }

    public static async Task<CatalogDownload> LoadAsync()
    {
        var batches = await Task.WhenAll(Download(ReferenceEndpoint, null),
            Download("https://dummyjson.com/products/category/smartphones?limit=100", "phones"),
            Download("https://dummyjson.com/products/category/laptops?limit=100", "computers"));
        var cached = Array.Empty<ShopProduct>();
        try { if (File.Exists(cache)) cached = Read(await File.ReadAllTextAsync(cache)).ToArray(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException) { }
        var products = batches.SelectMany(p => p).Concat(cached).Concat(Reference())
            .DistinctBy(p => p.Details.SourceKey).Select((p, i) => p with { Id = i + 1 }).ToArray();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cache)!);
            string temp = cache + "." + Guid.NewGuid().ToString("N");
            try { await File.WriteAllTextAsync(temp, JsonSerializer.Serialize(products)); File.Move(temp, cache, true); }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        return new(products, batches.Any(b => b.Count > 0)
            ? T.L("Veebikataloog uuendatud. Hinnad ja tellimused on õppeotstarbelised.", "Интернет-каталог обновлён. Цены и заказы учебные.", "Online catalog updated. Prices and orders are for teaching.")
            : T.L("Võrk pole saadaval. Kasutusel salvestatud ja kaasasolev kataloog.", "Сеть недоступна. Используется сохранённый и встроенный каталог.", "Network unavailable. Using cached and bundled products."));
    }

    public static List<Promotion> InitialPromotions() => new()
    {
        new Promotion { Title = new("Töölaud uuele tasemele", "Обнови свой рабочий стол", "Upgrade your desk"),
            Description = new("Valitud arvutitarvikud −10%. Õppepoe avamispakkumine.", "Компьютерные аксессуары −10%. Учебная акция к открытию магазина.", "Computer accessories −10%. Teaching shop opening offer."),
            Category = "accessories", Percent = 10, Position = 0 },
        new Promotion { Title = new("Leia oma järgmine arvuti", "Найди свой следующий компьютер", "Find your next computer"),
            Description = new("Võrdle protsessorit, videokaarti ja mälu.", "Сравни процессоры, видеокарты и память.", "Compare processors, graphics cards and memory."),
            Category = "computers", Percent = 0, Position = 1 }
    };
}
