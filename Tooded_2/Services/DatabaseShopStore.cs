using Microsoft.EntityFrameworkCore;

namespace Tooded_2;
public sealed class DatabaseShopStore : IShopStore
{
    private ShopConfiguration configuration = new();
    private bool imported;
    public bool IsDemoCatalog => false;

    private string? catalogStatusKey;
    public string CatalogStatus => catalogStatusKey == null ? "shoping67" : UiText.Get(catalogStatusKey);

    public IReadOnlyList<ShopProduct> GetProducts()
    {
        // The storefront runs this method on its catalog worker.
        if (!imported)
        {
            var catalog = TechCatalog.LoadAsync().GetAwaiter().GetResult();
            using var db = new AppDbContext();
            using var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            configuration = ShopSettingsStore.Load(db);
            if (!configuration.Initialized)
            {
                configuration.Promotions = TechCatalog.InitialPromotions();
                configuration.Initialized = true;
            }

            var storedRows = db.Toodetabel.ToList();
            CatalogRepair.Reconcile(configuration, storedRows.ToDictionary(p => p.Id, p => p.Toodenimetus), catalog.Products);
            var approved = catalog.Products.ToDictionary(p => p.Details.SourceKey);
            foreach (var row in storedRows)
                if (configuration.Products.TryGetValue(row.Id, out var detail) && detail.Visible && approved.TryGetValue(detail.SourceKey, out var model))
                    row.Pilt = model.Image;
            var categories = db.Kategooriatabel.ToList();
            foreach (var product in catalog.Products)
            {
                if (configuration.ImportedKeys.ContainsKey(product.Details.SourceKey))
                    continue;
                // Stable source keys prevent duplicate imports and do not overwrite admin edits or sold stock.
                var category = categories.FirstOrDefault(c => c.nimetus == product.Details.Category);
                if (category == null)
                {
                    category = new Kategooria
                    {
                        nimetus = product.Details.Category
                    };
                    categories.Add(category);
                    db.Kategooriatabel.Add(category);
                }

                var row = new Toode
                {
                    Toodenimetus = product.Name,
                    Hind = (float)product.Price,
                    Kogus = product.Stock,
                    Pilt = product.Image,
                    Kategooria = category
                };
                db.Toodetabel.Add(row);
                db.SaveChanges();
                configuration.ImportedKeys[product.Details.SourceKey] = row.Id;
                configuration.Products[row.Id] = product.Details;
            }

            ShopSettingsStore.Save(db, configuration);
            transaction.Commit();
            imported = true;
            catalogStatusKey = catalog.StatusKey;
        }

        using (var db = new AppDbContext())
        {
            configuration = ShopSettingsStore.Load(db);
            return db.Toodetabel.AsNoTracking().Include(t => t.Kategooria).OrderBy(t => t.Id).ToList().Select(Map).Where(p => p.Details.Visible && CatalogRules.IsTechnical(p)).ToArray();
        }
    }

    public IReadOnlyList<Promotion> GetPromotions() => configuration.Promotions;
    private ShopProduct Map(Toode product)
    {
        var details = ShopSettingsStore.Details(configuration, product);
        return PromotionEngine.Apply(new ShopProduct { Id = product.Id, Name = product.Toodenimetus, Category = details.Category, Price = decimal.Round((decimal)product.Hind, 2, MidpointRounding.AwayFromZero), Stock = Math.Max(0, product.Kogus), Image = product.Pilt, Details = details, Specifications = details.Specs }, configuration.Promotions, DateTimeOffset.UtcNow);
    }

    public ShopOrder Checkout(IReadOnlyList<CartLine> lines)
    {
        using var db = new AppDbContext();
        using var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        configuration = ShopSettingsStore.Load(db);
        var ids = lines.Select(l => l.Product.Id).ToArray();
        var current = db.Toodetabel.AsNoTracking().Where(p => ids.Contains(p.Id)).ToList();
        var available = current.Select(Map).Where(p => p.Details.Visible && CatalogRules.IsTechnical(p)).ToArray();
        OrderValidation.Validate(lines, available);
        foreach (var line in lines)
        {
            var original = current.Single(p => p.Id == line.Product.Id);
            int updated = db.Toodetabel.Where(p => p.Id == original.Id && p.Kogus >= line.Quantity && p.Hind == original.Hind).ExecuteUpdate(setters => setters.SetProperty(p => p.Kogus, p => p.Kogus - line.Quantity));
            if (updated != 1)
                throw new InvalidOperationException(UiText.Get("StockOrPriceChanged"));
            db.Muugitabel.Add(new Muuk { Aeg = DateTime.Now, ToodeId = original.Id, Toodenimetus = original.Toodenimetus, Kogus = line.Quantity, Hind = line.Product.Price, Summa = line.Total });
        }

        var order = ShopOrder.Create(lines);
        db.SaveChanges();
        transaction.Commit();
        return order;
    }

    public void Dispose()
    {
    }
}
