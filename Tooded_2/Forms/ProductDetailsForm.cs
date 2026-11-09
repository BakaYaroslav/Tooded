namespace Tooded_2;
internal sealed class ProductDetailsForm : LocalizedForm
{
    public ProductDetailsForm(ShopProduct product, ShoppingCart cart, Func<ShopProduct, int, bool> addToCart)
    {
        LanguageManager.Bind(this, "Text", () => product.DisplayName);
        Size = new Size(1160, 730);
        MinimumSize = new Size(960, 620);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(24)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var breadcrumb = ShopUi.Label(() => $"{UiText.Category("")}  ›  {product.DisplayCategory}  ›  {product.DisplayName}", 9);
        root.Controls.Add(breadcrumb, 0, 0);
        root.SetColumnSpan(breadcrumb, 3);
        var gallery = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        gallery.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        gallery.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        var picture = ShopImages.Picture(product.Image);
        picture.Cursor = Cursors.Hand;
        picture.Click += (_, _) =>
        {
            using var zoom = LanguageManager.Bind(new Form { Size = new Size(900, 720), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White }, "Text", () => product.DisplayName);
            zoom.Controls.Add(ShopImages.Picture(product.Image));
            zoom.ShowDialog(this);
        };
        gallery.Controls.Add(picture, 0, 0);
        gallery.Controls.Add(ShopUi.Label(() => UiText.Get("ClickTheImageToEnlarge"), 9), 0, 1);
        root.Controls.Add(gallery, 0, 1);
        var description = ShopUi.Stack();
        description.Padding = new Padding(16, 0, 20, 0);
        var title = ShopUi.Label(() => product.DisplayName, 20, true);
        var body = ShopUi.Label(() => product.DisplayDescription, 11);
        description.Controls.Add(title);
        description.Controls.Add(ShopUi.Label(() => product.DisplayCategory, 10));
        description.Controls.Add(ShopUi.Label(() => $"{product.Price:0.00} €", 25, true));
        if (product.OriginalPrice > product.Price)
        {
            var old = ShopUi.Label(() => UiText.Money(product.OriginalPrice.Value), 12);
            old.Font = new Font(old.Font, FontStyle.Strikeout);
            description.Controls.Add(old);
        }

        if (product.Details.ReferenceConfiguration)
            description.Controls.Add(ShopUi.Label(() => UiText.Get("ReferenceConfigurationDemoPrice"), 10, true));
        description.Controls.Add(ShopUi.Label(() => UiText.Get("ProductDescription"), 12, true));
        description.Controls.Add(body);
        description.Controls.Add(ShopUi.Label(() => UiText.Get("ProductDetails"), 12, true));
        var specificationLabels = new List<Label>();
        var technical = new Dictionary<string, string>(product.Specifications);
        technical["brand"] = product.Details.Brand;
        technical["kind"] = product.Details.Kind;
        foreach (string key in CatalogRules.Facets(product.Details.Category))
            if (!technical.ContainsKey(key) || technical[key].Length == 0)
                technical[key] = "";
        foreach (var spec in technical.Prepend(new KeyValuePair<string, string>("productCode", product.Id.ToString())))
        {
            var label = ShopUi.Label(() => $"{UiText.Spec(spec.Key)}: {(spec.Value.Length == 0 ? UiText.Get("NotSpecified") : UiText.Value(spec.Value))}", 10);
            specificationLabels.Add(label);
            description.Controls.Add(label);
        }

        void WrapDescription()
        {
            int width = Math.Max(120, description.ClientSize.Width - description.Padding.Horizontal - 20);
            title.MaximumSize = new Size(width, 0);
            body.MaximumSize = new Size(width, 0);
            foreach (var label in specificationLabels)
                label.MaximumSize = new Size(width, 0);
        }

        description.SizeChanged += (_, _) => WrapDescription();
        root.Controls.Add(description, 1, 1);
        var purchase = ShopUi.Stack();
        purchase.BorderStyle = BorderStyle.FixedSingle;
        purchase.Padding = new Padding(16);
        purchase.Controls.Add(ShopUi.Label(() => $"{product.Price:0.00} €", 22, true));
        var availability = ShopUi.Label(() => "", 10, true);
        purchase.Controls.Add(availability);
        purchase.Controls.Add(ShopUi.Label(() => UiText.Get("Quantity"), 10));
        var quantity = new NumericUpDown
        {
            Minimum = 1,
            Maximum = Math.Max(1, product.Stock),
            Value = 1,
            Width = 155,
            Margin = new Padding(0, 0, 0, 16)
        };
        purchase.Controls.Add(quantity);
        var add = ShopUi.Button(() => UiText.Get("AddToBasket"), true);
        var buy = ShopUi.Button(() => UiText.Get("BuyNow"));
        var notice = ShopUi.Label(() => "", 10);
        notice.ForeColor = ShopUi.Green;
        purchase.Controls.Add(add);
        purchase.Controls.Add(buy);
        purchase.Controls.Add(notice);
        var disclaimer = ShopUi.Label(() => UiText.Get("DemoPaymentNotice"), 10);
        purchase.Controls.Add(disclaimer);
        void Update()
        {
            int remaining = Math.Max(0, product.Stock - cart.Quantity(product.Id));
            LanguageManager.Bind(availability, "Text", () => remaining > 0 ? string.Format(UiText.Get("0Available"), remaining) : product.Stock > 0 ? UiText.Get("AllAvailableItemsAreInYourBasket") : UiText.Get("CurrentlyOutOfStock"));
            availability.ForeColor = remaining > 0 ? ShopUi.Green : ShopUi.Muted;
            quantity.Maximum = Math.Max(1, remaining);
            quantity.Enabled = add.Enabled = buy.Enabled = remaining > 0;
        }

        void Add(bool checkout)
        {
            if (!addToCart(product, (int)quantity.Value))
                return;
            Update();
            LanguageManager.Bind(notice, "Text", () => UiText.Get("AddedToBasket"));
            if (checkout)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        add.Click += (_, _) => Add(false);
        buy.Click += (_, _) => Add(true);
        purchase.SizeChanged += (_, _) =>
        {
            int width = Math.Max(130, purchase.ClientSize.Width - 36);
            add.Width = buy.Width = width;
            disclaimer.MaximumSize = availability.MaximumSize = notice.MaximumSize = new Size(width, 0);
        };
        root.Controls.Add(purchase, 2, 1);
        Controls.Add(root);
        Update();
    }
}
