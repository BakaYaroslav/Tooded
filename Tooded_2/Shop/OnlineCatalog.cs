using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tooded_2.Shop;

public sealed record CatalogDownload(IReadOnlyList<ShopProduct> Products, string Status);

/// <summary>Public teaching catalog. Prices and stock are for the demo shop only.</summary>
public sealed class OnlineCatalog
{
    public const string Endpoint = "https://fakestoreapi.com/products";
    private static readonly HttpClient sharedClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12),
        MaxResponseContentBufferSize = 5 * 1024 * 1024
    };
    private static readonly JsonSerializerOptions json = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient client;
    private readonly string directory;
    public static string DefaultDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tooded", "OnlineCatalog");

    public OnlineCatalog(HttpClient? client = null, string? directory = null)
    {
        this.client = client ?? sharedClient;
        this.directory = directory ?? DefaultDirectory;
    }

    public IReadOnlyList<ShopProduct> ReadCache()
    {
        try
        {
            return JsonSerializer.Deserialize<List<ShopProduct>>(
                File.ReadAllText(Path.Combine(directory, "products.json")), json) ?? new();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return Array.Empty<ShopProduct>();
        }
    }

    public static IReadOnlyList<ShopProduct> Parse(string content)
    {
        var rows = JsonSerializer.Deserialize<List<ApiProduct>>(content, json);
        if (rows == null || rows.Count == 0 || rows.Count > 500
            || rows.Any(p => p == null || p.Id <= 0 || string.IsNullOrWhiteSpace(p.Title)
                || p.Price < 0 || p.Price > 1000000)
            || rows.Select(p => p.Id).Distinct().Count() != rows.Count)
            throw new InvalidDataException("Veebikataloog on tühi või vigane.");

        return rows.Select(p => new ShopProduct
        {
            Id = p.Id, Name = p.Title!, Category = CategoryName(p.Category),
            Price = decimal.Round(p.Price, 2, MidpointRounding.AwayFromZero),
            Stock = 20, Image = p.Image, Description = p.Description ?? "",
            Specifications = new()
            {
                ["Allikas"] = "Fake Store API",
                ["Veebikataloogi ID"] = p.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["Laoseis"] = "Õppepoe näidislaoseis, mitte tarnija laoseis",
                ["Hind"] = "Näidishind eurodes; API hinnanumber ei ole valuutavahetus"
            }
        }).ToArray();
    }

    public async Task<CatalogDownload> LoadAsync(bool allowBundledFallback = true)
    {
        try
        {
            var products = Parse(await client.GetStringAsync(Endpoint));
            // Pictures are downloaded off the UI thread with bounded concurrency.
            using var slots = new SemaphoreSlim(4);
            var downloaded = await Task.WhenAll(products.Select(async product =>
            {
                await slots.WaitAsync();
                try { return product with { Image = await DownloadImageAsync(product.Image) }; }
                finally { slots.Release(); }
            }));
            try
            {
                Directory.CreateDirectory(directory);
                string temporary = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".tmp");
                try
                {
                    await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(downloaded));
                    File.Move(temporary, Path.Combine(directory, "products.json"), true);
                }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            return new(downloaded, "Tooted laaditud internetist · Fake Store API · Õppehinnad ja näidislaoseis.");
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException
            or JsonException or InvalidDataException)
        {
            var cached = ReadCache();
            if (cached.Count > 0)
                return new(cached, "Internet pole saadaval · Kasutusel varem alla laaditud kataloog.");
            if (allowBundledFallback)
                return new(ShopCatalog.Load(), "Internet pole saadaval · Kasutusel kuus kaasasolevat näidistoodet.");
            throw new InvalidOperationException(
                "Veebitooteid ei saanud laadida. Kontrolli internetiühendust ja vajuta „Uuenda”.", ex);
        }
    }

    private async Task<string?> DownloadImageAsync(string? address)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps || uri.Host != "fakestoreapi.com") return null;
        string name = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(address!))) + ".jpg";
        string path = Path.Combine(directory, "Images", name);
        if (File.Exists(path)) return path;
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(6));
            byte[] bytes = await client.GetByteArrayAsync(uri, timeout.Token);
            if (bytes.Length == 0 || bytes.Length > 5 * 1024 * 1024) return null;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { await File.WriteAllBytesAsync(temporary, bytes); File.Move(temporary, path, true); }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return path;
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException
            or IOException or UnauthorizedAccessException) { return null; }
    }

    private static string CategoryName(string? category) => category switch
    {
        "electronics" => "Elektroonika", "jewelery" => "Ehted",
        "men's clothing" => "Meesteriided", "women's clothing" => "Naisteriided",
        _ => string.IsNullOrWhiteSpace(category) ? "Muud tooted" : category
    };

    private sealed class ApiProduct
    {
        public ApiProduct() { }
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
    }
}
