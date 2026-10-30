using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tooded_2.Shop;

internal static class ShopUiChecks
{
    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        { yield return child; foreach (var nested in Descendants(child)) yield return nested; }
    }
    private static void Require(bool value, string message)
    { if (!value) throw new InvalidOperationException("UI check: " + message); }
    private static void PumpUntil(Func<bool> predicate)
    {
        var watch = Stopwatch.StartNew();
        while (!predicate() && watch.Elapsed < TimeSpan.FromSeconds(10))
        { Application.DoEvents(); Thread.Sleep(10); }
        Require(predicate(), "Timed out waiting for the storefront");
    }
    public static void Run()
    {
        T.SetLanguage("et");
        using (var db = new AppDbContext())
        {
            Require(!db.Database.HasPendingModelChanges(), "EF snapshot differs from the runtime model");
            string sql = db.GetService<IMigrator>().GenerateScript();
            Require(sql.Contains("ShopSettings"), "Migration script does not contain ShopSettings");
        }
        Directory.CreateDirectory("ui-checks");
        var products = TechCatalog.Reference().ToArray();
        using var store = new DemoShopStore(products, TechCatalog.InitialPromotions());
        using var form = new PoodForm(store); form.Show();
        PumpUntil(() => Descendants(form).OfType<ProductListItem>().Count() == 6);
        Require(Descendants(form).OfType<PictureBox>().All(p => p.Image != null), "A catalog product is missing its local photograph");
        using (var bitmap = new Bitmap(form.Width, form.Height))
        { form.DrawToBitmap(bitmap, form.ClientRectangle); bitmap.Save("ui-checks/home-et.png"); }
        Descendants(form).OfType<Button>().Single(b => b.Text == T.Category("computers")).PerformClick();
        var gpu = Descendants(form).OfType<CheckedListBox>().Single(list => list.Items.Cast<object>().Any(i => i.ToString() == "RTX 4070 Laptop GPU"));
        gpu.SetItemChecked(Enumerable.Range(0, gpu.Items.Count).Single(i => gpu.Items[i].ToString() == "RTX 4070 Laptop GPU"), true);
        PumpUntil(() => Descendants(form).OfType<ProductListItem>().Count() == 1);
        Require(!Descendants(form).OfType<ComboBox>().Any(c => c.AccessibleName == "ET / RU / EN"), "Language selector is still visible");
        Require(Descendants(form).OfType<Label>().Any(l => l.Text.Contains("Videokaart")), "Estonian filter label missing");
        using (var bitmap = new Bitmap(form.Width, form.Height))
        { form.DrawToBitmap(bitmap, form.ClientRectangle); bitmap.Save("ui-checks/rtx4070-et.png"); }
        var cart = new ShoppingCart(); var selected = store.GetProducts().First(p => p.Details.SourceKey == "model:asus-rog-strix-g16-g614ji-as94"); cart.Add(selected, 1);
        using (var details = new ProductDetailsForm(selected, cart, (_, _) => true)) { details.Show(); Application.DoEvents(); details.Close(); }
        using (var basket = new CartForm(cart, store)) { basket.Show(); Application.DoEvents(); basket.Close(); }
        var order = store.Checkout(cart.Lines);
        using (var confirmation = new OrderConfirmationForm(order)) { confirmation.Show(); Application.DoEvents(); confirmation.Close(); }
        using (var login = new LoginForm())
        {
            login.Show(); Application.DoEvents();
            Require(!Descendants(login).OfType<ComboBox>().Any(c => c.AccessibleName == "ET / RU / EN"), "Login still has a language selector");
            login.Close();
        }
        using (var registration = new RegistreeriForm()) { registration.Show(); Application.DoEvents(); registration.Close(); }
        Require(cart.Count == 1, "Closing dialogs changed the basket");
        var config = new ShopConfiguration { Products = products.ToDictionary(p => p.Id, p => p.Details) };
        var oldPhone = products.Single(p => p.Details.SourceKey == "dummyjson:123");
        var badOffer = new Promotion { Title = new("Eestikeelne kampaania"), Category = "accessories", ProductKeys = oldPhone.Details.SourceKey, Percent = 15 };
        var rows = products.Select(p => new Toode { Id = p.Id, Toodenimetus = p.Name }).ToArray();
        using (var editor = new PromotionEditorForm(badOffer, rows, config))
        {
            editor.Show(); Application.DoEvents();
            Require(Descendants(editor).OfType<TextBox>().Count(t => t.Parent is FlowLayoutPanel) == 3, "Promotion editor still asks for translation fields");
            Require(!Descendants(editor).OfType<Label>().Any(l => l.Text.EndsWith(" · RU") || l.Text.EndsWith(" · EN")), "Foreign-language editor labels remain");
            var list = Descendants(editor).OfType<CheckedListBox>().Single(c => c.AccessibleName == "promotion-products");
            Require(!list.Items.Cast<object>().Any(p => p.ToString()!.Contains("iPhone")), "Accessories target list contains a phone");
            Require(list.CheckedItems.Count == 0, "Incompatible selection was retained");
            var category = Descendants(editor).OfType<ComboBox>().Single(c => c.AccessibleName == "promotion-category");
            category.SelectedIndex = 1; Application.DoEvents();
            int index = Enumerable.Range(0, list.Items.Count).Single(i => list.Items[i].ToString() == oldPhone.DisplayName);
            list.SetItemChecked(index, true); Application.DoEvents();
            typeof(PromotionEditorForm).GetMethod("Save", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(editor, null);
            Require(editor.Value.Category == "phones" && editor.Value.ProductKeys == oldPhone.Details.SourceKey,
                "Corrected promotion did not save its selected phone");
            editor.Close();
        }
        var paymentProduct = store.GetProducts().First(p => p.Stock > 2);
        var paymentCart = new ShoppingCart(); paymentCart.Add(paymentProduct, 1); int sent = 0;
        using (var basket = new CartForm(paymentCart, store, _ => { sent++; return Task.CompletedTask; }))
        {
            basket.Show(); Application.DoEvents();
            var email = Descendants(basket).OfType<TextBox>().Single(t => t.AccessibleName == "order-email"); email.Text = "test@example.com";
            Descendants(basket).OfType<Button>().Single(b => b.AccessibleName == "pay-order").PerformClick();
            PumpUntil(() => basket.Order != null);
            Require(sent == 1 && paymentCart.Count == 0 && basket.PaymentFlow.Result?.EmailSent == true, "One-click email checkout failed");
            using var success = new OrderConfirmationForm(basket.Order!, basket.PaymentFlow); success.Show(); Application.DoEvents();
            Require(Descendants(success).OfType<Label>().Any(l => l.Text.Contains("umbes kahe nädala")), "Two-week delivery estimate is missing");
            Require(Descendants(success).OfType<Label>().Any(l => l.Text.Contains("test@example.com")), "Email delivery status is missing");
            using var image = new Bitmap(success.Width, success.Height); success.DrawToBitmap(image, success.ClientRectangle); image.Save("ui-checks/payment-success-et.png");
            success.Close();
        }
        using (var mailSettings = new MailSettingsForm())
        {
            mailSettings.Show(); Application.DoEvents();
            Require(Descendants(mailSettings).OfType<TextBox>().Count(t => t.Parent is FlowLayoutPanel) == 2,
                "Mail settings should ask only for sender and app password");
            Require(!Descendants(mailSettings).OfType<NumericUpDown>().Any() && !Descendants(mailSettings).OfType<CheckBox>().Any(),
                "Technical port or TLS controls are still shown");
            Require(!Descendants(mailSettings).OfType<Label>().Any(l => l.Text.Contains("SMTP") || l.Text == "Port"),
                "Mail settings still show connection details");
            mailSettings.Close();
        }
        string temporarySettings = Path.Combine(Path.GetTempPath(), "shoping67-mail-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            const string dummy = "test-credential-not-a-real-password";
            new MailSettings { SenderEmail = "test@example.com", Password = dummy }.Save(temporarySettings);
            Require(!File.ReadAllText(temporarySettings).Contains(dummy), "Mail password was stored as plaintext");
            Require(MailSettings.Load(temporarySettings).Password == dummy, "Windows-protected mail settings cannot be read");
        }
        finally { if (File.Exists(temporarySettings)) File.Delete(temporarySettings); }
        form.Close(); T.SetLanguage("et");
        Console.WriteLine("PASS Windows form rendering, GPU filter, Estonian-only forms and EF migration model");
    }
}
