using Microsoft.EntityFrameworkCore;

namespace Tooded_2.Shop;

public sealed class DatabaseShopStore : IShopStore
{
    private IReadOnlyList<ShopProduct> descriptions = ShopCatalog.Load().Concat(new OnlineCatalog().ReadCache()).ToArray();
    public string CatalogStatus { get; private set; } = "Poe kataloog · Demotellimused vähendavad laoseisu ja kajastuvad müügis.";
    public bool IsDemoCatalog => false;

    public IReadOnlyList<ShopProduct> GetProducts()
    {
        using var db = new AppDbContext();
        if (!db.Toodetabel.Any())
        {
            var download = new OnlineCatalog().LoadAsync(allowBundledFallback: false).GetAwaiter().GetResult();
            // Check again inside the transaction if another shop opened during the download.
            using var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            if (!db.Toodetabel.Any())
            {
                var categories = db.Kategooriatabel.ToList();
                foreach (var product in download.Products)
                {
                    var category = categories.FirstOrDefault(c => string.Equals(
                        c.nimetus, product.Category, StringComparison.OrdinalIgnoreCase));
                    if (category == null)
                    {
                        category = new Kategooria { nimetus = product.Category };
                        db.Kategooriatabel.Add(category);
                        categories.Add(category);
                    }
                    db.Toodetabel.Add(new Toode
                    {
                        Toodenimetus = product.Name, Hind = (float)product.Price,
                        Kogus = product.Stock, Pilt = product.Image, Kategooria = category
                    });
                }
                db.SaveChanges();
            }
            transaction.Commit();
            descriptions = download.Products.Concat(ShopCatalog.Load()).ToArray();
            CatalogStatus = download.Status + " Tooted salvestatud andmebaasi.";
        }
        return db.Toodetabel.AsNoTracking().Include(t => t.Kategooria).OrderBy(t => t.Id)
            .ToList().Select(Map).ToArray();
    }

    private ShopProduct Map(Toode product)
    {
        var info = descriptions.FirstOrDefault(p => string.Equals(p.Name, product.Toodenimetus, StringComparison.OrdinalIgnoreCase));
        return new ShopProduct
        {
            Id = product.Id, Name = product.Toodenimetus,
            Category = product.Kategooria?.nimetus ?? "Muud tooted",
            Price = decimal.Round((decimal)product.Hind, 2, MidpointRounding.AwayFromZero), Stock = Math.Max(0, product.Kogus),
            Image = string.IsNullOrWhiteSpace(product.Pilt) ? info?.Image : product.Pilt,
            Description = info?.Description ?? $"{product.Toodenimetus}. Kategooria: {product.Kategooria?.nimetus ?? "Muud tooted"}. " +
                "Vali sobiv kogus ja lisa toode ostukorvi. Ost vormistatakse demomaksega; päris raha ei võeta ega kaupa saadeta.",
            Specifications = info?.Specifications ?? new()
        };
    }

    public DemoOrder Checkout(IReadOnlyList<CartLine> lines)
    {
        using var db = new AppDbContext();
        using var transaction = db.Database.BeginTransaction();
        var ids = lines.Select(l => l.Product.Id).ToArray();
        var current = db.Toodetabel.AsNoTracking().Where(p => ids.Contains(p.Id)).ToList();
        OrderValidation.Validate(lines, current.Select(Map).ToArray());
        foreach (var line in lines)
        {
            var original = current.Single(p => p.Id == line.Product.Id);
            // Conditional update protects against concurrent purchases and price changes.
            int updated = db.Toodetabel.Where(p => p.Id == original.Id && p.Kogus >= line.Quantity && p.Hind == original.Hind)
                .ExecuteUpdate(setters => setters.SetProperty(p => p.Kogus, p => p.Kogus - line.Quantity));
            if (updated != 1) throw new InvalidOperationException("Laoseis või hind muutus ostu ajal. Uuenda kataloogi ja proovi uuesti.");
            db.Muugitabel.Add(new Muuk
            {
                Aeg = DateTime.Now, ToodeId = original.Id, Toodenimetus = original.Toodenimetus,
                Kogus = line.Quantity, Hind = line.Product.Price, Summa = line.Total
            });
        }
        var order = DemoOrder.Create(lines);
        db.SaveChanges();
        transaction.Commit();
        return order;
    }

    public void Dispose() { }
}
