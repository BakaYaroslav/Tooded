namespace Tooded_2;
internal sealed class OrderConfirmationForm : LocalizedForm
{
    public OrderConfirmationForm(ShopOrder order, CheckoutFlow? payment = null)
    {
        LanguageManager.Bind(this, "Text", () => UiText.Get("PaymentSuccessfulShoping67"));
        Size = new Size(780, 680);
        MinimumSize = new Size(710, 580);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            RowCount = 5,
            ColumnCount = 1
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        var title = ShopUi.Label(() => UiText.Get("PaymentSuccessful"), 24, true);
        title.ForeColor = ShopUi.Green;
        root.Controls.Add(title, 0, 0);
        root.Controls.Add(LanguageManager.Bind(new Label { Dock = DockStyle.Fill }, "Text", () => UiText.Format("OrderConfirmationDetails", order.Number, order.ExpectedDelivery)), 0, 1);
        root.Controls.Add(LanguageManager.Bind(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.White }, "Text", () => order.Receipt), 0, 2);
        var delivery = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 12, 0, 0),
            AccessibleName = "email-delivery-status"
        };
        void UpdateEmail()
        {
            LanguageManager.Bind(delivery, "Text", () => payment?.Result?.EmailStatus ?? UiText.Get("YourOrderConfirmationIsBelow"));
            delivery.ForeColor = payment?.Result?.EmailSent == true ? ShopUi.Green : ShopUi.Muted;
        }

        UpdateEmail();
        root.Controls.Add(delivery, 0, 3);
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false
        };
        var save = ShopUi.Button(() => UiText.Get("SavePDFReceipt"));
        save.Width = 190;
        save.Click += (_, _) =>
        {
            using var dialog = new SaveFileDialog
            {
                FileName = OrderPdf.FileName(order),
                Filter = UiText.Get("PDFFilePdfPdf"),
                DefaultExt = "pdf"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            try
            {
                File.WriteAllBytes(dialog.FileName, OrderPdf.Create(order));
            }
            catch (Exception ex)when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, UiText.Get("FileSaveFailed"), UiText.Get("Saving"));
            }
        };
        if (payment != null && payment.Result?.EmailSent != true)
        {
            var retry = ShopUi.Button(() => UiText.Get("ResendEmail"), true);
            retry.Width = 230;
            retry.Click += async (_, _) =>
            {
                retry.Enabled = false;
                LanguageManager.Bind(delivery, "Text", () => UiText.Get("SendingEmail"));
                try
                {
                    await payment.RetryEmailAsync();
                }
                finally
                {
                    if (!IsDisposed)
                    {
                        UpdateEmail();
                        retry.Visible = payment.Result?.EmailSent != true;
                        retry.Enabled = true;
                    }
                }
            };
            buttons.Controls.Add(retry);
        }

        buttons.Controls.Add(save);
        var close = ShopUi.Button(() => UiText.Get("ContinueShopping"));
        close.Click += (_, _) => Close();
        buttons.Controls.Add(close);
        root.Controls.Add(buttons, 0, 4);
        Controls.Add(root);
    }
}
