using Microsoft.EntityFrameworkCore;

namespace Tooded_2.Shop;

public sealed class DatabaseShopStore : IShopStore
{
    private readonly IReadOnlyList<ShopProduct> descriptions = ShopCatalog.Load();
    public bool IsDemoCatalog => false;

    public IReadOnlyList<ShopProduct> GetProducts()
    {
        using var db = new AppDbContext();
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
