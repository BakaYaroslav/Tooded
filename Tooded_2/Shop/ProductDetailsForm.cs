namespace Tooded_2.Shop;

internal sealed class ProductDetailsForm : Form
{
    public ProductDetailsForm(ShopProduct product, ShoppingCart cart, Func<ShopProduct, int, bool> addToCart)
    {
        Text = product.Name; Size = new Size(1160, 730); MinimumSize = new Size(960, 620);
        StartPosition = FormStartPosition.CenterParent; BackColor = Color.White;
        Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(24) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var breadcrumb = ShopStyle.Label($"Tooted  ›  {product.Category}  ›  {product.Name}", 9);
        root.Controls.Add(breadcrumb, 0, 0); root.SetColumnSpan(breadcrumb, 3);
        var gallery = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        gallery.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        gallery.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        var picture = ShopImages.Picture(product.Image); picture.Cursor = Cursors.Hand;
        picture.Click += (_, _) =>
        {
            using var zoom = new Form { Text = product.Name, Size = new Size(900, 720), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White };
            zoom.Controls.Add(ShopImages.Picture(product.Image)); zoom.ShowDialog(this);
        };
        gallery.Controls.Add(picture, 0, 0);
        gallery.Controls.Add(ShopStyle.Label("Klõpsa pildil, et suurendada", 9), 0, 1);
        root.Controls.Add(gallery, 0, 1);
        var description = ShopStyle.Stack(); description.Padding = new Padding(16, 0, 20, 0);
        var title = ShopStyle.Label(product.Name, 20, true);
        var body = ShopStyle.Label(product.Description, 11);
        description.Controls.Add(title);
        description.Controls.Add(ShopStyle.Label(product.Category, 10));
        description.Controls.Add(ShopStyle.Label($"{product.Price:0.00} €", 25, true));
        description.Controls.Add(ShopStyle.Label("Toote kirjeldus", 12, true)); description.Controls.Add(body);
        description.Controls.Add(ShopStyle.Label("Toote andmed", 12, true));
        var specificationLabels = new List<Label>();
        foreach (var spec in product.Specifications.Prepend(new KeyValuePair<string, string>("Tootekood", product.Id.ToString())))
        {
            var label = ShopStyle.Label($"{spec.Key}: {spec.Value}", 10); specificationLabels.Add(label); description.Controls.Add(label);
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
        purchase.Controls.Add(ShopStyle.Label("Kogus", 10));
        var quantity = new NumericUpDown { Minimum = 1, Maximum = Math.Max(1, product.Stock), Value = 1, Width = 155, Margin = new Padding(0, 0, 0, 16) };
        purchase.Controls.Add(quantity);
        var add = ShopStyle.Button("Lisa ostukorvi", true); var buy = ShopStyle.Button("Osta kohe");
        var notice = ShopStyle.Label("", 10); notice.ForeColor = ShopStyle.Green;
        purchase.Controls.Add(add); purchase.Controls.Add(buy); purchase.Controls.Add(notice);
        var disclaimer = ShopStyle.Label("Ainult demomakse. Päris raha ei võeta, kaardiandmeid ei küsita ja kaupu ei saadeta.", 10);
        purchase.Controls.Add(disclaimer);
        void Update()
        {
            int remaining = Math.Max(0, product.Stock - cart.Quantity(product.Id));
            availability.Text = remaining > 0 ? $"Saadaval {remaining} tk" : product.Stock > 0 ? "Kõik on ostukorvis" : "Hetkel otsas";
            availability.ForeColor = remaining > 0 ? ShopStyle.Green : ShopStyle.Muted;
            quantity.Maximum = Math.Max(1, remaining); quantity.Enabled = add.Enabled = buy.Enabled = remaining > 0;
        }
        void Add(bool checkout)
        {
            if (!addToCart(product, (int)quantity.Value)) return;
            Update(); notice.Text = "Lisatud ostukorvi";
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
