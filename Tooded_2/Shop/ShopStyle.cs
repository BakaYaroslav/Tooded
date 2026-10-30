namespace Tooded_2.Shop;

internal static class ShopStyle
{
    public static readonly Color Navy = Color.FromArgb(19, 31, 46);
    public static readonly Color Yellow = Color.FromArgb(255, 216, 20);
    public static readonly Color Ink = Color.FromArgb(24, 34, 45);
    public static readonly Color Muted = Color.FromArgb(92, 105, 117);
    public static readonly Color Green = Color.FromArgb(0, 117, 76);
    public static readonly Color Line = Color.FromArgb(225, 229, 232);

    public static Label Label(string text, float size = 10, bool bold = false) => new()
    {
        Text = text, AutoSize = true, ForeColor = Ink,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
        Margin = new Padding(0, 0, 0, 12)
    };

    public static Button Button(string text, bool primary = false)
    {
        var button = new Button
        {
            Text = text, AutoSize = false, Size = new Size(180, 38), FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Yellow : Color.White, ForeColor = Ink,
            Font = new Font("Segoe UI", 10, FontStyle.Regular), Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 10), UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = primary ? Yellow : Line;
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    public static FlowLayoutPanel Stack() => new()
    {
        FlowDirection = FlowDirection.TopDown, WrapContents = false,
        Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20), BackColor = Color.White
    };

    public static void Clear(Control parent)
    {
        foreach (Control child in parent.Controls.Cast<Control>().ToArray()) child.Dispose();
    }
}
