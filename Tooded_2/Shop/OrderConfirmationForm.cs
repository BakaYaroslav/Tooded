using System.Text;

namespace Tooded_2.Shop;

internal sealed class OrderConfirmationForm : Form
{
    public OrderConfirmationForm(DemoOrder order)
    {
        Text = "Tellimus kinnitatud"; Size = new Size(660, 580); MinimumSize = new Size(580, 480);
        StartPosition = FormStartPosition.CenterParent; BackColor = Color.White; Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), RowCount = 4, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        var title = ShopStyle.Label("Aitäh! Demomakse õnnestus.", 22, true); title.ForeColor = ShopStyle.Green;
        root.Controls.Add(title, 0, 0); root.Controls.Add(ShopStyle.Label("Raha ei võetud. Tellimuse kinnitus on allpool.", 10), 0, 1);
        root.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Text = order.Receipt, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle }, 0, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
        var save = ShopStyle.Button("Salvesta kinnitus", true); save.Width = 210;
        save.Click += (_, _) =>
        {
            using var dialog = new SaveFileDialog { FileName = order.Number + ".txt", Filter = "Tekstifail (*.txt)|*.txt", DefaultExt = "txt" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try { File.WriteAllText(dialog.FileName, order.Receipt, Encoding.UTF8); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { MessageBox.Show(this, "Faili ei saanud salvestada. Vali teine asukoht.", "Salvestamine"); }
        };
        var close = ShopStyle.Button("Jätka ostlemist"); close.Click += (_, _) => Close();
        buttons.Controls.Add(save); buttons.Controls.Add(close); root.Controls.Add(buttons, 0, 3); Controls.Add(root);
    }
}
