namespace Tooded_2.Shop;

internal sealed class ProductDetailsForm : Form
{
    public ProductDetailsForm(ShopProduct product, ShoppingCart cart, Func<ShopProduct, int, bool> addToCart)
    {
        Text = product.DisplayName; Size = new Size(1160, 730); MinimumSize = new Size(960, 620);
        StartPosition = FormStartPosition.CenterParent; BackColor = Color.White;
        Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(24) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var breadcrumb = ShopStyle.Label($"{T.Category("")}  ›  {product.DisplayCategory}  ›  {product.DisplayName}", 9);
        root.Controls.Add(breadcrumb, 0, 0); root.SetColumnSpan(breadcrumb, 3);
        var gallery = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        gallery.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        gallery.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        var picture = ShopImages.Picture(product.Image); picture.Cursor = Cursors.Hand;
        picture.Click += (_, _) =>
        {
            using var zoom = new Form { Text = product.DisplayName, Size = new Size(900, 720), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White };
            zoom.Controls.Add(ShopImages.Picture(product.Image)); zoom.ShowDialog(this);
        };
        gallery.Controls.Add(picture, 0, 0);
        gallery.Controls.Add(ShopStyle.Label(T.Legacy("Klõpsa pildil, et suurendada"), 9), 0, 1);
        root.Controls.Add(gallery, 0, 1);
        var description = ShopStyle.Stack(); description.Padding = new Padding(16, 0, 20, 0);
        var title = ShopStyle.Label(product.DisplayName, 20, true);
        var body = ShopStyle.Label(product.DisplayDescription, 11);
        description.Controls.Add(title);
        description.Controls.Add(ShopStyle.Label(product.DisplayCategory, 10));
        description.Controls.Add(ShopStyle.Label($"{product.Price:0.00} €", 25, true));
        if (product.OriginalPrice > product.Price)
        {
            var old = ShopStyle.Label(T.Money(product.OriginalPrice.Value), 12); old.Font = new Font(old.Font, FontStyle.Strikeout); description.Controls.Add(old);
        }
        if (product.Details.ReferenceConfiguration) description.Controls.Add(ShopStyle.Label(T.L("Näidiskonfiguratsioon · demohind", "Пример конфигурации · демоцена", "Reference configuration · demo price"), 10, true));
        description.Controls.Add(ShopStyle.Label(T.Legacy("Toote kirjeldus"), 12, true)); description.Controls.Add(body);
        description.Controls.Add(ShopStyle.Label(T.Legacy("Toote andmed"), 12, true));
        var specificationLabels = new List<Label>();
        var technical = new Dictionary<string, string>(product.Specifications);
        technical["brand"] = product.Details.Brand;
        technical["kind"] = product.Details.Kind;
        foreach (string key in CatalogRules.Facets(product.Details.Category))
            if (!technical.ContainsKey(key) || technical[key].Length == 0) technical[key] = T.L("Märkimata", "Не указано", "Not specified");
        foreach (var spec in technical.Prepend(new KeyValuePair<string, string>(T.Legacy("Tootekood"), product.Id.ToString())))
        {
            var label = ShopStyle.Label($"{T.Spec(spec.Key)}: {T.Value(spec.Value)}", 10); specificationLabels.Add(label); description.Controls.Add(label);
        }
        void WrapDescription()
        {
            int width = Math.Max(120, description.ClientSize.Width - description.Padding.Horizontal - 20);
            title.MaximumSize = new Size(width, 0); body.MaximumSize = new Size(width, 0);
            foreach (var label in specificationLabels) label.MaximumSize = new Size(width, 0);
        }
        description.SizeChanged += (_, _) => WrapDescription();
        root.Controls.Add(description, 1, 1);
        var purchase = ShopStyle.Stack(); purchase.BorderStyle = BorderStyle.FixedSingle; purchase.Padding = new Padding(16);
        purchase.Controls.Add(ShopStyle.Label($"{product.Price:0.00} €", 22, true));
        var availability = ShopStyle.Label("", 10, true); purchase.Controls.Add(availability);
        purchase.Controls.Add(ShopStyle.Label(T.Legacy("Kogus"), 10));
        var quantity = new NumericUpDown { Minimum = 1, Maximum = Math.Max(1, product.Stock), Value = 1, Width = 155, Margin = new Padding(0, 0, 0, 16) };
        purchase.Controls.Add(quantity);
        var add = ShopStyle.Button(T.Legacy("Lisa ostukorvi"), true); var buy = ShopStyle.Button(T.Legacy("Osta kohe"));
        var notice = ShopStyle.Label("", 10); notice.ForeColor = ShopStyle.Green;
        purchase.Controls.Add(add); purchase.Controls.Add(buy); purchase.Controls.Add(notice);
        var disclaimer = ShopStyle.Label(T.Legacy("Ainult demomakse. Päris raha ei võeta, kaardiandmeid ei küsita ja kaupu ei saadeta."), 10);
        purchase.Controls.Add(disclaimer);
        void Update()
        {
            int remaining = Math.Max(0, product.Stock - cart.Quantity(product.Id));
            availability.Text = remaining > 0 ? string.Format(T.L("Saadaval {0} tk", "В наличии {0} шт.", "{0} available"), remaining) : product.Stock > 0 ? T.Legacy("Kõik on ostukorvis") : T.Legacy("Hetkel otsas");
            availability.ForeColor = remaining > 0 ? ShopStyle.Green : ShopStyle.Muted;
            quantity.Maximum = Math.Max(1, remaining); quantity.Enabled = add.Enabled = buy.Enabled = remaining > 0;
        }
        void Add(bool checkout)
        {
            if (!addToCart(product, (int)quantity.Value)) return;
            Update(); notice.Text = T.Legacy("Lisatud ostukorvi");
            if (checkout) { DialogResult = DialogResult.OK; Close(); }
        }
        add.Click += (_, _) => Add(false); buy.Click += (_, _) => Add(true);
        purchase.SizeChanged += (_, _) =>
        {
            int width = Math.Max(130, purchase.ClientSize.Width - 36);
            add.Width = buy.Width = width; disclaimer.MaximumSize = availability.MaximumSize = notice.MaximumSize = new Size(width, 0);
        };
        root.Controls.Add(purchase, 2, 1); Controls.Add(root); Update();
    }
}
