using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tooded_2;
using Tooded_2.Shop;

void Require(bool condition, string message)
{ if (!condition) throw new Exception(message); }

using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
int deletedId, retainedId;
const string sourceKey = "model:deletion-test";
using (var db = new StorageContext(connection))
{
    db.Database.EnsureCreated();
    var category = new Kategooria { nimetus = "computers" };
    var product = new Toode { Toodenimetus = "Test laptop", Hind = 100, Kogus = 5, Kategooria = category };
    var retained = new Toode { Toodenimetus = "Other laptop", Hind = 200, Kogus = 3, Kategooria = category };
    db.Toodetabel.AddRange(product, retained); db.SaveChanges();
    deletedId = product.Id; retainedId = retained.Id;
    db.Muugitabel.Add(new Muuk { ToodeId = deletedId, Toodenimetus = product.Toodenimetus,
        Aeg = DateTime.Now, Kogus = 1, Hind = 100, Summa = 100 });
    var config = new ShopConfiguration { Initialized = true,
        Products = new() { [deletedId] = new ProductDetails { SourceKey = sourceKey, Category = "computers", Kind = "laptop", Brand = "Apple" } },
        ImportedKeys = new() { [sourceKey] = deletedId },
        Promotions = new() { new Promotion { Id = "targeted", ProductKeys = sourceKey, Percent = 10 }, new Promotion { Id = "category", Category = "computers", Percent = 5 } } };
    Shop67Settings.Save(db, config);
}
using (var db = new StorageContext(connection))
    Require(Shop67Settings.DeleteProduct(db, deletedId), "Delete did not find the product");
using (var db = new StorageContext(connection))
{
    Require(!db.Toodetabel.Any(p => p.Id == deletedId), "Product was not removed from the relational database");
    Require(db.Toodetabel.Any(p => p.Id == retainedId), "An unrelated product was removed");
    Require(db.Muugitabel.Single().ToodeId == deletedId, "Historical sale was deleted or changed");
    var config = Shop67Settings.Load(db);
    Require(!config.Products.ContainsKey(deletedId), "Product metadata survived deletion");
    Require(config.ImportedKeys.GetValueOrDefault(sourceKey) == deletedId, "Deleted import identity was lost");
    Require(!config.Promotions.Single(p => p.Id == "targeted").Enabled, "Empty targeted discount remained enabled");
    Require(config.Promotions.Single(p => p.Id == "category").Enabled, "Category-wide promotion was changed");
    Require(!Shop67Settings.DeleteProduct(db, deletedId), "A repeated delete should be a no-op");
}
Console.WriteLine("PASS relational deletion, import suppression, promotion cleanup and retained sales history");

using (var db = new StorageContext(connection))
{
    var config = Shop67Settings.Load(db);
    config.Products[retainedId] = new ProductDetails { SourceKey = "model:retained", Category = "computers", Kind = "laptop", Brand = "Dell" };
    config.ImportedKeys["model:retained"] = retainedId;
    Shop67Settings.Save(db, config);
    db.Database.ExecuteSqlRaw("CREATE TRIGGER RejectSettingsUpdate BEFORE UPDATE ON ShopSettings BEGIN SELECT RAISE(ABORT, 'simulated settings write failure'); END;");
}
bool failed = false;
try
{
    using var db = new StorageContext(connection);
    Shop67Settings.DeleteProduct(db, retainedId);
}
catch (DbUpdateException) { failed = true; }
using (var db = new StorageContext(connection))
{
    Require(failed, "Expected the simulated settings write to fail");
    Require(db.Toodetabel.Any(p => p.Id == retainedId), "Failed delete did not roll back the product row");
    Require(db.Muugitabel.Count() == 1, "Rollback changed sales history");
    Require(Shop67Settings.Load(db).Products.ContainsKey(retainedId), "Failed delete did not roll back product metadata");
}
Console.WriteLine("PASS failed metadata write rolls back product deletion atomically");

sealed class StorageContext(SqliteConnection connection) : AppDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connection);
}
