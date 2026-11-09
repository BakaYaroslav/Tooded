using System.Drawing.Drawing2D;

namespace Tooded_2;
internal sealed class ShopLogo : Control
{
    public ShopLogo()
    {
        DoubleBuffered = true;
        Size = new Size(210, 66);
        ForeColor = Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var font = new Font("Segoe UI", 23, FontStyle.Bold);
        using var brush = new SolidBrush(ForeColor);
        e.Graphics.DrawString("shoping67", font, brush, 5, 0);
        using var pen = new Pen(Color.FromArgb(255, 166, 35), 4)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        e.Graphics.DrawArc(pen, 48, 24, 112, 28, 12, 151);
        e.Graphics.DrawLines(pen, new[] { new Point(151, 42), new Point(160, 39), new Point(158, 49) });
    }
}
