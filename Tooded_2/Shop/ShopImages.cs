namespace Tooded_2.Shop;

internal static class ShopImages
{
    public static Image? Load(string? file)
    {
        if (string.IsNullOrWhiteSpace(file)) return null;
        string relative = file.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        var paths = new List<string>();
        if (Path.IsPathRooted(relative)) paths.Add(relative);
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            paths.Add(Path.Combine(dir.FullName, relative));
            paths.Add(Path.Combine(dir.FullName, "Images", relative));
            paths.Add(Path.Combine(dir.FullName, "Images", Path.GetFileName(relative)));
        }
        foreach (var path in paths.Distinct())
        {
            if (!File.Exists(path)) continue;
            try
            {
                using var stream = File.OpenRead(path);
                using var original = Image.FromStream(stream);
                return new Bitmap(original);
            }
            catch (Exception ex) when (ex is IOException or ArgumentException or OutOfMemoryException) { }
        }
        return null;
    }

    public static PictureBox Picture(string? file)
    {
        var picture = new PictureBox
        {
            Image = Load(file), SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 249, 250), Margin = new Padding(0, 0, 20, 0)
        };
        picture.Paint += (_, e) =>
        {
            if (picture.Image == null)
                TextRenderer.DrawText(e.Graphics, "Pilt puudub", SystemFonts.MessageBoxFont, picture.ClientRectangle,
                    ShopStyle.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        picture.Disposed += (_, _) => picture.Image?.Dispose();
        return picture;
    }
}
