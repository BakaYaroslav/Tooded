namespace Tooded_2.Shop;

internal sealed class CartForm : LocalizedForm
{
    private readonly ShoppingCart cart;
    private readonly FlowLayoutPanel rows = ShopStyle.Stack();
    private readonly Label total = ShopStyle.Label(()=>"", 16, true);
    private readonly Button checkout = ShopStyle.Button(()=>UiText.Get("Pay"), true);
    private readonly TextBox email = new() { Width = 530, PlaceholderText = "sinu@epost.ee", AccessibleName = "order-email" };
    private readonly Label progress = ShopStyle.Label(()=>"", 10);
    private FlowLayoutPanel buttons = null!;
    public CheckoutFlow PaymentFlow { get; }

    private readonly Button clear = ShopStyle.Button(()=>UiText.Get("EmptyBasket"));
    public DemoOrder? Order { get; private set; }

    public CartForm(ShoppingCart cart, IShopStore store, Func<DemoOrder, Task>? sendEmail = null)
    {
        this.cart = cart;
        PaymentFlow = new CheckoutFlow(store, sendEmail ?? (order => EpostiSaatja.SaadaTellimuseKinnitusAsync(order)));
        LanguageManager.Bind(this, "Text", () => UiText.Get("Basket")); Size = new Size(920, 760); MinimumSize = new Size(850, 640);
        Font = new Font("Segoe UI", 10); BackColor = Color.White; AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterParent;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(20) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
        root.Controls.Add(ShopStyle.Label(()=>UiText.Get("YourBasket"), 23, true), 0, 0);
        rows.Padding = new Padding(0); root.Controls.Add(rows, 0, 1);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        bottom.Controls.Add(total);
        var emailRow = new FlowLayoutPanel { Width = 820, Height = 45, WrapContents = false };
        emailRow.Controls.Add(ShopStyle.Label(()=>UiText.Get("YourEmail"), 11, true)); emailRow.Controls.Add(email);
        bottom.Controls.Add(emailRow);
        var notice = ShopStyle.Label(()=>UiText.Get("DemoPaymentNoMoneyIsChargedTheOrderConfirmationIsSentByEmail"), 10);
        notice.MaximumSize = new Size(800, 0); bottom.Controls.Add(notice);
        buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        checkout.Name = "checkoutButton"; checkout.Width = 220; checkout.AccessibleName = "pay-order";
        var back = ShopStyle.Button(()=>UiText.Get("ContinueShopping")); back.Click += (_, _) => Close();
        clear.Click += (_, _) => { cart.Clear(); Render(); };
        checkout.Click += async (_, _) => await Pay();
        buttons.Controls.AddRange(new Control[] { checkout, back, clear }); bottom.Controls.Add(buttons); bottom.Controls.Add(progress);
        root.Controls.Add(bottom, 0, 2); Controls.Add(root);
        rows.SizeChanged += (_, _) => ResizeRows(); Render();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (PaymentFlow.InProgress) { e.Cancel = true; return; }
        base.OnFormClosing(e);
    }

    private async Task Pay()
    {
        if (PaymentFlow.InProgress) return;
        if (!OrderEmail.ValidAddress(email.Text))
        { MessageBox.Show(this, UiText.Get("EnterAValidEmailAddress"), "shoping67"); email.Focus(); return; }
        rows.Enabled = buttons.Enabled = email.Enabled = false; UseWaitCursor = true;
        LanguageManager.Bind(progress, "Text", () => UiText.Get("ConfirmingYourOrderAndSendingEmail"));
        try
        {
            var result = await PaymentFlow.PayAsync(cart, email.Text);
            Order = result.Order; DialogResult = DialogResult.OK; Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex is InvalidOperationException ? ex.Message : UiText.Get("UnableToConfirmYourOrderCheckTheConnectionAndTryAgainYourBasketI"), "shoping67", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            LanguageManager.Bind(progress, "Text", () => UiText.Get("TheOrderWasNotConfirmedYourBasketIsRetained"));
        }
        finally { if (!IsDisposed) { UseWaitCursor = false; rows.Enabled = buttons.Enabled = email.Enabled = true; UpdateTotal(); } }
    }

    protected override void RefreshLocalizedValues() => Render();

    private void ResizeRows()
    {
        foreach (Control row in rows.Controls) row.Width = Math.Max(700, rows.ClientSize.Width - 24);
    }

    private void Render()
    {
        rows.SuspendLayout(); ShopStyle.Clear(rows);
        if (cart.Count == 0)
        {
            var empty = ShopStyle.Label(()=>UiText.Get("YourBasketIsEmptyFindSomethingYouLikeInTheCatalog"), 13);
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
            var name = ShopStyle.Label(()=>line.Product.DisplayName, 11, true); name.AutoSize = false; name.Dock = DockStyle.Fill;
            text.Controls.Add(name, 0, 0); text.Controls.Add(ShopStyle.Label(()=>$"{line.Product.Price:0.00} € / " + UiText.Get("Item") + "", 10), 0, 1);
            row.Controls.Add(text, 1, 0);
            var quantity = new NumericUpDown { Minimum = 1, Maximum = Math.Max(line.Quantity, line.Product.Stock), Value = line.Quantity, Width = 65, Anchor = AnchorStyles.None, AccessibleName = $"{line.Product.DisplayName}: {UiText.Get("Quantity_E89F2B")}" };
            var subtotal = ShopStyle.Label(()=>$"{line.Total:0.00} €", 12, true); subtotal.Anchor = AnchorStyles.None;
            quantity.ValueChanged += (_, _) =>
            {
                cart.SetQuantity(line.Product, (int)quantity.Value);
                subtotal.Text = $"{line.Product.Price * quantity.Value:0.00} €"; UpdateTotal();
            };
            var remove = ShopStyle.Button(()=>UiText.Get("Remove")); remove.Width = 95; remove.Anchor = AnchorStyles.None;
            remove.Click += (_, _) => { cart.Remove(line.Product.Id); Render(); };
            row.Controls.Add(quantity, 2, 0); row.Controls.Add(subtotal, 3, 0); row.Controls.Add(remove, 4, 0);
            rows.Controls.Add(row);
        }
        ResizeRows(); UpdateTotal(); rows.ResumeLayout();
    }

    private void UpdateTotal()
    {
        LanguageManager.Bind(total, "Text", () => string.Format(UiText.Get("Total0Items1"), cart.Count, UiText.Money(cart.Total)));
        checkout.Enabled = clear.Enabled = cart.Count > 0;
    }
}
