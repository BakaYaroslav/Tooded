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

    public static bool DeleteProduct(int productId)
    {
        using var db = new AppDbContext();
        return DeleteProduct(db, productId);
    }

    public static bool DeleteProduct(AppDbContext db, int productId)
    {
        using var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var configuration = Load(db);
        var product = db.Toodetabel.SingleOrDefault(p => p.Id == productId);
        if (product == null) return false;
        ProductRemoval.Apply(configuration, productId);
        db.Toodetabel.Remove(product);
        // SaveChanges commits the product removal and its metadata together; sales rows are untouched.
        Save(db, configuration);
        transaction.Commit();
        return true;
    }

    public static ProductDetails Details(ShopConfiguration config, Toode product) => config.Products.GetValueOrDefault(product.Id) ?? new ProductDetails
    {
        Name = new(product.Toodenimetus, product.Toodenimetus, product.Toodenimetus),
        // Legacy products stay in the admin list until assigned to a supported category.
        Visible = false
    };
}
