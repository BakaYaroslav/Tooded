namespace Tooded_2.Shop;

internal sealed class ProductListItem : Panel
{
    private readonly Button add;
    private readonly Label stock;
    private readonly ShopProduct product;
    public ProductListItem(ShopProduct product, Action open, Action addToCart)
    {
        this.product = product;
        Height = 256; Margin = new Padding(0, 0, 0, 12); BackColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var picture = ShopImages.Picture(product.Image);
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (int height in new[] { 30, 48, 44, 30, 48 }) content.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        var category = ShopStyle.Label(product.DisplayCategory + (product.Details.ReferenceConfiguration ? T.L(" · Näidiskonfiguratsioon", " · Пример конфигурации", " · Reference configuration") : ""), 9); category.ForeColor = ShopStyle.Muted;
        var title = ShopStyle.Label(product.DisplayName, 15, true); title.AutoSize = false; title.Dock = DockStyle.Fill;
        var price = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
        price.Controls.Add(ShopStyle.Label(T.Money(product.Price), 21, true));
        if (product.OriginalPrice > product.Price)
        {
            var old = ShopStyle.Label(T.Money(product.OriginalPrice.Value), 11); old.Font = new Font(old.Font, FontStyle.Strikeout); old.ForeColor = ShopStyle.Muted; price.Controls.Add(old);
        }
        stock = ShopStyle.Label("", 10);
        add = ShopStyle.Button(T.Legacy("Lisa ostukorvi"), true); add.Width = 200;
        add.Click += (_, _) => addToCart();
        content.Controls.Add(category, 0, 0); content.Controls.Add(title, 0, 1);
        content.Controls.Add(price, 0, 2); content.Controls.Add(stock, 0, 3); content.Controls.Add(add, 0, 4);
        layout.Controls.Add(picture, 0, 0); layout.Controls.Add(content, 1, 0); Controls.Add(layout);
        foreach (var control in new Control[] { this, layout, content, picture, title, category, price, stock })
        {
            control.Cursor = Cursors.Hand;
            control.Click += (_, _) => open();
        }
        UpdateAvailability(0);
    }

    public void UpdateAvailability(int inCart)
    {
        int remaining = Math.Max(0, product.Stock - inCart);
        stock.Text = product.Stock == 0 ? T.Legacy("Hetkel otsas") : string.Format(T.L("Laos {0} tk · Korvis {1}", "В наличии {0} шт. · В корзине {1}", "In stock: {0} · In basket: {1}"), product.Stock, inCart);
        stock.ForeColor = remaining > 0 ? ShopStyle.Green : ShopStyle.Muted;
        add.Enabled = remaining > 0;
        add.Text = product.Stock == 0 ? T.Legacy("Otsas") : remaining == 0 ? T.Legacy("Kõik on korvis") : T.Legacy("Lisa ostukorvi");
    }
}
