namespace Tooded_2.Shop;

internal sealed class CartForm : Form
{
    private readonly ShoppingCart cart;
    private readonly IShopStore store;
    private readonly FlowLayoutPanel rows = ShopStyle.Stack();
    private readonly Label total = ShopStyle.Label("", 16, true);
    private readonly Button checkout = ShopStyle.Button("Vormista tellimus", true);
    private readonly Button clear = ShopStyle.Button("Tühjenda korv");
    public DemoOrder? Order { get; private set; }

    public CartForm(ShoppingCart cart, IShopStore store)
    {
        this.cart = cart; this.store = store;
        Text = "Ostukorv"; Size = new Size(920, 690); MinimumSize = new Size(850, 550);
        Font = new Font("Segoe UI", 10); BackColor = Color.White; AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterParent;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(20) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.Controls.Add(ShopStyle.Label("Sinu ostukorv", 23, true), 0, 0);
        rows.Padding = new Padding(0); root.Controls.Add(rows, 0, 1);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        bottom.Controls.Add(total);
        var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        checkout.Width = 220;
        var back = ShopStyle.Button("Jätka ostlemist"); back.Click += (_, _) => Close();
        clear.Click += (_, _) => { cart.Clear(); Render(); };
        checkout.Click += (_, _) =>
        {
            using var payment = new DemoCheckoutForm(cart.Lines, store);
            if (payment.ShowDialog(this) != DialogResult.OK) return;
            Order = payment.Order; cart.Clear(); DialogResult = DialogResult.OK; Close();
        };
        buttons.Controls.AddRange(new Control[] { checkout, back, clear }); bottom.Controls.Add(buttons);
        root.Controls.Add(bottom, 0, 2); Controls.Add(root);
        rows.SizeChanged += (_, _) => ResizeRows(); Render();
    }

    private void ResizeRows()
    {
        foreach (Control row in rows.Controls) row.Width = Math.Max(700, rows.ClientSize.Width - 24);
    }

    private void Render()
    {
        rows.SuspendLayout(); ShopStyle.Clear(rows);
        if (cart.Count == 0)
        {
            var empty = ShopStyle.Label("Ostukorv on tühi. Leia kataloogist midagi meelepärast.", 13);
            rows.Controls.Add(empty);
        }
        foreach (var line in cart.Lines)
        {
            var row = new TableLayoutPanel { Height = 128, Width = 720, ColumnCount = 5, RowCount = 1, Margin = new Padding(0, 0, 0, 12), Padding = new Padding(8), BackColor = Color.FromArgb(248, 249, 250) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            row.Controls.Add(ShopImages.Picture(line.Product.Image), 0, 0);
            var text = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            text.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); text.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            var name = ShopStyle.Label(line.Product.Name, 11, true); name.AutoSize = false; name.Dock = DockStyle.Fill;
            text.Controls.Add(name, 0, 0); text.Controls.Add(ShopStyle.Label($"{line.Product.Price:0.00} € / tk", 10), 0, 1);
            row.Controls.Add(text, 1, 0);
            var quantity = new NumericUpDown { Minimum = 1, Maximum = Math.Max(line.Quantity, line.Product.Stock), Value = line.Quantity, Width = 65, Anchor = AnchorStyles.None, AccessibleName = $"{line.Product.Name}: kogus" };
            var subtotal = ShopStyle.Label($"{line.Total:0.00} €", 12, true); subtotal.Anchor = AnchorStyles.None;
            quantity.ValueChanged += (_, _) =>
            {
                cart.SetQuantity(line.Product, (int)quantity.Value);
                subtotal.Text = $"{line.Product.Price * quantity.Value:0.00} €"; UpdateTotal();
            };
            var remove = ShopStyle.Button("Eemalda"); remove.Width = 95; remove.Anchor = AnchorStyles.None;
            remove.Click += (_, _) => { cart.Remove(line.Product.Id); Render(); };
            row.Controls.Add(quantity, 2, 0); row.Controls.Add(subtotal, 3, 0); row.Controls.Add(remove, 4, 0);
            rows.Controls.Add(row);
        }
        ResizeRows(); UpdateTotal(); rows.ResumeLayout();
    }

    private void UpdateTotal()
    {
        total.Text = $"Kokku ({cart.Count} tk): {cart.Total:0.00} €";
        checkout.Enabled = clear.Enabled = cart.Count > 0;
    }
}
