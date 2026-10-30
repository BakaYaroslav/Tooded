using System.Drawing.Imaging;

namespace Tooded_2.Shop;

internal static class CatalogPhotos
{
    public static async Task Bundle()
    {
        var catalog = TechCatalog.Reference();
        string repository = Path.Combine(Directory.GetCurrentDirectory(), "Tooded_2", "Images");
        foreach (var product in catalog)
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
    }
}
