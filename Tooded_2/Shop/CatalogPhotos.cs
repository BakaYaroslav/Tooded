using System.Text.Json;
using System.Drawing.Imaging;

namespace Tooded_2.Shop;

internal static class CatalogPhotos
{
    public static async Task Bundle()
    {
        var catalog = TechCatalog.Reference();
        string repository = Path.Combine(Directory.GetCurrentDirectory(), "Tooded_2", "Images");
        var errors = new List<string>();
        foreach (var product in catalog)
        {
            try
            {
            string path = Path.Combine(repository, product.Image!);
            string output = Path.Combine(AppContext.BaseDirectory, "Images", product.Image!);
            using var existing = ShopImages.Load(path);
            if (existing == null)
            {
                using var photo = await ShopImages.Download(product.Details.PhotoUrl);
                if (photo == null) throw new InvalidOperationException("Unable to download the product photo: " + product.Name + " " + product.Details.PhotoUrl);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                photo.Save(path, ImageFormat.Png);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(output)!); File.Copy(path, output, true);
            Console.WriteLine("Verified product image: " + product.Name);
            }
            catch (Exception ex) { errors.Add(product.Name + ": " + ex.Message); }
        }
        Directory.CreateDirectory(Path.Combine(repository, "Products"));
        await File.WriteAllTextAsync(Path.Combine(repository, "Products", "download-report.json"), JsonSerializer.Serialize(errors));
        await File.WriteAllTextAsync(Path.Combine(repository, "Products", "SOURCES.md"),
            "# Product photographs\n\nProduct names and photograph/source URLs are versioned in Catalog/tech-products.json.\n\n" +
            string.Join("\n\n", catalog.Select(p => "## " + p.Name + "\n\nPhoto: " + p.Details.PhotoUrl + "\n\nProduct/source: " + p.Details.SourceUrl)) + "\n");
        if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
    }
}
