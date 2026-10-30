
namespace Tooded_2.Shop;

internal sealed class OrderConfirmationForm : Form
{
    public OrderConfirmationForm(DemoOrder order, CheckoutFlow? payment = null)
    {
        Text = "Makse õnnestus · shoping67"; Size = new Size(780, 680); MinimumSize = new Size(710, 580);
        StartPosition = FormStartPosition.CenterParent; BackColor = Color.White; Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), RowCount = 5, ColumnCount = 1 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        var title = ShopStyle.Label("Makse õnnestus!", 24, true); title.ForeColor = ShopStyle.Green;
        root.Controls.Add(title, 0, 0);
        root.Controls.Add(new Label { Dock = DockStyle.Fill, Text = $"Tellimus {order.Number} on vastu võetud ja töös.\nEeldatav tarne: {order.ExpectedDelivery:dd.MM.yyyy} — umbes kahe nädala pärast.\nNäidistellimus: päris raha ei võeta ja kaupa ei saadeta." }, 0, 1);
        root.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Text = order.Receipt, BackColor = Color.White }, 0, 2);
        var delivery = new Label { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 0), AccessibleName = "email-delivery-status" };
        void UpdateEmail()
        {
            delivery.Text = payment?.Result?.EmailStatus ?? "Tellimuse kinnitus on allpool.";
            delivery.ForeColor = payment?.Result?.EmailSent == true ? ShopStyle.Green : ShopStyle.Muted;
        }
        UpdateEmail(); root.Controls.Add(delivery, 0, 3);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var save = ShopStyle.Button("Salvesta PDF-tšekk"); save.Width = 190;
        save.Click += (_, _) =>
        {
            using var dialog = new SaveFileDialog { FileName = OrderPdf.FileName(order), Filter = "PDF-fail (*.pdf)|*.pdf", DefaultExt = "pdf" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try { File.WriteAllBytes(dialog.FileName, OrderPdf.Create(order)); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { MessageBox.Show(this, "Faili ei saanud salvestada. Vali teine asukoht.", "Salvestamine"); }
        };
        if (payment != null && payment.Result?.EmailSent != true)
        {
            var retry = ShopStyle.Button("Saada e-kiri uuesti", true); retry.Width = 230;
            retry.Click += async (_, _) =>
            {
                retry.Enabled = false; delivery.Text = "E-kirja saatmine…";
                try { await payment.RetryEmailAsync(); }
                finally
                {
                    if (!IsDisposed) { UpdateEmail(); retry.Visible = payment.Result?.EmailSent != true; retry.Enabled = true; }
                }
            };
            buttons.Controls.Add(retry);
        }
        buttons.Controls.Add(save);
        var close = ShopStyle.Button("Jätka ostlemist"); close.Click += (_, _) => Close(); buttons.Controls.Add(close);
        root.Controls.Add(buttons, 0, 4); Controls.Add(root);
    }
}
