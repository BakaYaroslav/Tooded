using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tooded_2;
using Tooded_2;

System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("et-EE");

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
    ShopSettingsStore.Save(db, config);
}
using (var db = new StorageContext(connection))
    Require(ShopSettingsStore.DeleteProduct(db, deletedId), "Delete did not find the product");
using (var db = new StorageContext(connection))
{
    Require(!db.Toodetabel.Any(p => p.Id == deletedId), "Product was not removed from the relational database");
    Require(db.Toodetabel.Any(p => p.Id == retainedId), "An unrelated product was removed");
    Require(db.Muugitabel.Single().ToodeId == deletedId, "Historical sale was deleted or changed");
    var config = ShopSettingsStore.Load(db);
    Require(!config.Products.ContainsKey(deletedId), "Product metadata survived deletion");
    Require(config.ImportedKeys.GetValueOrDefault(sourceKey) == deletedId, "Deleted import identity was lost");
    Require(!config.Promotions.Single(p => p.Id == "targeted").Enabled, "Empty targeted discount remained enabled");
    Require(config.Promotions.Single(p => p.Id == "category").Enabled, "Category-wide promotion was changed");
    Require(!ShopSettingsStore.DeleteProduct(db, deletedId), "A repeated delete should be a no-op");
}
Console.WriteLine("PASS relational deletion, import suppression, promotion cleanup and retained sales history");

using (var db = new StorageContext(connection))
{
    var config = ShopSettingsStore.Load(db);
    config.Products[retainedId] = new ProductDetails { SourceKey = "model:retained", Category = "computers", Kind = "laptop", Brand = "Dell" };
    config.ImportedKeys["model:retained"] = retainedId;
    ShopSettingsStore.Save(db, config);
    db.Database.ExecuteSqlRaw("CREATE TRIGGER RejectSettingsUpdate BEFORE UPDATE ON ShopSettings BEGIN SELECT RAISE(ABORT, 'simulated settings write failure'); END;");
}
bool failed = false;
try
{
    using var db = new StorageContext(connection);
    ShopSettingsStore.DeleteProduct(db, retainedId);
}
catch (DbUpdateException) { failed = true; }
using (var db = new StorageContext(connection))
{
    Require(failed, "Expected the simulated settings write to fail");
    Require(db.Toodetabel.Any(p => p.Id == retainedId), "Failed delete did not roll back the product row");
    Require(db.Muugitabel.Count() == 1, "Rollback changed sales history");
    Require(ShopSettingsStore.Load(db).Products.ContainsKey(retainedId), "Failed delete did not roll back product metadata");
}
Console.WriteLine("PASS failed metadata write rolls back product deletion atomically");


var receiptProduct = new ShopProduct { Id = 1, Name = "Kõrvaklapid Õ Ä Ö Ü", Price = 879.99m, Stock = 3 };
var receiptOrder = new ShopOrder("DEMO-RECEIPT", new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero),
    new[] { new CartLine(receiptProduct, 1) }) { CustomerEmail = "buyer@example.com" };
byte[] receipt = OrderPdf.Create(receiptOrder);
Require(receipt.Length > 1000 && System.Text.Encoding.ASCII.GetString(receipt, 0, 5) == "%PDF-", "Receipt is not a PDF file");
using (var parsed = UglyToad.PdfPig.PdfDocument.Open(receipt))
{
    Require(parsed.NumberOfPages == 1, "A one-item receipt should fit on one page");
    string text = parsed.GetPage(1).Text;
    Require(text.Contains("DEMO-RECEIPT") && text.Contains("MAKSTUD"), "Paid status or order reference missing from PDF");
    Require(text.Contains("Kõrvaklapid") && new[] { "Õ", "Ä", "Ö", "Ü" }.All(text.Contains), "Estonian Unicode text did not survive PDF generation");
    Require(text.Contains("879,99") && text.Contains("€"), "Paid amount or euro sign missing from PDF");
    Require(text.Contains("buyer@example.com") && text.Contains("13.11.2026"), "Customer or delivery date missing from PDF");
}
Directory.CreateDirectory("ui-checks"); File.WriteAllBytes("ui-checks/payment-receipt.pdf", receipt);
var manyLines = Enumerable.Range(1, 150).Select(i => new CartLine(receiptProduct with { Id = i,
    Name = "Toode " + i + " väga pikk tootenimetus " + new string('A', 180) }, 1)).ToArray();
var largeOrder = receiptOrder with { Lines = manyLines };
using (var parsed = UglyToad.PdfPig.PdfDocument.Open(OrderPdf.Create(largeOrder)))
{
    Require(parsed.NumberOfPages > 1, "A large receipt did not paginate");
    string text = string.Join(" ", parsed.GetPages().Select(page => page.Text));
    Require(text.Contains("Toode 1") && text.Contains("Toode 150") && text.Contains("KOKKU TASUTUD"), "Large PDF lost line items or its total");
}
Console.WriteLine("PASS PDF receipt structure, paid status, actual amount, Unicode, delivery date and pagination");

using (var message = OrderMailMessage.Create(receiptOrder, new MailSettings { SenderEmail = "sender@example.com" }))
{
    Require(message.Attachments.Count == 1, "Confirmation has no PDF attachment");
    var attachment = message.Attachments[0];
    Require(attachment.ContentType.MediaType == "application/pdf" && attachment.Name == OrderPdf.FileName(receiptOrder), "Invalid PDF MIME type or filename");
    using var data = new MemoryStream(); attachment.ContentStream.CopyTo(data); attachment.ContentStream.Position = 0;
    using var parsed = UglyToad.PdfPig.PdfDocument.Open(data.ToArray());
    Require(parsed.GetPage(1).Text.Contains("879,99"), "Attached PDF has an incorrect paid amount");
    string pickup = Path.Combine(Path.GetTempPath(), "shoping67-mail-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(pickup);
    try
    {
        using var smtp = new System.Net.Mail.SmtpClient { DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = pickup, UseDefaultCredentials = false };
        smtp.Send(message); // Local MIME serialization only: never connects to a mail server.
        string mime = File.ReadAllText(Directory.GetFiles(pickup).Single());
        Require(mime.Contains("application/pdf", StringComparison.OrdinalIgnoreCase) && mime.Contains("JVBERi0"), "Serialized email lost its PDF attachment");
    }
    finally { Directory.Delete(pickup, true); }
}
var englishOrder = receiptOrder with { CultureName = "en-US" };
using (var parsed = UglyToad.PdfPig.PdfDocument.Open(OrderPdf.Create(englishOrder)))
{
    string text = parsed.GetPage(1).Text;
    Require(text.Contains("PAID") && text.Contains("879.99") && text.Contains("Estimated delivery"), "English PDF did not use English resources/culture");
}
Require(OrderEmail.Html(englishOrder).Contains("Payment successful!") && OrderEmail.Html(englishOrder).Contains("879.99"), "English email did not use resource strings and numbers");
Require(System.Globalization.CultureInfo.CurrentUICulture.Name != "en-US", "Document culture leaked into the interface");
Console.WriteLine("PASS English PDF and email resources, amounts and isolated document culture");

Console.WriteLine("PASS confirmation email contains an actual readable PDF attachment without sending live email");

var russianOrder = receiptOrder with { CultureName = "ru-RU" };
using (var parsed = UglyToad.PdfPig.PdfDocument.Open(OrderPdf.Create(russianOrder)))
{
    string text = parsed.GetPage(1).Text;
    Require(text.Contains("ОПЛАЧЕНО") && text.Contains("879,99") && text.Contains("Ожидаемая доставка"), "Russian PDF lost Cyrillic text, paid amount or delivery date");
}
Require(OrderEmail.Html(russianOrder).Contains("Оплата прошла успешно!"), "Russian email resources are missing");
Console.WriteLine("PASS Russian email and PDF receipt with readable Cyrillic text");

using var productConnection = new SqliteConnection("Data Source=:memory:");
productConnection.Open();
using (var db = new StorageContext(productConnection))
{
    db.Database.EnsureCreated();
    var details = new ProductDetails { Name = new("Test mouse"), Category = "accessories", Kind = "mouse", Brand = "Logitech", Visible = true };
    int id = ProductService.Save(db, null, details, 29.99m, 4, "Products/logitech-g305.png");
    Require(id > 0 && db.Toodetabel.Single().Kogus == 4, "Unified admin cannot add a product");
    string source = ShopSettingsStore.Load(db).Products[id].SourceKey;
    Require(source == "local:" + id, "New product metadata has no stable identity");
    ProductService.Save(db, id, details with { SourceKey = source, Specs = new() { ["connection"] = "wireless" } }, 19.99m, 6, "Products/logitech-g305.png");
    Require(db.Toodetabel.Count() == 1 && db.Toodetabel.Single().Kogus == 6, "Editing a product inserted a duplicate or lost stock");
    Require(ShopSettingsStore.Load(db).Products[id].Specs["connection"] == "wireless", "Product specifications were not saved");
    bool rejected = false;
    try { ProductService.Save(db, null, details, -1, 1, "image.png"); } catch (InvalidOperationException) { rejected = true; }
    Require(rejected && db.Toodetabel.Count() == 1, "Invalid product was not rejected before saving");
}
using (var db = new StorageContext(productConnection))
{
    db.Database.ExecuteSqlRaw("CREATE TRIGGER RejectProductMetadata BEFORE UPDATE ON ShopSettings BEGIN SELECT RAISE(ABORT, 'simulated write failure'); END;");
    bool rolledBack = false;
    try
    {
        ProductService.Save(db, null, new ProductDetails { Name = new("Another mouse"), Category = "accessories", Kind = "mouse", Brand = "Logitech" }, 9.99m, 2, "image.png");
    }
    catch (DbUpdateException) { rolledBack = true; }
    Require(rolledBack, "Expected metadata write failure was not triggered");
}
using (var db = new StorageContext(productConnection))
{
    Require(db.Toodetabel.Count() == 1 && ShopSettingsStore.Load(db).Products.Count == 1,
        "Failed metadata save left a partially created product");
}
Console.WriteLine("PASS unified administrator adds and edits products with stable metadata and validation");

sealed class StorageContext(SqliteConnection connection) : AppDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connection);
}
