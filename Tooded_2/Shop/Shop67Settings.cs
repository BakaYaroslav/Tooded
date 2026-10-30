using Microsoft.EntityFrameworkCore;

namespace Tooded_2.Shop;

public static class Shop67Settings
{
    public static ShopConfiguration Load(AppDbContext db)
    {
        var row = db.ShopSettings.SingleOrDefault(s => s.Id == "shoping67");
        return row == null ? new() : ShopConfiguration.Parse(row.Value);
    }

    public static void Save(AppDbContext db, ShopConfiguration config)
    {
        var row = db.ShopSettings.SingleOrDefault(s => s.Id == "shoping67");
        if (row == null) { row = new(); db.ShopSettings.Add(row); }
        row.Value = ShopConfiguration.Serialize(config);
        db.SaveChanges();
    }

    public static ProductDetails Details(ShopConfiguration config, Toode product) => config.Products.GetValueOrDefault(product.Id) ?? new ProductDetails
    {
        Name = new(product.Toodenimetus, product.Toodenimetus, product.Toodenimetus),
        // Legacy products stay in the admin list until assigned to a supported category.
        Visible = false
    };
}
