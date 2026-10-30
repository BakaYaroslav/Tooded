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
        var products = TechCatalog.Reference().Select(p => p with { Image = null }).ToArray();
        using var store = new DemoShopStore(products, TechCatalog.InitialPromotions());
        using var form = new PoodForm(store); form.Show();
        PumpUntil(() => Descendants(form).OfType<ProductListItem>().Count() == 6);
        using (var bitmap = new Bitmap(form.Width, form.Height))
        { form.DrawToBitmap(bitmap, form.ClientRectangle); bitmap.Save("ui-checks/home-et.png"); }
        Descendants(form).OfType<Button>().Single(b => b.Text == T.Category("computers")).PerformClick();
        var gpu = Descendants(form).OfType<CheckedListBox>().Single(list => list.Items.Cast<object>().Any(i => i.ToString() == "RTX 4070"));
        gpu.SetItemChecked(Enumerable.Range(0, gpu.Items.Count).Single(i => gpu.Items[i].ToString() == "RTX 4070"), true);
        PumpUntil(() => Descendants(form).OfType<ProductListItem>().Count() == 1);
        var picker = Descendants(form).OfType<ComboBox>().Single(c => c.AccessibleName == "ET / RU / EN"); picker.SelectedIndex = 1;
        Application.DoEvents();
        Require(Descendants(form).OfType<ProductListItem>().Count() == 1, "Language change lost the GPU filter");
        Require(Descendants(form).OfType<Label>().Any(l => l.Text.Contains("Видеокарта")), "Russian filter label missing");
        using (var bitmap = new Bitmap(form.Width, form.Height))
        { form.DrawToBitmap(bitmap, form.ClientRectangle); bitmap.Save("ui-checks/rtx4070-ru.png"); }
        var cart = new ShoppingCart(); var selected = store.GetProducts().First(p => p.Details.SourceKey == "ref:pc-4070"); cart.Add(selected, 1);
        using (var details = new ProductDetailsForm(selected, cart, (_, _) => true)) { details.Show(); Application.DoEvents(); details.Close(); }
        using (var basket = new CartForm(cart, store)) { basket.Show(); Application.DoEvents(); basket.Close(); }
        using (var checkout = new DemoCheckoutForm(cart.Lines, store)) { checkout.Show(); Application.DoEvents(); checkout.Close(); }
        var order = store.Checkout(cart.Lines);
        using (var confirmation = new OrderConfirmationForm(order)) { confirmation.Show(); Application.DoEvents(); confirmation.Close(); }
        using (var login = new LoginForm()) { login.Show(); Application.DoEvents(); login.Close(); }
        using (var registration = new RegistreeriForm()) { registration.Show(); Application.DoEvents(); registration.Close(); }
        Require(cart.Count == 1, "Closing dialogs changed the basket");
        form.Close(); T.SetLanguage("et");
        Console.WriteLine("PASS Windows form rendering, GPU filter, language preservation and EF migration model");
    }
}
