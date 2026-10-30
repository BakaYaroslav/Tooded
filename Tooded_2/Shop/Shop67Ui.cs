using System.Drawing.Drawing2D;

namespace Tooded_2.Shop;

internal sealed class Shop67Logo : Control
{
    public Shop67Logo() { DoubleBuffered = true; Size = new Size(210, 66); ForeColor = Color.White; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var font = new Font("Segoe UI", 23, FontStyle.Bold);
        using var brush = new SolidBrush(ForeColor);
        e.Graphics.DrawString("shoping67", font, brush, 5, 0);
        using var pen = new Pen(Color.FromArgb(255, 166, 35), 4) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        e.Graphics.DrawArc(pen, 48, 24, 112, 28, 12, 151);
        e.Graphics.DrawLines(pen, new[] { new Point(151, 42), new Point(160, 39), new Point(158, 49) });
    }
}

internal static class Shop67Ui
{
    public static void Translate(Control root)
    {
        if (root is not TextBoxBase && root is not ComboBox && root is not NumericUpDown)
            root.Text = T.Legacy(T.Canonical(root.Text));
        if (root is DataGridView grid)
            foreach (DataGridViewColumn column in grid.Columns) column.HeaderText = T.Legacy(T.Canonical(column.HeaderText));
        foreach (Control child in root.Controls) Translate(child);
    }

    public static Button Button(string text, Action click, bool primary = false)
    { var button = ShopStyle.Button(text, primary); button.Click += (_, _) => click(); return button; }
    public static Label Label(string text, int size = 10, bool bold = false) => ShopStyle.Label(text, size, bold);
}
