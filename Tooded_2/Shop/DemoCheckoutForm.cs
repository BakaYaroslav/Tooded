namespace Tooded_2.Shop;

internal sealed class DemoCheckoutForm : Form
{
    public DemoOrder? Order { get; private set; }
    public DemoCheckoutForm(IReadOnlyList<CartLine> lines, IShopStore store)
    {
        Text = "Tellimuse vormistamine · Demomakse"; Size = new Size(620, 620);
        MinimumSize = new Size(570, 560); StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10); BackColor = Color.White; AutoScaleMode = AutoScaleMode.Dpi;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(24) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        root.Controls.Add(ShopStyle.Label("Vaata tellimus üle", 23, true), 0, 0);
        var summary = new TextBox
        {
            Multiline = true, ReadOnly = true, BorderStyle = BorderStyle.None, Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 249, 250), ScrollBars = ScrollBars.Vertical,
            Text = string.Join(Environment.NewLine + Environment.NewLine, lines.Select(l => $"{l.Product.Name}\r\n{l.Quantity} × {l.Product.Price:0.00} € = {l.Total:0.00} €"))
                + $"\r\n\r\nKOKKU: {lines.Sum(l => l.Total):0.00} €"
        };
        root.Controls.Add(summary, 0, 1);
        var notice = new Label
        {
            Dock = DockStyle.Fill, Text = "Makseviis: demomakse\nPäris raha ei võeta ja kaupu ei saadeta. Demotellimus vähendab laoseisu" +
                (store.IsDemoCatalog ? " selle sessiooni jooksul." : " ja kajastub müügiaruandes."),
            Padding = new Padding(0, 12, 0, 0), ForeColor = ShopStyle.Muted
        };
        root.Controls.Add(notice, 0, 2);
        var agree = new CheckBox { Text = "Kinnitan, et tegemist on demotellimusega", Dock = DockStyle.Fill, AutoSize = true };
        root.Controls.Add(agree, 0, 3);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var pay = ShopStyle.Button("Kinnita demomakse", true); pay.Width = 250; pay.Enabled = false;
        var cancel = ShopStyle.Button("Tagasi korvi"); cancel.Click += (_, _) => Close();
        agree.CheckedChanged += (_, _) => pay.Enabled = agree.Checked;
        pay.Click += (_, _) =>
        {
            pay.Enabled = cancel.Enabled = agree.Enabled = false; UseWaitCursor = true;
            try
            {
                Order = store.Checkout(lines); DialogResult = DialogResult.OK; Close();
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Tellimust ei kinnitatud. Kontrolli laoseisu ja hindu, uuenda kataloogi ning proovi uuesti. Ostukorv on alles.",
                    "Tellimus ei õnnestunud", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                pay.Enabled = agree.Checked; cancel.Enabled = agree.Enabled = true;
            }
            finally { UseWaitCursor = false; }
        };
        actions.Controls.Add(pay); actions.Controls.Add(cancel); root.Controls.Add(actions, 0, 4); Controls.Add(root);
        CancelButton = cancel;
    }
}
