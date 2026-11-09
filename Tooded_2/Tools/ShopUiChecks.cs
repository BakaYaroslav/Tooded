using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tooded_2;
internal static class ShopUiChecks
{
    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var nested in Descendants(child))
                yield return nested;
        }
    }

    private static void Require(bool value, string message)
    {
        if (!value)
            throw new InvalidOperationException("UI check: " + message);
    }

    private static void PumpUntil(Func<bool> predicate)
    {
        var watch = Stopwatch.StartNew();
        while (!predicate() && watch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Application.DoEvents();
            Thread.Sleep(10);
        }

        Require(predicate(), "Timed out waiting for the storefront");
    }

    public static void Run()
    {
        string saved = Properties.Settings.Default.UserLanguage;
        try
        {
            RunChecks();
        }
        finally
        {
            LanguageManager.ChangeLanguage(saved);
        }
    }

    private static void RunChecks()
    {
        LanguageManager.ChangeLanguage("et-EE", save: false);
        using (var db = new AppDbContext())
        {
            Require(!db.Database.HasPendingModelChanges(), "EF snapshot differs from the runtime model");
            string sql = db.GetService<IMigrator>().GenerateScript();
            Require(sql.Contains("ShopSettings"), "Migration script does not contain ShopSettings");
        }

        Directory.CreateDirectory("ui-checks");
        var products = TechCatalog.Reference().ToArray();
        using var store = new DemoShopStore(products, TechCatalog.InitialPromotions());
        using var form = new StoreForm(store);
        form.Show();
        PumpUntil(() => Descendants(form).OfType<ProductListItem>().Count() == 6);
        Require(Descendants(form).OfType<PictureBox>().All(p => p.Image != null), "A catalog product is missing its local photograph");
        using (var bitmap = new Bitmap(form.Width, form.Height))
        {
            form.DrawToBitmap(bitmap, form.ClientRectangle);
            bitmap.Save("ui-checks/home-et.png");
        }

        Descendants(form).OfType<Button>().Single(b => b.Text == UiText.Category("computers")).PerformClick();
        var gpu = Descendants(form).OfType<CheckedListBox>().Single(list => list.Items.Cast<object>().Any(i => i.ToString() == "RTX 4070 Laptop GPU"));
        gpu.SetItemChecked(Enumerable.Range(0, gpu.Items.Count).Single(i => gpu.Items[i].ToString() == "RTX 4070 Laptop GPU"), true);
        PumpUntil(() => Descendants(form).OfType<ProductListItem>().Count() == 1);
        Require(!Descendants(form).OfType<ComboBox>().Any(c => c.AccessibleName == "ET / RU / EN"), "Language selector is still visible");
        Require(Descendants(form).OfType<Label>().Any(l => l.Text.Contains("Videokaart")), "Estonian filter label missing");
        using (var bitmap = new Bitmap(form.Width, form.Height))
        {
            form.DrawToBitmap(bitmap, form.ClientRectangle);
            bitmap.Save("ui-checks/rtx4070-et.png");
        }

        var cart = new ShoppingCart();
        var selected = store.GetProducts().First(p => p.Details.SourceKey == "model:asus-rog-strix-g16-g614ji-as94");
        cart.Add(selected, 1);
        using (var details = new ProductDetailsForm(selected, cart, (_, _) => true))
        {
            details.Show();
            Application.DoEvents();
            details.Close();
        }

        using (var basket = new CartForm(cart, store))
        {
            basket.Show();
            Application.DoEvents();
            basket.Close();
        }

        var order = store.Checkout(cart.Lines);
        using (var confirmation = new OrderConfirmationForm(order))
        {
            confirmation.Show();
            Application.DoEvents();
            confirmation.Close();
        }

        using (var login = new LoginForm())
        {
            login.Show();
            Application.DoEvents();
            Require(!Descendants(login).OfType<ComboBox>().Any(c => c.AccessibleName == "ET / RU / EN"), "Login still has a language selector");
            login.Close();
        }

        using (var registration = new RegisterForm())
        {
            registration.Show();
            Application.DoEvents();
            registration.Close();
        }

        Require(cart.Count == 1, "Closing dialogs changed the basket");
        var config = new ShopConfiguration
        {
            Products = products.ToDictionary(p => p.Id, p => p.Details)
        };
        var oldPhone = products.Single(p => p.Details.SourceKey == "dummyjson:123");
        var badOffer = new Promotion
        {
            Title = new("Eestikeelne kampaania"),
            Category = "accessories",
            ProductKeys = oldPhone.Details.SourceKey,
            Percent = 15
        };
        var rows = products.Select(p => new Toode { Id = p.Id, Toodenimetus = p.Name }).ToArray();
        using (var editor = new PromotionEditorForm(badOffer, rows, config))
        {
            editor.Show();
            Application.DoEvents();
            Require(Descendants(editor).OfType<TextBox>().Count(t => t.Parent is FlowLayoutPanel) == 3, "Promotion editor still asks for translation fields");
            Require(!Descendants(editor).OfType<Label>().Any(l => l.Text.EndsWith(" · RU") || l.Text.EndsWith(" · EN")), "Foreign-language editor labels remain");
            var list = Descendants(editor).OfType<CheckedListBox>().Single(c => c.AccessibleName == "promotion-products");
            Require(!list.Items.Cast<object>().Any(p => p.ToString()!.Contains("iPhone")), "Accessories target list contains a phone");
            Require(list.CheckedItems.Count == 0, "Incompatible selection was retained");
            var category = Descendants(editor).OfType<ComboBox>().Single(c => c.AccessibleName == "promotion-category");
            category.SelectedIndex = 1;
            Application.DoEvents();
            int index = Enumerable.Range(0, list.Items.Count).Single(i => list.Items[i].ToString() == oldPhone.DisplayName);
            list.SetItemChecked(index, true);
            Application.DoEvents();
            typeof(PromotionEditorForm).GetMethod("Save", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(editor, null);
            Require(editor.Value.Category == "phones" && editor.Value.ProductKeys == oldPhone.Details.SourceKey, "Corrected promotion did not save its selected phone");
            editor.Close();
        }

        var paymentProduct = store.GetProducts().First(p => p.Stock > 2);
        var paymentCart = new ShoppingCart();
        paymentCart.Add(paymentProduct, 1);
        int sent = 0;
        using (var basket = new CartForm(paymentCart, store, _ =>
        {
            sent++;
            return Task.CompletedTask;
        }))
        {
            basket.Show();
            Application.DoEvents();
            var email = Descendants(basket).OfType<TextBox>().Single(t => t.AccessibleName == "order-email");
            email.Text = "test@example.com";
            Descendants(basket).OfType<Button>().Single(b => b.AccessibleName == "pay-order").PerformClick();
            PumpUntil(() => basket.Order != null);
            Require(sent == 1 && paymentCart.Count == 0 && basket.PaymentFlow.Result?.EmailSent == true, "One-click email checkout failed");
            using var success = new OrderConfirmationForm(basket.Order!, basket.PaymentFlow);
            success.Show();
            Application.DoEvents();
            Require(Descendants(success).OfType<Label>().Any(l => l.Text.Contains("umbes kahe nädala")), "Two-week delivery estimate is missing");
            Require(Descendants(success).OfType<Label>().Any(l => l.Text.Contains("test@example.com")), "Email delivery status is missing");
            using var image = new Bitmap(success.Width, success.Height);
            success.DrawToBitmap(image, success.ClientRectangle);
            image.Save("ui-checks/payment-success-et.png");
            success.Close();
        }

        using (var mailSettings = new MailSettingsForm())
        {
            mailSettings.Show();
            Application.DoEvents();
            Require(Descendants(mailSettings).OfType<TextBox>().Count(t => t.Parent is FlowLayoutPanel) == 2, "Mail settings should ask only for sender and app password");
            Require(!Descendants(mailSettings).OfType<NumericUpDown>().Any() && !Descendants(mailSettings).OfType<CheckBox>().Any(), "Technical port or TLS controls are still shown");
            Require(!Descendants(mailSettings).OfType<Label>().Any(l => l.Text.Contains("SMTP") || l.Text == "Port"), "Mail settings still show connection details");
            mailSettings.Close();
        }

        string temporarySettings = Path.Combine(Path.GetTempPath(), "shoping67-mail-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            const string dummy = "test-credential-not-a-real-password";
            new MailSettings
            {
                SenderEmail = "test@example.com",
                Password = dummy
            }.Save(temporarySettings);
            Require(!File.ReadAllText(temporarySettings).Contains(dummy), "Mail password was stored as plaintext");
            Require(MailSettings.Load(temporarySettings).Password == dummy, "Windows-protected mail settings cannot be read");
        }
        finally
        {
            if (File.Exists(temporarySettings))
                File.Delete(temporarySettings);
        }

        CheckLanguageSwitching(form, store, selected, config, rows);
        form.Close();
        LanguageManager.ChangeLanguage("et-EE", save: false);
        Console.WriteLine("PASS Windows rendering, GPU filter, RESX language switching, Settings persistence and EF migration model");
    }

    private static void CheckLanguageSwitching(StoreForm form, IShopStore store, ShopProduct selected, ShopConfiguration config, Toode[] productRows)
    {
        var language = Descendants(form).OfType<ComboBox>().Single(c => c.Name == "comboLanguage");
        var search = Descendants(form).OfType<TextBox>().Single(t => t.Name == "search");
        search.Text = "ASUS";
        var price = Descendants(form).OfType<NumericUpDown>().First();
        price.Value = 100;
        var cart = new ShoppingCart();
        cart.Add(selected, 1);
        using var basket = new CartForm(cart, store);
        basket.Show();
        Application.DoEvents();
        var email = Descendants(basket).OfType<TextBox>().Single(t => t.AccessibleName == "order-email");
        email.Text = "buyer@example.com";
        using var login = new LoginForm();
        login.Show();
        Application.DoEvents();
        var username = Descendants(login).OfType<TextBox>().Single(t => t.Name == "kasutajaEntry");
        username.Text = "unchanged-user";
        var offer = new Promotion
        {
            Title = new("Mu enda pealkiri"),
            Category = selected.Details.Category,
            ProductKeys = selected.Details.SourceKey,
            Percent = 15
        };
        using var editor = new PromotionEditorForm(offer, productRows, config);
        editor.Show();
        Application.DoEvents();
        var title = Descendants(editor).OfType<TextBox>().First(t => t.Parent is FlowLayoutPanel);
        title.Text = "Unsaved promotion";
        var percentage = Descendants(editor).OfType<NumericUpDown>().First();
        percentage.Value = 17.5m;
        var targets = Descendants(editor).OfType<CheckedListBox>().Single(t => t.AccessibleName == "promotion-products");
        int targetCount = targets.CheckedItems.Count;
        for (int i = 0; i < 3; i++)
        {
            language.SelectedIndex = 1;
            Application.DoEvents();
            Require(form.Visible && search.Text == "ASUS", "Language switching recreated the form or cleared search");
            Require(Descendants(form).OfType<NumericUpDown>().First().Value == 100, "Language switching reset price filters");
            Require(Descendants(form).OfType<ProductListItem>().Count() == 1, "Language switching reset the exact GPU filter");
            Require(Descendants(form).OfType<Label>().Any(l => l.Text == "Graphics card"), "Dynamic GPU label did not switch to English");
            Require(search.PlaceholderText.StartsWith("Search"), "Named form placeholder did not use English RESX");
            Require(login.Text == "Sign in" && Descendants(login).OfType<Button>().Any(b => b.Text == "Register"), "Designer form captions did not use English RESX");
            Require(username.Text == "unchanged-user" && email.Text == "buyer@example.com", "Language switching overwrote typed text");
            Require(cart.Count == 1 && Descendants(basket).OfType<Button>().Any(b => b.Text == "Pay"), "Language switching changed basket data or missed Pay");
            Require(percentage.Text == "17.50", "Numeric input did not use the English number format");
            Require(title.Text == "Unsaved promotion" && percentage.Value == 17.5m && targets.CheckedItems.Count == targetCount, "Language switching reset unsaved promotion edits or selected targets");
            Require(Properties.Settings.Default.UserLanguage == "en-US", "Language selection was not saved in user settings");
            using (var bitmap = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bitmap, form.ClientRectangle);
                bitmap.Save("ui-checks/resx-shop-en.png");
            }

            if (i == 0)
            {
                using var probe = Process.Start(new ProcessStartInfo(Environment.ProcessPath!, "--language-settings-check en-US") { UseShellExecute = false });
                Require(probe != null && probe.WaitForExit(15000) && probe.ExitCode == 0, "A fresh process did not load the saved language before startup");
                using var reopened = new LoginForm();
                reopened.Show();
                Application.DoEvents();
                Require(reopened.Text == "Sign in", "A newly opened form ignored the selected language");
                reopened.Close();
                using var settings = new MailSettingsForm();
                settings.Show();
                Application.DoEvents();
                Require(Descendants(settings).OfType<Label>().Any(l => l.Text == "Sender Gmail address"), "Mail settings did not use English resources");
                settings.Close();
            }

            language.SelectedIndex = 2;
            Application.DoEvents();
            Require(login.Text == "Вход" && Descendants(login).OfType<Button>().Any(button => button.Text == "Регистрация"), "Russian login resources are missing");
            Require(Descendants(form).OfType<Label>().Any(label => label.Text == "Видеокарта"), "Russian filter labels are missing");
            Require(Descendants(basket).OfType<Button>().Any(button => button.Text == "Оплатить") && cart.Count == 1, "Russian basket labels/data are incorrect");
            Require(username.Text == "unchanged-user" && email.Text == "buyer@example.com", "Russian switching overwrote input");
            Require(title.Text == "Unsaved promotion" && percentage.Value == 17.5m && targets.CheckedItems.Count == targetCount, "Russian switching lost unsaved editor data");
            Require(Properties.Settings.Default.UserLanguage == "ru-RU", "Russian preference was not saved");
            var loginLanguage = Descendants(login).OfType<ComboBox>().Single(combo => combo.Name == "comboLanguage");
            var loginTitle = Descendants(login).Single(control => control.Name == "pealkiriLabel");
            Require(loginLanguage.Top < loginTitle.Top && loginLanguage.Right > login.ClientSize.Width * 0.8, "Login language selector is not in the upper right corner");
            var usernameLabel = Descendants(login).Single(control => control.Name == "kasutajaLabel");
            Require(usernameLabel.Right < username.Left, "Russian username label overlaps the input");
            using (var bitmap = new Bitmap(login.Width, login.Height))
            {
                login.DrawToBitmap(bitmap, login.ClientRectangle);
                bitmap.Save("ui-checks/login-ru-upper-right.png");
            }
            using (var bitmap = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bitmap, form.ClientRectangle);
                bitmap.Save("ui-checks/store-ru.png");
            }
            if (i == 0)
            {
                using var probe = Process.Start(new ProcessStartInfo(Environment.ProcessPath!, "--language-settings-check ru-RU") { UseShellExecute = false });
                Require(probe != null && probe.WaitForExit(15000) && probe.ExitCode == 0, "A fresh process did not restore Russian");
                using var addProduct = new ProductEditorForm();
                addProduct.Show(); Application.DoEvents();
                Require(Descendants(addProduct).OfType<TextBox>().Any(), "Unified product creation editor did not open");
                addProduct.Close();
            }
            language.SelectedIndex = 0;
            Application.DoEvents();
            Require(percentage.Text == "17,50", "Numeric input did not return to the Estonian number format");
            Require(login.Text == "Logi sisse" && Descendants(form).OfType<Label>().Any(l => l.Text == "Videokaart"), "Switching back to neutral Estonian resources failed");
        }

        editor.Close();
        login.Close();
        basket.Close();
    }
}
